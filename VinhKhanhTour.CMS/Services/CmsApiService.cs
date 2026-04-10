using System.Net.Http.Json;
using System.Text.Json.Serialization;
using VinhKhanhTour.Shared.Models;

namespace VinhKhanhTour.CMS.Services;

public class CmsApiService
{
    private readonly HttpClient _http;
    
    // ── Cache Storage ──
    private List<PoiModel>? _poisCache;
    private List<AppHistory>? _historyCache;
    private List<AnalyticsEvent>? _analyticsCache;
    private List<LocationTrace>? _tracesCache;
    private DateTime _lastRefresh = DateTime.MinValue;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public CmsApiService(HttpClient http) => _http = http;

    private bool IsCacheValid() => DateTime.UtcNow - _lastRefresh < _cacheDuration;

    // ── POI ───────────────────────────────────────────
    public async Task<List<PoiModel>?> GetPoisAsync(bool force = false)
    {
        if (!force && _poisCache != null && IsCacheValid()) return _poisCache;
        _poisCache = await _http.GetFromJsonAsync<List<PoiModel>>("api/pois?admin=true");
        _lastRefresh = DateTime.UtcNow;
        return _poisCache;
    }

    public async Task<string?> SavePoiAsync(PoiModel poi)
    {
        var resp = await _http.PostAsJsonAsync("api/pois", poi);
        if (resp.IsSuccessStatusCode) 
        {
            _poisCache = null; 
            var result = await resp.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            return result?["id"];
        }
        return null;
    }

    public async Task<HttpResponseMessage> DeletePoiAsync(string id)
    {
        var resp = await _http.DeleteAsync($"api/pois/{id}");
        if (resp.IsSuccessStatusCode) _poisCache = null; // Clear cache on change
        return resp;
    }

    // ── Audio ─────────────────────────────────────────
    public async Task<string?> UploadAudioAsync(
        string poiId, string lang, Stream stream, string fileName)
    {
        var form = new MultipartFormDataContent();
        var content = new StreamContent(stream);
        content.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("audio/mpeg");
        form.Add(content, "file", fileName);

        var resp = await _http.PostAsync($"api/pois/{poiId}/audio/{lang}", form);
        if (!resp.IsSuccessStatusCode) return null;

        var result = await resp.Content
            .ReadFromJsonAsync<Dictionary<string, string>>();
        return result?["url"];
    }

    public async Task<string?> UploadImageAsync(Stream stream, string fileName)
    {
        var form = new MultipartFormDataContent();
        var content = new StreamContent(stream);
        content.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(content, "file", fileName);

        var resp = await _http.PostAsync("api/tours/upload-image", form);
        if (!resp.IsSuccessStatusCode) return null;

        var result = await resp.Content
            .ReadFromJsonAsync<Dictionary<string, string>>();
        
        var url = result?["url"];
        if (!string.IsNullOrEmpty(url) && url.StartsWith("/"))
        {
            return _http.BaseAddress?.ToString().TrimEnd('/') + url;
        }

        return url;
    }

    // ── History ───────────────────────────────────────
    public async Task<List<AppHistory>?> GetHistoryAsync(bool force = false)
    {
        if (!force && _historyCache != null && IsCacheValid()) return _historyCache;
        _historyCache = await _http.GetFromJsonAsync<List<AppHistory>>("api/history?limit=2000");
        return _historyCache;
    }

    // ── Tours ─────────────────────────────────────────
    public Task<List<TourModel>?> GetToursAsync()
        => _http.GetFromJsonAsync<List<TourModel>>("api/tours");

    public Task<HttpResponseMessage> SaveTourAsync(TourModel tour)
        => _http.PostAsJsonAsync("api/tours", tour);

    public Task<HttpResponseMessage> DeleteTourAsync(string id)
        => _http.DeleteAsync($"api/tours/{id}");

    public Task<HttpResponseMessage> IncrementScanAsync(string tourId)
        => _http.PostAsync($"api/tours/{tourId}/scan", null);

    // ── Analytics ─────────────────────────────────────
    public async Task<List<AnalyticsEvent>?> GetAnalyticsAsync(bool force = false)
    {
        if (!force && _analyticsCache != null && IsCacheValid()) return _analyticsCache;
        _analyticsCache = await _http.GetFromJsonAsync<List<AnalyticsEvent>>("api/analytics?limit=2000");
        return _analyticsCache;
    }

    // ── Location Trace ────────────────────────────────
    public async Task<List<LocationTrace>?> GetTracesAsync(bool force = false)
    {
        if (!force && _tracesCache != null && IsCacheValid()) return _tracesCache;
        _tracesCache = await _http.GetFromJsonAsync<List<LocationTrace>>("api/trace?limit=2000");
        return _tracesCache;
    }

    // ── Translate (Gemini AI) ─────────────────────────
    // Chỉ gọi khi user bấm nút, KHÔNG tự động
    public async Task<TranslateResult?> TranslateAsync(string viText)
    {
        var resp = await _http.PostAsJsonAsync("api/translate",
            new { text = viText });

        if (!resp.IsSuccessStatusCode) return null;

        return await resp.Content.ReadFromJsonAsync<TranslateResult>();
    }

    // ── Visit/Pay ──────────────────────────────────
    public async Task<bool> CheckAccessAsync(string poiId, string deviceId)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<AccessResult>($"api/pois/{poiId}/check-access?deviceId={deviceId}");
            return result?.Paid ?? false;
        }
        catch { return false; }
    }

    public Task<HttpResponseMessage> PayAsync(string poiId, string deviceId, string lang = "vi")
        => _http.PostAsJsonAsync($"api/pois/{poiId}/pay", new { Device = deviceId, Language = lang });

    // ── Users Management ───────────────────────────
    public Task<List<AppUser>?> GetUsersAsync()
        => _http.GetFromJsonAsync<List<AppUser>>("api/users");

    public Task<HttpResponseMessage> SaveUserAsync(AppUser user)
        => _http.PostAsJsonAsync("api/users", user);

    public Task<HttpResponseMessage> DeleteUserAsync(string id)
        => _http.DeleteAsync($"api/users/{id}");

    public Task<HttpResponseMessage> RegisterMerchantAsync(MerchantRegisterRequest req)
        => _http.PostAsJsonAsync("api/users/register-merchant", req);

    public async Task<AppUser?> GetProfileAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<AppUser>("api/users/profile");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔥 GetProfile Error: {ex.Message}");
            return null;
        }
    }

} // end of CmsApiService class

// ── Access result model
public class AccessResult
{
    [JsonPropertyName("paid")] public bool Paid { get; set; }
}

// ── Model kết quả dịch từ Gemini ─────────────────────────────────────
public class TranslateResult
{
    [JsonPropertyName("en")] public string En { get; set; } = "";
    [JsonPropertyName("zh")] public string Zh { get; set; } = "";
    [JsonPropertyName("ja")] public string Ja { get; set; } = "";
    [JsonPropertyName("ko")] public string Ko { get; set; } = "";
}