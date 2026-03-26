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

    // POST api/pois/{id}/audio/{lang}
    [HttpPost("{id}/audio/{lang}")]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20MB limit
    public async Task<IActionResult> UploadAudio(
        string id, string lang, IFormFile file)
    {
        Console.WriteLine($"[Upload] START poi={id} lang={lang} file={file?.FileName} size={file?.Length}");

        if (file == null || file.Length == 0)
        {
            Console.WriteLine("[Upload] File is null or empty!");
            return BadRequest("File is required");
        }

        try
        {
            string url;
            using (var stream = file.OpenReadStream())
            {
                url = await _storage.UploadAudioAsync(stream, id, lang, file.ContentType);
            }

            Console.WriteLine($"[Upload] Cloudinary OK: {url}");

            var pois = await _db.GetAllPoisAsync();
            var poi = pois.FirstOrDefault(p => p.Id == id);
            if (poi == null)
            {
                Console.WriteLine($"[Upload] POI not found: {id}");
                return NotFound();
            }

            poi.AudioUrls[lang] = url;
            await _db.SavePoiAsync(poi);

            Console.WriteLine($"[Upload] Saved to Firestore OK");
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Upload] EXCEPTION: {ex}");
            return StatusCode(500, ex.Message);
        }
    }
}