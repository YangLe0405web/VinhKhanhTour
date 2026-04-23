using Microsoft.AspNetCore.Mvc;
using VinhKhanhTour.Api.Services;
using VinhKhanhTour.Shared.Models;

namespace VinhKhanhTour.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TraceController : ControllerBase
{
    private readonly FirestoreService _db;

    public TraceController(FirestoreService db) => _db = db;

    // POST api/trace — MAUI app gọi khi di chuyển
    [HttpPost]
    public async Task<IActionResult> Log([FromBody] LocationTrace trace)
    {
        await _db.LogTraceAsync(trace);
        return Ok();
    }

    // GET api/trace?force=true — CMS lấy để vẽ heatmap
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool force = false)
    {
        if (force) _db.ClearAllCache();
        return Ok(await _db.GetTracesAsync());
    }
}