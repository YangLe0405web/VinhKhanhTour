using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

public class StorageService
{
    private readonly Cloudinary _cloudinary;

    public StorageService()
    {
        // Thay 2 cái mã này bằng mã trên Dashboard Cloudinary của Giang nhé
        var account = new Account(
            "denzxxuw4",
            "162781952147593",
            "3IbFP7kQAIOBBEqzdgDy_7VoJZk"
        );
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string> UploadAudioAsync(Stream fileStream, string poiId, string lang)
    {
        if (fileStream.CanSeek) fileStream.Position = 0;

        var uploadParams = new VideoUploadParams()
        {
            File = new FileDescription($"{poiId}_{lang}.mp3", fileStream),
            PublicId = $"audio/{poiId}/{lang}",
            Overwrite = true,
            // Với SDK này, Giang thậm chí không cần dùng Upload Preset 
            // vì nó dùng API Key để xác thực rồi, cực kỳ an toàn.
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new Exception($"Lỗi Cloudinary: {uploadResult.Error.Message}");
        }

        return uploadResult.SecureUrl.ToString();
    }
}