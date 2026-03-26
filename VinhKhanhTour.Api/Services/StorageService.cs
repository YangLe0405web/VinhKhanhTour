using System.Security.Cryptography;
using System.Text;

namespace VinhKhanhTour.Api.Services;

public class StorageService
{
    private const string CLOUD_NAME = "denzxxuw4";
    private const string API_KEY = "162781952147593";
    private const string UPLOAD_PRESET = "vinhkhanh_audio"; // Unsigned preset trên Cloudinary
    private readonly IConfiguration _config;
    private readonly HttpClient _http = new();

    public StorageService(IConfiguration config)
    {
        _config = config;
    }

    private async Task<string> GetApiSecret()
    {
        var renderSecret = "/etc/secrets/cloudinary-secret.txt";
        var localSecret = Path.Combine(AppContext.BaseDirectory, "cloudinary-secret.txt");

        if (File.Exists(renderSecret)) return (await File.ReadAllTextAsync(renderSecret)).Trim();
        if (File.Exists(localSecret)) return (await File.ReadAllTextAsync(localSecret)).Trim();

        return _config["Cloudinary:ApiSecret"] ?? "3IbFP7kQAIOBBEqzdgDy_7VoJZk";
    }

    /// <summary>Upload audio lên Cloudinary (unsigned preset)</summary>
    public async Task<string> UploadAudioAsync(
        Stream fileStream, string poiId, string lang, string contentType)
    {
        var publicId = $"audio/{poiId}/{lang}";
        var url = $"https://api.cloudinary.com/v1_1/{CLOUD_NAME}/video/upload";

        // ── Copy stream vào MemoryStream TRƯỚC khi dùng ──────────────
        // IFormFile stream bị dispose ngay sau request → phải copy ra MemoryStream
        // nếu không, Cloudinary nhận file rỗng → lỗi 400 hoặc 500
        using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);
        ms.Position = 0;

        using var form = new MultipartFormDataContent();

        var fileContent = new StreamContent(ms);
        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(
                string.IsNullOrEmpty(contentType) ? "audio/mpeg" : contentType);

        form.Add(fileContent, "file", $"{lang}.mp3");
        form.Add(new StringContent(publicId), "public_id");
        form.Add(new StringContent(UPLOAD_PRESET), "upload_preset");

        var resp = await _http.PostAsync(url, form);
        var body = await resp.Content.ReadAsStringAsync();

        Console.WriteLine($"[Cloudinary Upload] {resp.StatusCode} | poi={poiId} lang={lang}");
        if (!resp.IsSuccessStatusCode)
            Console.WriteLine($"[Cloudinary Upload] Error body: {body}");

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"Cloudinary upload failed ({resp.StatusCode}): {body}");

        using var doc = System.Text.Json.JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("secure_url").GetString() ?? "";
    }

    /// <summary>Xóa audio trên Cloudinary (cần signed request)</summary>
    public async Task DeleteAudioAsync(string poiId, string lang)
    {
        var apiSecret = await GetApiSecret();
        var publicId = $"audio/{poiId}/{lang}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var sigString = $"public_id={publicId}&timestamp={timestamp}{apiSecret}";
        var signature = ComputeSha1(sigString);

        var url = $"https://api.cloudinary.com/v1_1/{CLOUD_NAME}/video/destroy";

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["public_id"] = publicId,
            ["timestamp"] = timestamp,
            ["api_key"] = API_KEY,
            ["signature"] = signature,
        });

        await _http.PostAsync(url, form);
    }

    private static string ComputeSha1(string input)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}