using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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
        var publicId = $"audio/{poiId}/{lang}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        // Ký: các param theo alphabet, KHÔNG có api_key / file / resource_type
        var sigString = $"public_id={publicId}&timestamp={timestamp}{API_SECRET}";
        var signature = ComputeSha1(sigString);

        Console.WriteLine($"[Cloudinary] sig_input : {sigString}");
        Console.WriteLine($"[Cloudinary] signature : {signature}");

        // Đọc file vào byte[]
        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await fileStream.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }
        Console.WriteLine($"[Cloudinary] file size : {fileBytes.Length} bytes");

        // ── KEY FIX: text fields phải NULL ContentType header ──
        // Nếu để mặc định (text/plain; charset=utf-8) Cloudinary bỏ qua api_key
        // và treat request là unsigned → lỗi "Upload preset must be specified"
        StringContent Field(string value)
        {
            var sc = new StringContent(value);
            sc.Headers.ContentType = null;   // <── bắt buộc
            return sc;
        }

        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(
            string.IsNullOrEmpty(contentType) ? "audio/mpeg" : contentType);

        using var form = new MultipartFormDataContent();
        form.Add(fileContent, "file", $"{lang}.mp3");
        form.Add(Field(publicId), "public_id");
        form.Add(Field(timestamp), "timestamp");
        form.Add(Field(API_KEY), "api_key");
        form.Add(Field(signature), "signature");

        var url = $"https://api.cloudinary.com/v1_1/{CLOUD_NAME}/video/upload";
        Console.WriteLine($"[Cloudinary] POST {url}  public_id={publicId}");

        var resp = await _http.PostAsync(url, form);
        var body = await resp.Content.ReadAsStringAsync();

        Console.WriteLine($"[Cloudinary] status : {resp.StatusCode}");
        Console.WriteLine($"[Cloudinary] body   : {body[..Math.Min(body.Length, 500)]}");

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"Cloudinary upload failed ({resp.StatusCode}): {body}");

        using var doc = JsonDocument.Parse(body);
        var secureUrl = doc.RootElement.GetProperty("secure_url").GetString() ?? "";
        Console.WriteLine($"[Cloudinary] OK → {secureUrl}");
        return secureUrl;
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
            Console.WriteLine($"[Cloudinary Delete] ERROR: {ex.Message}");
        }
    }

    private static string ComputeSha1(string input)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}