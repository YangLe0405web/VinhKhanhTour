using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public async Task<IActionResult> Save([FromBody] PoiModel poi)
    {
        var isNew = string.IsNullOrEmpty(poi.Id);
        var username = User.Identity?.Name ?? "";
        bool isAdmin = User.IsInRole("admin");
        
        Console.WriteLine($"💾 [API] Save POI: isNew={isNew}, user='{username}', isAdmin={isAdmin}");
        foreach(var c in User.Claims) Console.WriteLine($"   Claim: {c.Type} = {c.Value}");

        if (isNew)
        {
            if (!isAdmin) 
            {
                poi.OwnerId = username;
                Console.WriteLine($"🔗 [API] New POI: Set OwnerId={username}");
            }
        }
        else
        {
            // Bảo vệ OwnerId khi sửa: Không cho phép thay đổi chủ sở hữu trừ khi là Admin
            var existingPoi = (await _db.GetAllPoisAsync()).FirstOrDefault(p => p.Id == poi.Id);
            if (existingPoi != null && !isAdmin)
            {
                poi.OwnerId = existingPoi.OwnerId; // Giữ nguyên chủ sở hữu cũ
                if (existingPoi.OwnerId != username)
                {
                    Console.WriteLine($"⚠️ [API] Unauthorized Edit Attempt: {username} tries to edit POI of {existingPoi.OwnerId}");
                    return Forbid();
                }
            }
        }

        var id = await _db.SavePoiAsync(poi);
        Console.WriteLine($"✅ [API] Save SUCCESS: id={id}, OwnerId={poi.OwnerId}");
        return Ok(new { id });
    }

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
            var path = await _storage.UploadAudioAsync(
                stream, id, lang, file.ContentType);

            var forwardedProto = Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
            var scheme = string.IsNullOrWhiteSpace(forwardedProto) ? Request.Scheme : forwardedProto;
            var baseUrl = $"{scheme}://{Request.Host}";
            var fullUrl = $"{baseUrl}{path}";

            Console.WriteLine($"[Upload] Firestore OK: {fullUrl}");

            var pois = await _db.GetAllPoisAsync();
            var poi = pois.FirstOrDefault(p => p.Id == id);
            if (poi == null) return NotFound($"POI {id} not found");

            poi.AudioUrls[lang] = fullUrl;
            await _db.SavePoiAsync(poi);
            Console.WriteLine($"[Upload] Saved to POI OK");
            return Ok(new { url = fullUrl });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔥 UPLOAD ERROR: {ex}");
            return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
        }
    }

    // GET api/pois/{id}/audio/{lang} — Phát audio file
    [HttpGet("{id}/audio/{lang}")]
    public async Task<IActionResult> GetAudio(string id, string lang)
    {
        var (data, contentType) = await _storage.GetAudioAsync(id, lang);
        if (data == null) return NotFound("Audio not found");
        return File(data, contentType ?? "audio/mpeg");
    }

    // GET api/pois/{id}/check-access?deviceId=...
    [HttpGet("{id}/check-access")]
    public async Task<IActionResult> CheckAccess(string id, [FromQuery] string deviceId)
    {
        Console.WriteLine($"🔍 [API] CheckAccess Start: poi={id}, device={deviceId}");
        if (string.IsNullOrEmpty(deviceId)) return BadRequest("DeviceId is required");
        var hasAccess = await _db.CheckPoiAccessAsync(deviceId, id);
        return Ok(new { paid = hasAccess });
    }

    // POST api/pois/{id}/pay
    [HttpPost("{id}/pay")]
    public async Task<IActionResult> Pay(string id, [FromBody] AppHistory payInfo)
    {
        Console.WriteLine($"💰 [API] Pay Start: poi={id}, device={payInfo?.Device}");
        if (string.IsNullOrEmpty(payInfo.Device)) return BadRequest("Device info is required");
        
        var pois = await _db.GetAllPoisAsync();
        var poi = pois.FirstOrDefault(p => p.Id == id);
        if (poi == null) return NotFound("POI not found");

        payInfo.Action = "pay_audio";
        payInfo.PoiId = id;
        payInfo.PoiName = poi.Name;
        payInfo.Amount = poi.Price;
        payInfo.Currency = "VND";
        payInfo.Timestamp = DateTime.UtcNow;

        await _db.LogHistoryAsync(payInfo);

        return Ok(new { success = true, amount = poi.Price, currency = "VND" });
    }
}