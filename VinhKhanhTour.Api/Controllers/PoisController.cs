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
        using var stream = file.OpenReadStream();
        var url = await _storage.UploadAudioAsync(
            stream, id, lang, file.ContentType);

        var pois = await _db.GetAllPoisAsync();
        var poi = pois.FirstOrDefault(p => p.Id == id);
        if (poi == null) return NotFound();

        poi.AudioUrls[lang] = url;
        await _db.SavePoiAsync(poi);
        return Ok(new { url });
    }

    // POST api/analytics

}