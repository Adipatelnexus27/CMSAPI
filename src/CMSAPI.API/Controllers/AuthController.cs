using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CMSAPI.API.Options;
using CMSAPI.Application.DTOs.Auth;
using CMSAPI.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CMSAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly JwtOptions _jwtOptions;

    public AuthController(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequestDto request)
    {
        var userRole = ValidateUser(request);
        if (userRole is null)
        {
            return Unauthorized("Invalid credentials.");
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes);
        var token = BuildToken(request.Username, userRole.Value, expiresAt);

        return Ok(new LoginResponseDto
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAt
        });
    }

    private UserRole? ValidateUser(LoginRequestDto request)
    {
        var users = new Dictionary<string, (string Password, UserRole Role)>(StringComparer.OrdinalIgnoreCase)
        {
            ["admin"] = ("Admin@123", UserRole.Admin),
            ["adjuster"] = ("Adjuster@123", UserRole.Adjuster),
            ["supervisor"] = ("Supervisor@123", UserRole.Supervisor)
        };

        if (!users.TryGetValue(request.Username, out var user))
        {
            return null;
        }

        return user.Password == request.Password ? user.Role : null;
    }

    private string BuildToken(string username, UserRole role, DateTime expiresAtUtc)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

