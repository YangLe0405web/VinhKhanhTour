using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VinhKhanhTour.Api.Services;
using VinhKhanhTour.Shared.Models;

namespace VinhKhanhTour.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly FirestoreService _db;

    public MeController(FirestoreService db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var username = User.Identity?.Name;
        Console.WriteLine($"👤 [API/ME] Request for: {username}");
        
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"🔍 [API/ME] Claim: {claim.Type} = {claim.Value}");
        }

        if (string.IsNullOrEmpty(username)) 
        {
            return Unauthorized();
        }

        var user = await _db.GetUserByUsernameAsync(username);
        if (user == null) 
        {
            Console.WriteLine($"❌ [API/ME] User '{username}' not found");
            return NotFound();
        }

        user.PasswordHash = "";
        return Ok(user);
    }
}
