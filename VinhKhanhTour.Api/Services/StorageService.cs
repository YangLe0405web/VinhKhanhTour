using System.Net.Http.Headers;

namespace VinhKhanhTour.Api.Services;

public class StorageService
{
    private const string CLOUD_NAME = "denzxxuw4";
    private const string API_KEY = "162781952147593";
    private const string API_SECRET = "3IbFP7kQAIOBBEqzdgDy_7VoJZk";

    private readonly HttpClient _http = new();

    public async Task<string> UploadAudioAsync(
        Stream fileStream, string poiId, string lang, string contentType)
    {
        // Tạo public_id để dễ quản lý: audio/poi_01/vi
        var publicId = $"audio/{poiId}/{lang}";

        // Tạo signature cho Cloudinary API
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var sigString = $"public_id={publicId}&timestamp={timestamp}{API_SECRET}";
        var signature = ComputeSha1(sigString);

        // Upload lên Cloudinary
        var url = $"https://api.cloudinary.com/v1_1/{CLOUD_NAME}/raw/upload";

        using var form = new MultipartFormDataContent();

        // File content
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        form.Add(fileContent, "file", $"{lang}.mp3");

        form.Add(new StringContent(publicId), "public_id");
        form.Add(new StringContent(timestamp), "timestamp");
        form.Add(new StringContent(API_KEY), "api_key");
        form.Add(new StringContent(signature), "signature");
        form.Add(new StringContent("true"), "overwrite");
        form.Add(new StringContent("audio"), "resource_type");

        var resp = await _http.PostAsync(url, form);
        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"Cloudinary upload failed: {body}");

        // Parse secure_url từ response JSON
        using var doc = System.Text.Json.JsonDocument.Parse(body);
        var secureUrl = doc.RootElement
            .GetProperty("secure_url")
            .GetString();

        return secureUrl ?? throw new Exception("No URL in Cloudinary response");
    }

    public async Task DeleteAudioAsync(string poiId, string lang)
    {
        var publicId = $"audio/{poiId}/{lang}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var sigString = $"public_id={publicId}&timestamp={timestamp}{API_SECRET}";
        var signature = ComputeSha1(sigString);

        var url = $"https://api.cloudinary.com/v1_1/{CLOUD_NAME}/raw/destroy";

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["public_id"] = publicId,
            ["timestamp"] = timestamp,
            ["api_key"] = API_KEY,
            ["signature"] = signature,
        });

        await _http.PostAsync(url, form);
    }

    // ── SHA1 helper ───────────────────────────────────────────────────
    private static string ComputeSha1(string input)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hash = System.Security.Cryptography.SHA1.HashData(bytes);
        return Convert.ToHexString(hash).ToLower();
    }
}