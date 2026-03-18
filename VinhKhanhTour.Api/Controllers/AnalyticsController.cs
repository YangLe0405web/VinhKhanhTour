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

    // GET api/analytics
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _db.GetAnalyticsAsync());

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