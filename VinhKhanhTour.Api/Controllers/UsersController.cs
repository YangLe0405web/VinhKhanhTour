using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VinhKhanhTour.Api.Services;
using VinhKhanhTour.Shared.Models;

namespace VinhKhanhTour.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class UsersController : ControllerBase
{
    private readonly FirestoreService _db;

    public UsersController(FirestoreService db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.GetAllUsersAsync();
        // Remove password hashes from response for safety
        foreach (var u in users) u.PasswordHash = "";
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] AppUser user)
    {
        if (string.IsNullOrEmpty(user.Username)) return BadRequest("Username is required");

        // If it's a new user, password is required
        var existing = await _db.GetUserByUsernameAsync(user.Username);
        if (existing == null && string.IsNullOrEmpty(user.PasswordHash))
        {
            return BadRequest("Password is required for new accounts");
        }

        if (!string.IsNullOrEmpty(user.PasswordHash) && !user.PasswordHash.StartsWith("$2b$")) // check if already hashed (from client side or edited)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        }
        else if (existing != null && string.IsNullOrEmpty(user.PasswordHash))
        {
            user.PasswordHash = existing.PasswordHash; // Keep old password if not changing
        }

        await _db.SaveUserAsync(user);
        return Ok(new { id = user.Id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _db.DeleteUserAsync(id);
        return Ok();
    }
}
