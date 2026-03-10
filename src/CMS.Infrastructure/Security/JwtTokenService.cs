using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CMS.Application.Interfaces.Services;
using CMS.Application.Models;
using CMS.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CMS.Infrastructure.Security;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(Guid userId, string email, string fullName, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions)
    {
        var nowUtc = DateTime.UtcNow;
        var expiresAtUtc = nowUtc.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, fullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));
        foreach (var permission in permissions) claims.Add(new Claim("permission", permission));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: nowUtc,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }

    public GeneratedRefreshToken GenerateRefreshToken()
    {
        var plainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(plainToken));

        return new GeneratedRefreshToken
        {
            PlainToken = plainToken,
            TokenHash = Convert.ToHexString(hash),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_options.RefreshTokenDays)
        };
    }
}
