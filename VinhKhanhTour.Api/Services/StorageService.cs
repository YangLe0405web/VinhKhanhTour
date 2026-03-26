using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace VinhKhanhTour.Api.Services;

public class StorageService
{
    // Cấu hình Cloudinary
    private const string CLOUD_NAME = "denzxxuw4";
    private const string API_KEY = "162781952147593";
    private const string API_SECRET = "3IbFP7kQAIOBBEqzdgDy_7VoJZk";

    // Tên Upload Preset bạn đã tạo trên Dashboard Cloudinary (chế độ Unsigned)
    private const string UPLOAD_PRESET = "vinhkhanh_preset";

    private readonly HttpClient _http = new();

    public StorageService() { }

    /// <summary>
    /// Tải file audio lên Cloudinary bằng phương thức Unsigned (Không cần Signature).
    /// </summary>
    public async Task<string> UploadAudioAsync(
        Stream fileStream, string poiId, string lang, string contentType)
    {
        // Public ID để quản lý file theo cấu trúc thư mục
        var publicId = $"audio/{poiId}/{lang}";

        using var content = new MultipartFormDataContent();

        // 1. Thêm các tham số cho Unsigned Upload
        // Quan trọng: Phải có upload_preset và tên preset đó phải ở chế độ 'Unsigned'
        content.Add(new StringContent(UPLOAD_PRESET), "upload_preset");
        content.Add(new StringContent(publicId), "public_id");

        // 2. Xử lý file stream
        using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

        // Cloudinary nhận file qua key "file"
        content.Add(fileContent, "file", $"{lang}.mp3");

        // 3. Gửi request lên endpoint video/upload
        var url = $"https://api.cloudinary.com/v1_1/{CLOUD_NAME}/video/upload";
        var resp = await _http.PostAsync(url, content);
        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
        {
            throw new Exception($"Cloudinary Unsigned Upload failed: {body}");
        }

        // 4. Trả về URL an toàn
        using var doc = JsonDocument.Parse(body);
        var secureUrl = doc.RootElement.GetProperty("secure_url").GetString() ?? "";

        Console.WriteLine($"[Cloudinary] Upload thành công: {secureUrl}");
        return secureUrl;
    }

    /// <summary>
    /// Xóa file trên Cloudinary (Hành động xóa bắt buộc phải dùng Signature).
    /// </summary>
    public async Task DeleteAudioAsync(string poiId, string lang)
    {
        try
        {
            var publicId = $"audio/{poiId}/{lang}";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            // Tạo chữ ký cho hành động xóa
            var sigString = $"public_id={publicId}&timestamp={timestamp}{API_SECRET.Trim()}";
            var signature = ComputeSha1(sigString);

            var url = $"https://api.cloudinary.com/v1_1/{CLOUD_NAME}/video/destroy";

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

    private static string ComputeSha1(string input)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLower();
    }
}