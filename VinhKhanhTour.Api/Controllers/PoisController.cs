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
    public async Task<IActionResult> UploadAudio(string id, string lang, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File trống");

        try
        {
            using var stream = file.OpenReadStream();
            // Gọi service bằng stream luôn, không cần byte array nữa
            string url = await _storage.UploadAudioAsync(stream, id, lang);

            // Đoạn lưu Firestore giữ nguyên của Giang nhé
            var pois = await _db.GetAllPoisAsync();
            var poi = pois.FirstOrDefault(p => p.Id == id);
            if (poi != null)
            {
                poi.AudioUrls[lang] = url;
                await _db.SavePoiAsync(poi);
            }

            return Ok(new { url });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}