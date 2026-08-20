using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using KisiRehberiApi.Services;

namespace KisiRehberiApi.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private const string AdminName = "Admin";
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogService _logService;

    public AuthController(AppDbContext db, IConfiguration config, ILogService logService)
    {
        _db = db;
        _config = config;
        _logService = logService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
        {
            return BadRequest("Bu kullanıcı adı zaten mevcut.");
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = passwordHash
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok("Admin kullanıcısı başarıyla oluşturuldu.");
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null)
        {
            return BadRequest("Kullanıcı bulunamadı veya şifre yanlış.");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return BadRequest("Kullanıcı bulunamadı veya şifre yanlış.");
        }

        var jwtConf = _config.GetSection("Jwt");
        var keyBytes = Encoding.UTF8.GetBytes(jwtConf["Key"]!);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, AdminName)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = jwtConf["Issuer"],
            Audience = jwtConf["Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        await _logService.LogAsync(user.Username, "LOGIN", "Sisteme başarılı giriş yapıldı.");
        return Ok(new { token = tokenString });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var username = User.Identity?.Name ?? "Bilinmeyen";
        await _logService.LogAsync(username, "LOGOUT", "Sistemden çıkış yapıldı.");
        return Ok("Çıkış kaydedildi.");
    }
}
