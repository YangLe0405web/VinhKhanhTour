using Microsoft.AspNetCore.Mvc;
using VinhKhanhTour.Api.Services;
using VinhKhanhTour.Shared.Models;

namespace VinhKhanhTour.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly FirestoreService _db;

    public HistoryController(FirestoreService db) => _db = db;

    // GET api/history?force=true
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int limit = 100, [FromQuery] bool force = false)
    {
        if (force) _db.ClearAllCache();
        return Ok(await _db.GetHistoryAsync(limit));
    }

    // POST api/history
    [HttpPost]
    public async Task<IActionResult> Log([FromBody] AppHistory history)
    {
        await _db.LogHistoryAsync(history);
        return Ok();
    }
}