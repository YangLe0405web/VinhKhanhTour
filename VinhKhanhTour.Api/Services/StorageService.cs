using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace VinhKhanhTour.Api.Services;

public class StorageService
{
    // Thông tin cấu hình Cloudinary
    // Thay vì dùng private const string...
    private readonly string CLOUD_NAME = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? "denzxxuw4";
    private readonly string API_KEY = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? "162781952147593";
    private readonly string API_SECRET = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? "3IbFP7kQAIOBBEqzdgDy_7VoJZk";

    private readonly HttpClient _http = new();

    public StorageService() { }

    /// <summary>
    /// Tải file audio lên Cloudinary với Public ID cố định theo POI và ngôn ngữ.
    /// </summary>
    public async Task<string> UploadAudioAsync(
        Stream fileStream, string poiId, string lang, string contentType)
    {
        // 1. Chuẩn bị các tham số
        var publicId = $"audio/{poiId}/{lang}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        // 2. Tạo Signature (Sắp xếp theo Alphabet: public_id -> timestamp)
        // Lưu ý: Không bao gồm file, api_key hay resource_type trong chuỗi ký
        var sigString = $"public_id={publicId}&timestamp={timestamp}{API_SECRET.Trim()}";
        var signature = ComputeSha1(sigString);

        Console.WriteLine($"[Cloudinary] sig_input : {sigString}");
        Console.WriteLine($"[Cloudinary] signature : {signature}");

        using var content = new MultipartFormDataContent();

        // Hàm helper để thêm các trường text mà không có Content-Type header (tránh lỗi signature)
        void AddStringField(string value, string name)
        {
            var fieldContent = new StringContent(value);
            fieldContent.Headers.ContentType = null;
            content.Add(fieldContent, name);
        }

        AddStringField(publicId, "public_id");
        AddStringField(timestamp, "timestamp");
        AddStringField(API_KEY, "api_key");
        AddStringField(signature, "signature");

        // 3. Xử lý file stream
        using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

        // Cloudinary nhận file qua key "file"
        content.Add(fileContent, "file", $"{lang}.mp3");

        // 4. Gửi request lên endpoint video (dùng cho cả audio)
        var url = $"https://api.cloudinary.com/v1_1/{CLOUD_NAME}/video/upload";
        var resp = await _http.PostAsync(url, content);
        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
        {
            throw new Exception($"Cloudinary upload failed ({resp.StatusCode}): {body}");
        }

        // 5. Lấy URL trả về
        using var doc = JsonDocument.Parse(body);
        var secureUrl = doc.RootElement.GetProperty("secure_url").GetString() ?? "";

        Console.WriteLine($"[Cloudinary] OK → {secureUrl}");
        return secureUrl;
    }

    /// <summary>
    /// Xóa file audio trên Cloudinary.
    /// </summary>
    public async Task DeleteAudioAsync(string poiId, string lang)
    {
        try
        {
            var publicId = $"audio/{poiId}/{lang}";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            var sigString = $"public_id={publicId}&timestamp={timestamp}{API_SECRET.Trim()}";
            var signature = ComputeSha1(sigString);

            var url = $"https://api.cloudinary.com/v1_1/{CLOUD_NAME}/video/destroy";

            // Với method destroy, Cloudinary chấp nhận FormUrlEncodedContent
            using var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["public_id"] = publicId,
                ["timestamp"] = timestamp,
                ["api_key"] = API_KEY,
                ["signature"] = signature,
            });

            var resp = await _http.PostAsync(url, form);
            var body = await resp.Content.ReadAsStringAsync();

            Console.WriteLine($"[Cloudinary Delete] Response: {body}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Cloudinary Delete] ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Mã hóa SHA1 và trả về chuỗi hex viết thường (lowercase).
    /// </summary>
    private static string ComputeSha1(string input)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLower();
    }
}