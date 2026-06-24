using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Habitto.Domain.Entities;
using Habitto.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Habitto.Api.Controllers;

public sealed record RegisterRequest(string Email, string Password, string FullName);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string Token, Guid UserId);

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;

    public AuthController(IUserRepository users, IUnitOfWork unitOfWork, IConfiguration config)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var existing = await _users.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            return Conflict("Ya existe un usuario con ese email.");

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new AppUser(request.Email, hash, request.FullName);

        await _users.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Ok(new AuthResponse(GenerateToken(user), user.Id));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(request.Email, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Credenciales inválidas.");

        return Ok(new AuthResponse(GenerateToken(user), user.Id));
    }

    private string GenerateToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
