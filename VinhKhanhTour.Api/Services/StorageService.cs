using Google.Cloud.Storage.V1;

namespace VinhKhanhTour.Api.Services;

public class StorageService
{
    private readonly StorageClient _client;

    // ⚠️ Thay bằng bucket của bạn (thường là "projectid.appspot.com")
    const string BUCKET = "vinhkhanhtour-c8e3f.appspot.com";
    public StorageService()
        => _client = StorageClient.Create();

    public async Task<string> UploadAudioAsync(
        Stream fileStream, string poiId, string lang, string contentType)
    {
        var objectName = $"audio/{poiId}/{lang}.mp3";
        await _client.UploadObjectAsync(
            BUCKET, objectName, contentType, fileStream);
        return $"https://storage.googleapis.com/{BUCKET}/{objectName}";
    }

    public async Task DeleteAudioAsync(string poiId, string lang)
        => await _client.DeleteObjectAsync(
               BUCKET, $"audio/{poiId}/{lang}.mp3");
}