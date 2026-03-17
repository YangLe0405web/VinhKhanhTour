using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace VinhKhanhTour.Api.Services;

public class StorageService
{
    private readonly StorageClient _client;
    const string BUCKET = "vinhkhanhtour-c8e3f.appspot.com";

    public StorageService()
    {
        var base64Key = Environment.GetEnvironmentVariable("FIREBASE_KEY_BASE64");

        if (!string.IsNullOrEmpty(base64Key))
        {
            var keyJson = System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(base64Key));
            var credential = GoogleCredential.FromJson(keyJson);
            _client = StorageClient.Create(credential);
        }
        else
        {
            _client = StorageClient.Create();
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