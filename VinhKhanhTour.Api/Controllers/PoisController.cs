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

    // GET api/pois
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool admin = false)
    {
        var pois = await _db.GetAllPoisAsync();
        if (!admin)
            pois = pois.Where(p => p.IsActive).ToList();
        return Ok(pois);
    }

    // POST api/pois
    [HttpPost]
    public async Task<IActionResult> Save([FromBody] PoiModel poi)
        => Ok(new { id = await _db.SavePoiAsync(poi) });

    // DELETE api/pois/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _db.DeletePoiAsync(id);
        return Ok();
    }

    // POST api/pois/{id}/audio/{lang}
    [HttpPost("{id}/audio/{lang}")]
    public async Task<IActionResult> UploadAudio(
        string id, string lang, IFormFile file)
    {
        try
        {
            Console.WriteLine($"[Upload] Start: poi={id}, lang={lang}, file={file?.FileName}, size={file?.Length}");
            
            using var stream = file.OpenReadStream();
            var url = await _storage.UploadAudioAsync(
                stream, id, lang, file.ContentType);

            Console.WriteLine($"[Upload] GCS OK: {url}");

            var pois = await _db.GetAllPoisAsync();
            var poi = pois.FirstOrDefault(p => p.Id == id);
            if (poi == null) return NotFound($"POI {id} not found");

            poi.AudioUrls[lang] = url;
            await _db.SavePoiAsync(poi);
            Console.WriteLine($"[Upload] Saved to Firestore OK");
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔥 UPLOAD ERROR: {ex}");
            return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
        }
    }

    // POST api/analytics

}