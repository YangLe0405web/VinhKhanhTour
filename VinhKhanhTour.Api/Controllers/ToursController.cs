using Microsoft.AspNetCore.Mvc;
using VinhKhanhTour.Api.Services;
using VinhKhanhTour.Shared.Models;

namespace VinhKhanhTour.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToursController : ControllerBase
{
    private readonly FirestoreService _db;
    private readonly StorageService _storage;

    public ToursController(FirestoreService db, StorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    // GET api/tours
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _db.GetAllToursAsync());

    // POST api/tours
    [HttpPost]
    public async Task<IActionResult> Save([FromBody] TourModel tour)
        => Ok(new { id = await _db.SaveTourAsync(tour) });

    // DELETE api/tours/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _db.DeleteTourAsync(id);
        return Ok();
    }

    // POST api/tours/{id}/scan
    [HttpPost("{id}/scan")]
    public async Task<IActionResult> Scan(string id)
    {
        await _db.IncrementQrScansAsync(id);
        return Ok();
    }

    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File is empty");
        using var stream = file.OpenReadStream();
        var url = await _storage.UploadImageAsync(stream, file.FileName, file.ContentType);
        return Ok(new { url });
    }

    [HttpGet("image/{id}")]
    public async Task<IActionResult> GetImage(string id)
    {
        var (data, contentType) = await _storage.GetImageAsync(id);
        if (data == null) return NotFound();
        return File(data, contentType ?? "image/jpeg");
    }
}