using Microsoft.AspNetCore.Mvc;
using VinhKhanhTour.Api.Services;
using VinhKhanhTour.Shared.Models;

namespace VinhKhanhTour.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PoisController : ControllerBase
{
    private readonly FirestoreService _db;
    private readonly StorageService _storage;

    public PoisController(FirestoreService db, StorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _db.GetAllPoisAsync());

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] PoiModel poi)
        => Ok(new { id = await _db.SavePoiAsync(poi) });

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _db.DeletePoiAsync(id);
        return Ok();
    }

    [HttpPost("{id}/audio/{lang}")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> UploadAudio(string id, string lang, IFormFile file)
    {
        Console.WriteLine($"[Upload] START poi={id} lang={lang} file={file?.FileName} size={file?.Length}");

        if (file == null || file.Length == 0) return BadRequest("File is required");

        try
        {
            // 1. Kiểm tra POI tồn tại trước khi upload (Tránh phí công upload nếu ID sai)
            var pois = await _db.GetAllPoisAsync();
            var poi = pois.FirstOrDefault(p => p.Id == id);
            if (poi == null) return NotFound($"Không tìm thấy POI với ID: {id}");

            // 2. Thực hiện upload
            string url;
            using (var stream = file.OpenReadStream())
            {
                url = await _storage.UploadAudioAsync(stream, id, lang, file.ContentType);
            }

            // 3. Cập nhật Firestore
            poi.AudioUrls[lang] = url;
            poi.UpdatedAt = DateTime.UtcNow; // Nên update cả thời gian cập nhật
            await _db.SavePoiAsync(poi);

            return Ok(new { url });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Upload ERROR]: {ex.Message}");
            return StatusCode(500, ex.Message);
        }
    }
}