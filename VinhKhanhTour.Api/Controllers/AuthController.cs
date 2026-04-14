using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VinhKhanhTour.Api.Services;
using VinhKhanhTour.Shared.Models;

namespace VinhKhanhTour.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly FirestoreService _db;
    private readonly IConfiguration _config;

    public AuthController(FirestoreService db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.GetUserByUsernameAsync(request.Username);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Tài khoản hoặc mật khẩu không chính xác");
        }

        // Chặn đăng nhập nếu hết hạn (chỉ áp dụng cho chủ quán)
        if (user.Role == "owner" && user.ExpiryDate < DateTime.UtcNow)
        {
            return StatusCode(403, "Tài khoản của bạn đã hết hạn sử dụng. Vui lòng liên hệ Admin để gia hạn phí dịch vụ.");
        }

        var token = GenerateJwtToken(user);
        return Ok(new
        {
            token = token,
            username = user.Username,
            role = user.Role,
            fullName = user.FullName,
            expiryDate = user.ExpiryDate,
            managedPoiIds = user.ManagedPoiIds
        });
    }

    private string GenerateJwtToken(AppUser user)
    {
        var jwtKey = _config["Jwt:Secret"] ?? "vinhkhanhtour_super_secret_key_2026_!@#";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("UserId", user.Id),
            new Claim("ExpiryDate", user.ExpiryDate.ToString("O")) // ISO 8601 format
        };

        if (user.ManagedPoiIds.Any())
        {
            claims.Add(new Claim("ManagedPoiIds", string.Join(",", user.ManagedPoiIds)));
        }

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
