using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using VinhKhanhTour.Shared.Models;

namespace VinhKhanhTour.Api.Services;

public class FirestoreService
{
    private readonly FirestoreDb _db;
    const string PROJECT_ID = "vinhkhanhtour-c8e3f";

    public FirestoreService()
    {
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
        var snap = await _db.Collection("pois").GetSnapshotAsync();
        return snap.Documents
            .Select(d => d.ConvertTo<PoiModel>())
            .ToList();
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
        return poi.Id;
    }

    public async Task DeletePoiAsync(string id)
        => await _db.Collection("pois").Document(id).DeleteAsync();

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
        }
        catch (Exception ex)
        {
            Console.WriteLine("🔥 FIRESTORE ERROR: " + ex.ToString());
            throw;
        }
    }

    // ── App History ───────────────────────────────────
    public async Task<List<AppHistory>> GetHistoryAsync(int limit = 100)
    {
        var snap = await _db.Collection("history")
            .OrderByDescending("Timestamp")
            .Limit(limit)
            .GetSnapshotAsync();
        return snap.Documents
            .Select(d => d.ConvertTo<AppHistory>())
            .ToList();
    }

    public async Task LogHistoryAsync(AppHistory history)
    {
        history.Id = Guid.NewGuid().ToString("N")[..8];
        await _db.Collection("history").Document(history.Id).SetAsync(history);
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
    public async Task IncrementQrScansAsync(string tourId)
    {
        var docRef = _db.Collection("tours").Document(tourId);
        await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "QrScans", FieldValue.Increment(1) }
        });
    }

    // ── Analytics ─────────────────────────────────────
    public async Task<List<AnalyticsEvent>> GetAnalyticsAsync()
    {
        var snap = await _db.Collection("analytics")
            .OrderByDescending("Timestamp")
            .Limit(500)
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
        var snap = await _db.Collection("traces")
            .Limit(1000)
            .GetSnapshotAsync();
        return snap.Documents
            .Select(d => d.ConvertTo<LocationTrace>())
            .ToList();
    }
}