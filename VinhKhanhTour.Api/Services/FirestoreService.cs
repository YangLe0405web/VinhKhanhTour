using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using VinhKhanhTour.Shared.Models;
using Microsoft.Extensions.Caching.Memory;

namespace VinhKhanhTour.Api.Services;

public class FirestoreService
{
    private readonly FirestoreDb _db;
    private readonly IMemoryCache _cache;
    const string PROJECT_ID = "vinhkhanhtour-c8e3f";

    // Cache Keys
    private const string CACHE_POIS = "pois_list";
    private const string CACHE_ANALYTICS = "analytics_list";
    private const string CACHE_HISTORY = "history_list";
    private const string CACHE_TRACES = "traces_list";

    public FirestoreService(IMemoryCache cache)
    {
        _cache = cache;

        var keyPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
        if (!string.IsNullOrEmpty(keyPath) && File.Exists(keyPath))
        {
            var keyJson = File.ReadAllText(keyPath);
            var credential = GoogleCredential.FromJson(keyJson)
                .CreateScoped("https://www.googleapis.com/auth/datastore");

            var client = new FirestoreClientBuilder
            {
                ChannelCredentials = credential.ToChannelCredentials()
            }.Build();

            _db = FirestoreDb.Create(PROJECT_ID, client);
        }
        else
        {
            _db = FirestoreDb.Create(PROJECT_ID);
        }
    }

    // ── POI ──────────────────────────────────────────
    public async Task<List<PoiModel>> GetAllPoisAsync()
    {
        return await _cache.GetOrCreateAsync(CACHE_POIS, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            var snap = await _db.Collection("pois").GetSnapshotAsync();
            return snap.Documents
                .Select(d => d.ConvertTo<PoiModel>())
                .ToList();
        }) ?? new();
    }

    public async Task<string> SavePoiAsync(PoiModel poi)
    {
        poi.UpdatedAt = DateTime.UtcNow;
        if (string.IsNullOrEmpty(poi.Id))
        {
            poi.Id = Guid.NewGuid().ToString("N")[..8];
            poi.CreatedAt = DateTime.UtcNow;
        }
        await _db.Collection("pois").Document(poi.Id).SetAsync(poi);
        
        // Invalidate cache
        _cache.Remove(CACHE_POIS);
        return poi.Id;
    }

    public async Task DeletePoiAsync(string id)
    {
        await _db.Collection("pois").Document(id).DeleteAsync();
        _cache.Remove(CACHE_POIS);
    }

    // ── Analytics ─────────────────────────────────────
    public async Task LogEventAsync(AnalyticsEvent ev)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                { "EventType", ev.EventType ?? "" },
                { "PoiId", ev.PoiId ?? "" },
                { "Language", ev.Language ?? "vi" },
                { "Duration", ev.Duration },
                { "Lat", Math.Round(ev.Lat, 3) },
                { "Lng", Math.Round(ev.Lng, 3) },
                { "Timestamp", Timestamp.GetCurrentTimestamp() }
            };

            await _db.Collection("analytics").AddAsync(data);
            
            // Invalidate analytics cache for real-time feedback
            _cache.Remove(CACHE_ANALYTICS);
        }
        catch (Exception ex)
        {
            Console.WriteLine("🔥 FIRESTORE ERROR: " + ex.ToString());
            throw;
        }
    }

    public async Task<List<AnalyticsEvent>> GetAnalyticsAsync()
    {
        return await _cache.GetOrCreateAsync(CACHE_ANALYTICS, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            var snap = await _db.Collection("analytics")
                .OrderByDescending("Timestamp")
                .Limit(2000)
                .GetSnapshotAsync();

            return snap.Documents.Select(d =>
            {
                var ev = new AnalyticsEvent
                {
                    EventType = d.ContainsField("EventType") ? d.GetValue<string>("EventType") : "",
                    PoiId = d.ContainsField("PoiId") ? d.GetValue<string>("PoiId") : "",
                    Language = d.ContainsField("Language") ? d.GetValue<string>("Language") : "vi",
                    Duration = d.ContainsField("Duration") ? d.GetValue<int>("Duration") : 0,
                    Lat = d.ContainsField("Lat") ? d.GetValue<double>("Lat") : 0,
                    Lng = d.ContainsField("Lng") ? d.GetValue<double>("Lng") : 0,
                };

                if (d.ContainsField("Timestamp"))
                {
                    var ts = d.GetValue<Google.Cloud.Firestore.Timestamp>("Timestamp");
                    ev.Timestamp = ts.ToDateTime();
                }

                return ev;
            }).ToList();
        }) ?? new();
    }

    // ── App History ───────────────────────────────────
    public async Task<List<AppHistory>> GetHistoryAsync(int limit = 2000)
    {
        // History cache key includes limit to be safe
        var key = $"{CACHE_HISTORY}_{limit}";
        return await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            var snap = await _db.Collection("history")
                .OrderByDescending("Timestamp")
                .Limit(limit)
                .GetSnapshotAsync();
            return snap.Documents
                .Select(d => d.ConvertTo<AppHistory>())
                .ToList();
        }) ?? new();
    }

    public async Task LogHistoryAsync(AppHistory history)
    {
        history.Id = Guid.NewGuid().ToString("N")[..8];
        await _db.Collection("history").Document(history.Id).SetAsync(history);
        
        // Invalidate history cache
        _cache.Remove(CACHE_HISTORY);
        _cache.Remove($"{CACHE_HISTORY}_2000");
    }

    public async Task<bool> CheckPoiAccessAsync(string deviceId, string poiId)
    {
        if (string.IsNullOrWhiteSpace(poiId) || poiId.Contains("{") || poiId.Contains("}")) 
            return false;

        // 1. Kiểm tra nếu POI không yêu cầu thanh toán
        var poiDoc = await _db.Collection("pois").Document(poiId).GetSnapshotAsync();
        if (poiDoc.Exists)
        {
            var poi = poiDoc.ConvertTo<PoiModel>();
            if (!poi.RequirePayment) return true;
        }

        // 2. Tìm lịch sử thanh toán cho thiết bị này (lọc bằng code C# để tránh lỗi thiếu Index trên Firestore)
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var query = _db.Collection("history")
            .WhereEqualTo("Action", "pay_audio")
            .WhereEqualTo("Device", deviceId);
            
        var snap = await query.GetSnapshotAsync();

        return snap.Documents.Any(d => {
            if (!d.Exists) return false;
            var history = d.ConvertTo<AppHistory>();
            return history.PoiId == poiId && history.Timestamp >= cutoff;
        });
    }

    // ── Location Trace ────────────────────────────────
    public async Task LogTraceAsync(LocationTrace trace)
    {
        trace.Lat = Math.Round(trace.Lat, 3);
        trace.Lng = Math.Round(trace.Lng, 3);
        await _db.Collection("traces").AddAsync(trace);
    }

    public async Task<List<LocationTrace>> GetTracesAsync()
    {
        return await _cache.GetOrCreateAsync(CACHE_TRACES, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            var snap = await _db.Collection("traces")
                .OrderByDescending("Timestamp") // Thêm Sort để lấy cái mới nhất trước
                .Limit(500) // Giảm còn 500
                .GetSnapshotAsync();
            return snap.Documents
                .Select(d => d.ConvertTo<LocationTrace>())
                .ToList();
        }) ?? new();
    }

    // ── Tour ──────────────────────────────────────────
    public async Task<List<TourModel>> GetAllToursAsync()
    {
        var snap = await _db.Collection("tours").GetSnapshotAsync();
        return snap.Documents
            .Select(d => d.ConvertTo<TourModel>())
            .ToList();
    }

    public async Task<string> SaveTourAsync(TourModel tour)
    {
        if (string.IsNullOrEmpty(tour.Id))
        {
            tour.Id = Guid.NewGuid().ToString("N")[..8];
            tour.CreatedAt = DateTime.UtcNow;
        }
        await _db.Collection("tours").Document(tour.Id).SetAsync(tour);
        return tour.Id;
    }

    public async Task DeleteTourAsync(string id)
        => await _db.Collection("tours").Document(id).DeleteAsync();

    // ── QR Scan Counter ───────────────────────────────
    public async Task IncrementQrScansAsync(string tourId, string device = "Mobile", string lang = "vi")
    {
        var docRef = _db.Collection("tours").Document(tourId);
        var snap = await docRef.GetSnapshotAsync();
        var tourName = snap.ContainsField("Name") ? snap.GetValue<string>("Name") : "Tour " + tourId;

        await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "QrScans", FieldValue.Increment(1) }
        });

        // 1. Log to History (Visible in CMS History Page)
        var history = new AppHistory
        {
            Action = "scan_qr",
            PoiId = tourId,
            PoiName = tourName,
            Device = device,
            Language = lang,
            Timestamp = DateTime.UtcNow
        };
        await LogHistoryAsync(history);

        // 2. Log to Analytics (Counted as 'Scan QR' in Dashboard)
        var ev = new AnalyticsEvent
        {
            EventType = "scan_qr",
            PoiId = tourId,
            Language = lang,
            Timestamp = DateTime.UtcNow
        };
        await LogEventAsync(ev);
    }

    // ── User Management & Auth ────────────────────────
    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        var snap = await _db.Collection("users")
            .WhereEqualTo("Username", username)
            .Limit(1)
            .GetSnapshotAsync();

        return snap.Documents.FirstOrDefault()?.ConvertTo<AppUser>();
    }

    public async Task<AppUser?> GetUserByEmailAsync(string email)
    {
        var snap = await _db.Collection("users")
            .WhereEqualTo("Email", email)
            .Limit(1)
            .GetSnapshotAsync();

        return snap.Documents.FirstOrDefault()?.ConvertTo<AppUser>();
    }

    public async Task<List<AppUser>> GetAllUsersAsync()
    {
        var snap = await _db.Collection("users").GetSnapshotAsync();
        return snap.Documents.Select(d => d.ConvertTo<AppUser>()).ToList();
    }

    public async Task<string> SaveUserAsync(AppUser user)
    {
        if (string.IsNullOrEmpty(user.Id)) user.Id = Guid.NewGuid().ToString();
        await _db.Collection("users").Document(user.Id).SetAsync(user);
        return user.Id;
    }

    public async Task DeleteUserAsync(string id)
    {
        await _db.Collection("users").Document(id).DeleteAsync();
    }

    public async Task InitializeAdminAsync()
    {
        var admin = await GetUserByUsernameAsync("admin");
        if (admin == null)
        {
            var newAdmin = new AppUser
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FullName = "System Admin",
                Role = "admin"
            };
            await SaveUserAsync(newAdmin);
            Console.WriteLine("✅ Initialized default admin account: admin / admin123");
        }
    }
}