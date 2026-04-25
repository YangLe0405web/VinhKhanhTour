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

    // Xóa toàn bộ cache — gọi khi CMS bấm TẢI LẠI (force=true)
    public void ClearAllCache()
    {
        _cache.Remove(CACHE_POIS);
        _cache.Remove(CACHE_ANALYTICS);
        _cache.Remove(CACHE_HISTORY);
        // Xóa tất cả variant cache keys có thể
        foreach (var limit in new[] { 50, 100, 200, 500, 1000, 2000 })
        {
            _cache.Remove($"{CACHE_HISTORY}_{limit}");
        }
        _cache.Remove(CACHE_TRACES);
    }

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
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
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

    private const string CACHE_STATS = "global_stats";

    // ── Global Stats (Siêu tiết kiệm & Chính xác) ──────────
    public async Task<Dictionary<string, long>> GetGlobalStatsAsync(bool force = false)
    {
        if (force) _cache.Remove(CACHE_STATS);
        return await _cache.GetOrCreateAsync(CACHE_STATS, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            var doc = await _db.Collection("stats").Document("global").GetSnapshotAsync();
            if (!doc.Exists) return new Dictionary<string, long> { { "TotalPlay", 0 }, { "TotalQr", 0 } };
            return new Dictionary<string, long> 
            { 
                { "TotalPlay", doc.ContainsField("TotalPlay") ? doc.GetValue<long>("TotalPlay") : 0 },
                { "TotalQr", doc.ContainsField("TotalQr") ? doc.GetValue<long>("TotalQr") : 0 }
            };
        }) ?? new();
    }

    private async Task IncrementStatAsync(string field)
    {
        var docRef = _db.Collection("stats").Document("global");
        await docRef.SetAsync(new Dictionary<string, object> { { field, FieldValue.Increment(1) } }, SetOptions.MergeAll);
    }

    // ── Analytics ─────────────────────────────────────
    public async Task LogEventAsync(AnalyticsEvent ev)
    {
        try
        {
            ev.Timestamp = DateTime.UtcNow;
            var data = new Dictionary<string, object>
            {
                { "DeviceId", ev.DeviceId ?? "" },
                { "EventType", ev.EventType ?? "" },
                { "PoiId", ev.PoiId ?? "" },
                { "Language", ev.Language ?? "vi" },
                { "Duration", ev.Duration },
                { "Lat", ev.Lat },
                { "Lng", ev.Lng },
                { "Timestamp", Timestamp.GetCurrentTimestamp() }
            };

            await _db.Collection("analytics").AddAsync(data);

            // Tăng counter tổng
            if (ev.EventType == "scan_qr" || ev.EventType == "tour_scan") await IncrementStatAsync("TotalQr");
            else if (ev.EventType == "play_audio" || ev.EventType == "poi_play") await IncrementStatAsync("TotalPlay");
            
            // Write-through: thêm vào cache NGAY (0 reads thêm)
            if (_cache.TryGetValue(CACHE_ANALYTICS, out List<AnalyticsEvent>? cached) && cached != null)
            {
                cached.Insert(0, ev);
            }
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
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            var snap = await _db.Collection("analytics")
                .OrderByDescending("Timestamp")
                .Limit(500) // Giảm xuống 500 cho tiết kiệm vì đã có Stats tổng riêng
                .GetSnapshotAsync();

            return snap.Documents.Select(d =>
            {
                var ev = new AnalyticsEvent
                {
                    DeviceId = d.ContainsField("DeviceId") ? d.GetValue<string>("DeviceId") : "",
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
        var key = $"{CACHE_HISTORY}_{limit}";
        return await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            var snap = await _db.Collection("history")
                .OrderByDescending("Timestamp")
                .Limit(500) // Giảm xuống 500 cho tiết kiệm
                .GetSnapshotAsync();
            return snap.Documents
                .Select(d => d.ConvertTo<AppHistory>())
                .ToList();
        }) ?? new();
    }

    public async Task LogHistoryAsync(AppHistory history)
    {
        history.Id = Guid.NewGuid().ToString("N")[..8];
        history.Timestamp = DateTime.UtcNow;
        await _db.Collection("history").Document(history.Id).SetAsync(history);

        // Tăng counter tổng
        if (history.Action == "scan_qr" || history.Action == "tour_scan") await IncrementStatAsync("TotalQr");
        else if (history.Action == "play_audio" || history.Action == "poi_play") await IncrementStatAsync("TotalPlay");
        
        // Write-through: thêm vào TẤT CẢ history cache variants (0 reads thêm)
        foreach (var limit in new[] { 50, 100, 200, 500, 1000, 2000 })
        {
            var key = $"{CACHE_HISTORY}_{limit}";
            if (_cache.TryGetValue(key, out List<AppHistory>? cached) && cached != null)
            {
                cached.Insert(0, history);
            }
        }
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
        // Đã xóa Math.Round để đạt độ chính xác cao nhất
        await _db.Collection("traces").AddAsync(trace);
    }

    public async Task<List<LocationTrace>> GetTracesAsync()
    {
        return await _cache.GetOrCreateAsync(CACHE_TRACES, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            var snap = await _db.Collection("traces")
                .OrderByDescending("Timestamp")
                .Limit(300)
                .GetSnapshotAsync();
            return snap.Documents
                .Select(d => d.ConvertTo<LocationTrace>())
                .ToList();
        }) ?? new();
    }

    public async Task<List<LocationTrace>> GetAllTracesAsync(string? days = null)
    {
        var traces = new List<LocationTrace>();
        Query query = _db.Collection("traces");

        // Hỗ trợ lọc theo bộ lọc của Dashboard (Hôm nay, 7, 30 ngày)
        if (!string.IsNullOrEmpty(days) && days != "all")
        {
            int d = days switch { "today" => 0, "7" => 7, "30" => 30, _ => -1 };
            if (d >= 0)
            {
                var cutoff = DateTime.UtcNow.AddDays(-d).Date;
                query = query.WhereGreaterThanOrEqualTo("Timestamp", cutoff.ToString("o"));
            }
        }

        var snap = await query.GetSnapshotAsync();
        foreach (var doc in snap.Documents)
        {
            var t = doc.ConvertTo<LocationTrace>();
            traces.Add(t);
        }
        return traces;
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
            DeviceId = device,
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