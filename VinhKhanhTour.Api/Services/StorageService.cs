using System.Security.Cryptography;
using System.Text;

namespace VinhKhanhTour.Api.Services;

public class StorageService
{
    private const string CLOUD_NAME = "denzxxuw4";
    private const string API_KEY = "162781952147593";
    private const string API_SECRET = "3IbFP7kQAIOBBEqzdgDy_7VoJZk";

    private readonly HttpClient _http = new();

    public StorageService() { }

    public async Task<string> UploadAudioAsync(
        Stream fileStream, string poiId, string lang, string contentType)
    {
        try
        {
            var publicId = $"audio/{poiId}/{lang}";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            // Signed upload: ký SHA1(public_id=...&timestamp=...{SECRET})
            // Các param phải sắp xếp alphabet trước khi ký
            var sigString = $"public_id={publicId}&timestamp={timestamp}{API_SECRET}";
            var signature = ComputeSha1(sigString);

            var url = $"https://api.cloudinary.com/v1_1/{CLOUD_NAME}/video/upload";

            // Đọc toàn bộ stream ra byte[] trước để tránh dispose stream sớm
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await fileStream.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            Console.WriteLine($"[Cloudinary] File size: {fileBytes.Length} bytes");

            using var form = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    string.IsNullOrEmpty(contentType) ? "audio/mpeg" : contentType);

            form.Add(fileContent, "file", $"{lang}.mp3");
            form.Add(new StringContent(publicId), "public_id");
            form.Add(new StringContent(timestamp), "timestamp");
            form.Add(new StringContent(API_KEY), "api_key");
            form.Add(new StringContent(signature), "signature");
            // KHÔNG dùng upload_preset khi signed upload

            Console.WriteLine($"[Cloudinary] Sending signed upload → public_id={publicId}");

            var resp = await _http.PostAsync(url, form);
            var body = await resp.Content.ReadAsStringAsync();

            Console.WriteLine($"[Cloudinary] Status={resp.StatusCode}");
            Console.WriteLine($"[Cloudinary] Body={body[..Math.Min(body.Length, 500)]}");

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Cloudinary upload failed ({resp.StatusCode}): {body}");

            using var doc = System.Text.Json.JsonDocument.Parse(body);
            var secureUrl = doc.RootElement.GetProperty("secure_url").GetString() ?? "";
            Console.WriteLine($"[Cloudinary] Upload OK: {secureUrl}");
            return secureUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Cloudinary] EXCEPTION: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteAudioAsync(string poiId, string lang)
    {
        try
        {
            var publicId = $"audio/{poiId}/{lang}";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var sigString = $"public_id={publicId}&timestamp={timestamp}{API_SECRET}";
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
        catch (Exception ex)
        {
            Console.WriteLine($"[Cloudinary Delete] EXCEPTION: {ex.Message}");
        }
    }

    private static string ComputeSha1(string input)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}