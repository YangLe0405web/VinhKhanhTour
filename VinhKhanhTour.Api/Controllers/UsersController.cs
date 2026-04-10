using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VinhKhanhTour.Api.Services;
using VinhKhanhTour.Shared.Models;

namespace VinhKhanhTour.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly FirestoreService _db;

    public UsersController(FirestoreService db) => _db = db;

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.GetAllUsersAsync();
        // Remove password hashes from response for safety
        foreach (var u in users) u.PasswordHash = "";
        return Ok(users);
    }


    [HttpPost]
    [Authorize(Roles = "admin")]
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
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(string id)
    {
        await _db.DeleteUserAsync(id);
        return Ok();
    }

    [HttpPost("register-merchant")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterMerchant([FromBody] MerchantRegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.PhoneNumber)) return BadRequest("Số điện thoại là bắt buộc");
        if (string.IsNullOrWhiteSpace(req.FullName)) return BadRequest("Họ và Tên là bắt buộc");
        
        // Kiểm tra trùng SĐT (Username)
        var existingPhone = await _db.GetUserByUsernameAsync(req.PhoneNumber);
        if (existingPhone != null) return BadRequest("Số điện thoại này đã được sử dụng");

        // Kiểm tra trùng Email
        if (!string.IsNullOrWhiteSpace(req.Gmail))
        {
            var existingEmail = await _db.GetUserByEmailAsync(req.Gmail);
            if (existingEmail != null) return BadRequest("Email này đã được sử dụng");
        }

        var newUser = new AppUser
        {
            Username = req.PhoneNumber,
            PhoneNumber = req.PhoneNumber,
            FullName = req.FullName,
            Email = req.Gmail,
            Address = req.Address,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
            Role = "owner",
            ManagedPoiIds = new List<string>()
        };

        await _db.SaveUserAsync(newUser);

        // Ghi lại lịch sử thanh toán đăng ký (500,000 VND mặc định)
        var history = new AppHistory
        {
            Action = "register_poi",
            PoiName = "Đăng ký Chủ quán mới: " + req.FullName,
            Device = "CMS Registration",
            Amount = 500000,
            Currency = "VND",
            Timestamp = DateTime.UtcNow
        };
        await _db.LogHistoryAsync(history);

        return Ok(new { success = true, username = newUser.Username, password = "123" });
    }
}
