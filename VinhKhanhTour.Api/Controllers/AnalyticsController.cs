using Microsoft.AspNetCore.Mvc;
using VinhKhanhTour.Api.Services;
using VinhKhanhTour.Shared.Models;

namespace VinhKhanhTour.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly FirestoreService _db;

    public AnalyticsController(FirestoreService db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool force = false)
    {
        if (force) _db.ClearAllCache();
        var data = await _db.GetAnalyticsAsync();
        return Ok(data);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] bool force = false)
    {
        var stats = await _db.GetGlobalStatsAsync(force);
        return Ok(stats);
    }

    // POST api/analytics
    [HttpPost]
    public async Task<IActionResult> Log([FromBody] AnalyticsEvent ev)
    {
        try
        {
            await _db.LogEventAsync(ev);
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine("🔥 ANALYTICS ERROR: " + ex.ToString());
            return StatusCode(500, ex.ToString());
        }
    }
}