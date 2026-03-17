using Microsoft.AspNetCore.Mvc;
using VinhKhanhTour.Api.Services;
using VinhKhanhTour.Shared.Models;

namespace VinhKhanhTour.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToursController : ControllerBase
{
    private readonly FirestoreService _db;

    public ToursController(FirestoreService db) => _db = db;

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
}