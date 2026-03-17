using System.Net.Http.Json;
using VinhKhanhTour.Shared.Models;

namespace VinhKhanhTour.CMS.Services;

public class CmsApiService
{
    private readonly HttpClient _http;

    public CmsApiService(HttpClient http) => _http = http;

    public Task<List<PoiModel>?> GetPoisAsync()
        => _http.GetFromJsonAsync<List<PoiModel>>("api/pois");

    public Task<HttpResponseMessage> SavePoiAsync(PoiModel poi)
        => _http.PostAsJsonAsync("api/pois", poi);

    public Task<HttpResponseMessage> DeletePoiAsync(string id)
        => _http.DeleteAsync($"api/pois/{id}");

    public async Task<string?> UploadAudioAsync(
    string poiId, string lang, Stream stream, string fileName)
    {
        var form = new MultipartFormDataContent();
        var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/mpeg");
        form.Add(content, "file", fileName);

        var resp = await _http.PostAsync($"api/pois/{poiId}/audio/{lang}", form);
        if (!resp.IsSuccessStatusCode) return null;

        var result = await resp.Content
            .ReadFromJsonAsync<Dictionary<string, string>>();
        return result?["url"];
    }

    // ── History ───────────────────────────────────────
    public Task<List<AppHistory>?> GetHistoryAsync()
        => _http.GetFromJsonAsync<List<AppHistory>>("api/history");

    // ── Tours ─────────────────────────────────────────
    public Task<List<TourModel>?> GetToursAsync()
        => _http.GetFromJsonAsync<List<TourModel>>("api/tours");

    public Task<HttpResponseMessage> SaveTourAsync(TourModel tour)
        => _http.PostAsJsonAsync("api/tours", tour);

    public Task<HttpResponseMessage> DeleteTourAsync(string id)
        => _http.DeleteAsync($"api/tours/{id}");

    // ── Analytics ─────────────────────────────────────
    public Task<List<AnalyticsEvent>?> GetAnalyticsAsync()
        => _http.GetFromJsonAsync<List<AnalyticsEvent>>("api/analytics");

    // ── Location Trace ────────────────────────────────
    public Task<List<LocationTrace>?> GetTracesAsync()
        => _http.GetFromJsonAsync<List<LocationTrace>>("api/trace");
}