using Google.Cloud.Firestore;

namespace VinhKhanhTour.Api.Services;

public class StorageService
{
    private readonly FirestoreDb _db;
    const string PROJECT_ID = "vinhkhanhtour-c8e3f";

    public StorageService()
    {
        _db = FirestoreDb.Create(PROJECT_ID);
    }

    /// <summary>
    /// Lưu audio file dưới dạng Base64 vào Firestore collection "audio_files"
    /// Document ID = "{poiId}_{lang}" (ví dụ: "abc123_vi")
    /// Trả về URL dạng API endpoint để App download
    /// </summary>
    public async Task<string> UploadAudioAsync(
        Stream fileStream, string poiId, string lang, string contentType)
    {
        // Đọc stream thành byte[] rồi Base64
        using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);
        var base64 = Convert.ToBase64String(ms.ToArray());

        var docId = $"{poiId}_{lang}";
        var data = new Dictionary<string, object>
        {
            { "poiId", poiId },
            { "lang", lang },
            { "contentType", contentType },
            { "data", base64 },
            { "updatedAt", Timestamp.GetCurrentTimestamp() }
        };

        await _db.Collection("audio_files").Document(docId).SetAsync(data);

        // Trả URL endpoint để App/CMS gọi lấy file
        return $"/api/pois/{poiId}/audio/{lang}";
    }

    /// <summary>
    /// Lấy audio Base64 từ Firestore
    /// </summary>
    public async Task<(byte[]? data, string? contentType)> GetAudioAsync(string poiId, string lang)
    {
        var docId = $"{poiId}_{lang}";
        var doc = await _db.Collection("audio_files").Document(docId).GetSnapshotAsync();
        
        if (!doc.Exists) return (null, null);

        var base64 = doc.GetValue<string>("data");
        var ct = doc.ContainsField("contentType") ? doc.GetValue<string>("contentType") : "audio/mpeg";
        return (Convert.FromBase64String(base64), ct);
    }

    public async Task DeleteAudioAsync(string poiId, string lang)
    {
        var docId = $"{poiId}_{lang}";
        await _db.Collection("audio_files").Document(docId).DeleteAsync();
    }
}