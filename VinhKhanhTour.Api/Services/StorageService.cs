using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace VinhKhanhTour.Api.Services;

public class StorageService
{
    private const string CLOUD_NAME = "denzxxuw4";
    private const string API_KEY = "162781952147593";
    private readonly IConfiguration _config;
    private readonly HttpClient _http = new();

    public StorageService(IConfiguration config)
    {
        _config = config;
    }

    private async Task<string> GetApiSecret()
    {
        // Ưu tiên đọc từ file bí mật (cloudinary-secret.txt)
        var renderSecret = "/etc/secrets/cloudinary-secret.txt";
        var localSecret = Path.Combine(AppContext.BaseDirectory, "cloudinary-secret.txt");

        if (File.Exists(renderSecret)) return (await File.ReadAllTextAsync(renderSecret)).Trim();
        if (File.Exists(localSecret)) return (await File.ReadAllTextAsync(localSecret)).Trim();

        // Backup nếu chưa tạo file (Giang nên tạo file để an toàn hơn)
        return _config["Cloudinary:ApiSecret"] ?? "3IbFP7kQAIOBBEqzdgDy_7VoJZk";
    }

    public async Task<string> UploadAudioAsync(Stream fileStream, string poiId, string lang, string contentType)
    {
        var publicId = $"audio/{poiId}/{lang}";

        // ĐỊA CHỈ UPLOAD (Dùng /video/ cho audio để tránh lỗi CORS)
        var url = $"https://api.cloudinary.com/v1_1/{CLOUD_NAME}/video/upload";

        using var form = new MultipartFormDataContent();

        // File content
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType);
        form.Add(fileContent, "file", $"{lang}.mp3");

        // CÁC THAM SỐ CHO UNSIGNED UPLOAD
        form.Add(new StringContent(publicId), "public_id");
        form.Add(new StringContent("vinhkhanh_audio"), "upload_preset"); // Tên bạn vừa đặt ở Bước 1

        var resp = await _http.PostAsync(url, form);
        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"Cloudinary upload failed: {body}");

        using var doc = System.Text.Json.JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("secure_url").GetString() ?? "";
    }

    public async Task DeleteAudioAsync(string poiId, string lang)
    {
        var apiSecret = await GetApiSecret();
        var publicId = $"audio/{poiId}/{lang}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var sigString = $"public_id={publicId}&timestamp={timestamp}{apiSecret}";
        var signature = ComputeSha1(sigString);

        // CŨNG PHẢI ĐỔI /raw/ SANG /video/ Ở ĐÂY
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