using System.Net.Http.Json;
using System.Text.Json.Serialization;
using VinhKhanhTour.Shared.Models;

namespace VinhKhanhTour.CMS.Services;

public class CmsApiService
{
    private readonly HttpClient _http;

    public CmsApiService(HttpClient http) => _http = http;

    // ── POI ───────────────────────────────────────────
    public Task<List<PoiModel>?> GetPoisAsync()
        => _http.GetFromJsonAsync<List<PoiModel>>("api/pois?admin=true");

    public Task<HttpResponseMessage> SavePoiAsync(PoiModel poi)
        => _http.PostAsJsonAsync("api/pois", poi);

    public Task<HttpResponseMessage> DeletePoiAsync(string id)
        => _http.DeleteAsync($"api/pois/{id}");

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
    public Task<List<AppHistory>?> GetHistoryAsync()
        => _http.GetFromJsonAsync<List<AppHistory>>("api/history?limit=2000");

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
    public Task<List<AnalyticsEvent>?> GetAnalyticsAsync()
        => _http.GetFromJsonAsync<List<AnalyticsEvent>>("api/analytics?limit=2000");

    // ── Location Trace ────────────────────────────────
    public Task<List<LocationTrace>?> GetTracesAsync()
        => _http.GetFromJsonAsync<List<LocationTrace>>("api/trace?limit=2000");

    // ── Translate (Gemini AI) ─────────────────────────
    // Chỉ gọi khi user bấm nút, KHÔNG tự động
    public async Task<TranslateResult?> TranslateAsync(string viText)
    {
        var resp = await _http.PostAsJsonAsync("api/translate",
            new { text = viText });

        if (!resp.IsSuccessStatusCode) return null;

        return await resp.Content.ReadFromJsonAsync<TranslateResult>();
    }
}

// ── Model kết quả dịch từ Gemini ─────────────────────────────────────
public class TranslateResult
{
    [JsonPropertyName("en")] public string En { get; set; } = "";
    [JsonPropertyName("zh")] public string Zh { get; set; } = "";
    [JsonPropertyName("ja")] public string Ja { get; set; } = "";
    [JsonPropertyName("ko")] public string Ko { get; set; } = "";
}