using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace VinhKhanhTour.Api.Services;

public class StorageService
{
    private readonly StorageClient _client;
    const string BUCKET = "vinhkhanhtour-c8e3f.firebasestorage.app";

    public StorageService()
     => _client = StorageClient.Create();

    // Tạm: liệt kê tất cả buckets trong project
    public IEnumerable<string> ListBuckets()
    {
        try
        {
            return _client.ListBuckets("vinhkhanhtour-c8e3f")
                .Select(b => b.Name);
        }
        catch (Exception ex)
        {
            return new[] { $"Error: {ex.Message}" };
        }
    }

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