using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Application.Interfaces.Services;
using CMS.Domain.Enums;

namespace CMS.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IAuthRepository authRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(request.FullName)) throw new InvalidOperationException("Full name is required.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8) throw new InvalidOperationException("Password must be at least 8 characters.");
        if (!SystemRoles.All.Contains(request.Role)) throw new InvalidOperationException("Invalid role.");

        var existingUser = await _authRepository.GetUserByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser is not null) throw new InvalidOperationException("User with this email already exists.");

        var userId = Guid.NewGuid();
        var (hash, salt) = _passwordHasher.HashPassword(request.Password);

        await _authRepository.RegisterUserAsync(userId, normalizedEmail, request.FullName.Trim(), hash, salt, request.Role, cancellationToken);
        return await GenerateAuthResponseAsync(userId, normalizedEmail, request.FullName.Trim(), cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _authRepository.GetUserByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive) throw new UnauthorizedAccessException("Invalid credentials.");
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt)) throw new UnauthorizedAccessException("Invalid credentials.");

        return await GenerateAuthResponseAsync(user.UserId, user.Email, user.FullName, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken)) throw new UnauthorizedAccessException("Refresh token is required.");

        var requestHash = HashRefreshToken(request.RefreshToken);
        var currentToken = await _authRepository.GetRefreshTokenAsync(requestHash, cancellationToken);

        if (currentToken is null || currentToken.RevokedAtUtc.HasValue || currentToken.ExpiresAtUtc <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token is invalid or expired.");

        var user = await _authRepository.GetUserByIdAsync(currentToken.UserId, cancellationToken);
        if (user is null || !user.IsActive) throw new UnauthorizedAccessException("User is inactive.");

        await _authRepository.RevokeRefreshTokenAsync(requestHash, "Rotated on refresh", cancellationToken);
        return await GenerateAuthResponseAsync(user.UserId, user.Email, user.FullName, cancellationToken);
    }

    public async Task RevokeAsync(RevokeTokenRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken)) return;

        var requestHash = HashRefreshToken(request.RefreshToken);
        await _authRepository.RevokeRefreshTokenAsync(requestHash, request.Reason, cancellationToken);
    }

    public async Task<IReadOnlyList<UserSummaryDto>> GetUsersAsync(CancellationToken cancellationToken)
    {
        var users = await _authRepository.GetUsersAsync(cancellationToken);
        return users
            .Select(user => new UserSummaryDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Roles = user.Roles,
                IsActive = user.IsActive,
                CreatedAtUtc = user.CreatedAtUtc
            })
            .ToList();
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(Guid userId, string email, string fullName, CancellationToken cancellationToken)
    {
        var roles = await _authRepository.GetUserRolesAsync(userId, cancellationToken);
        var permissions = await _authRepository.GetUserPermissionsAsync(userId, cancellationToken);

        var (accessToken, accessTokenExpiresAtUtc) = _tokenService.GenerateAccessToken(userId, email, fullName, roles, permissions);

        var refresh = _tokenService.GenerateRefreshToken();
        await _authRepository.StoreRefreshTokenAsync(Guid.NewGuid(), userId, refresh.TokenHash, refresh.ExpiresAtUtc, cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refresh.PlainToken,
            AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
            RefreshTokenExpiresAtUtc = refresh.ExpiresAtUtc,
            Email = email,
            FullName = fullName,
            Roles = roles,
            Permissions = permissions
        };
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new InvalidOperationException("Email is required.");
        return email.Trim().ToLowerInvariant();
    }

    private static string HashRefreshToken(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token.Trim());
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
