using CMSAPI.Application.Configuration;
using CMSAPI.Application.DTOs.Auth;
using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Domain.Entities;
using CMSAPI.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace CMSAPI.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<RefreshTokenRequestDto> _refreshValidator;
    private readonly IValidator<RevokeTokenRequestDto> _revokeValidator;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IAuthRepository authRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasherService passwordHasher,
        ITokenService tokenService,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<RefreshTokenRequestDto> refreshValidator,
        IValidator<RevokeTokenRequestDto> revokeValidator,
        IOptions<JwtOptions> jwtOptions)
    {
        _authRepository = authRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
        _revokeValidator = revokeValidator;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthUserDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        await _registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (await _authRepository.UserNameExistsAsync(request.UserName, cancellationToken))
        {
            throw new InvalidOperationException("Username already exists.");
        }

        if (await _authRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var role = await _authRepository.GetRoleByCodeAsync(request.RoleCode, cancellationToken)
            ?? throw new InvalidOperationException($"Role '{request.RoleCode}' is not valid.");

        var (hash, salt) = _passwordHasher.HashPassword(request.Password);
        var now = DateTime.UtcNow;

        var user = new AuthUser
        {
            UserName = request.UserName.Trim(),
            DisplayName = request.DisplayName.Trim(),
            Email = request.Email.Trim(),
            RoleId = role.RoleId,
            PasswordHash = hash,
            PasswordSalt = salt,
            LastPasswordChangedDate = now,
            CreatedDate = now,
            CreatedBy = request.UserName.Trim()
        };

        await _authRepository.AddUserAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var permissions = await _authRepository.GetPermissionsByRoleIdAsync(role.RoleId, cancellationToken);
        return ToUserDto(user, role, permissions);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, CancellationToken cancellationToken = default)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await _authRepository.GetUserByUserNameOrEmailAsync(request.UserNameOrEmail, cancellationToken);
        if (user is null || !user.IsActive || user.IsLocked)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var role = await _authRepository.GetRoleByIdAsync(user.RoleId, cancellationToken)
            ?? throw new InvalidOperationException("User role is not configured.");

        var permissions = await _authRepository.GetPermissionsByRoleIdAsync(role.RoleId, cancellationToken);

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes);
        var accessToken = _tokenService.GenerateAccessToken(user, role.RoleCode, permissions, expiresAt);

        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _tokenService.HashToken(rawRefreshToken);

        var refreshToken = new AuthRefreshToken
        {
            UserId = user.UserId,
            TokenHash = refreshTokenHash,
            ExpiresDate = now.AddDays(_jwtOptions.RefreshTokenExpiryDays),
            CreatedFromIp = ipAddress,
            CreatedBy = user.UserName,
            CreatedDate = now
        };

        await _authRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
        user.LastLoginDate = now;
        user.ModifiedDate = now;
        user.ModifiedBy = user.UserName;
        _authRepository.UpdateUser(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            ExpiresAtUtc = expiresAt,
            User = ToUserDto(user, role, permissions)
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string ipAddress, CancellationToken cancellationToken = default)
    {
        await _refreshValidator.ValidateAndThrowAsync(request, cancellationToken);

        var tokenHash = _tokenService.HashToken(request.RefreshToken);
        var existing = await _authRepository.GetRefreshTokenByHashAsync(tokenHash, cancellationToken);

        if (existing is null || !existing.IsActive || existing.RevokedDate.HasValue || existing.ExpiresDate <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token is invalid or expired.");
        }

        var user = await _authRepository.GetUserByIdAsync(existing.UserId, cancellationToken);
        if (user is null || !user.IsActive || user.IsLocked)
        {
            throw new UnauthorizedAccessException("User is invalid.");
        }

        var role = await _authRepository.GetRoleByIdAsync(user.RoleId, cancellationToken)
            ?? throw new InvalidOperationException("User role is not configured.");

        var permissions = await _authRepository.GetPermissionsByRoleIdAsync(role.RoleId, cancellationToken);

        var now = DateTime.UtcNow;
        var accessTokenExpires = now.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes);
        var accessToken = _tokenService.GenerateAccessToken(user, role.RoleCode, permissions, accessTokenExpires);

        var newRawRefreshToken = _tokenService.GenerateRefreshToken();
        var newHash = _tokenService.HashToken(newRawRefreshToken);

        existing.RevokedDate = now;
        existing.IsActive = false;
        existing.ReplacedByTokenHash = newHash;
        existing.ModifiedDate = now;
        existing.ModifiedBy = user.UserName;
        _authRepository.UpdateRefreshToken(existing);

        var replacement = new AuthRefreshToken
        {
            UserId = user.UserId,
            TokenHash = newHash,
            ExpiresDate = now.AddDays(_jwtOptions.RefreshTokenExpiryDays),
            CreatedFromIp = ipAddress,
            CreatedDate = now,
            CreatedBy = user.UserName
        };

        await _authRepository.AddRefreshTokenAsync(replacement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRawRefreshToken,
            ExpiresAtUtc = accessTokenExpires,
            User = ToUserDto(user, role, permissions)
        };
    }

    public async Task RevokeRefreshTokenAsync(RevokeTokenRequestDto request, string revokedBy, CancellationToken cancellationToken = default)
    {
        await _revokeValidator.ValidateAndThrowAsync(request, cancellationToken);

        var tokenHash = _tokenService.HashToken(request.RefreshToken);
        var refreshToken = await _authRepository.GetRefreshTokenByHashAsync(tokenHash, cancellationToken);
        if (refreshToken is null || !refreshToken.IsActive || refreshToken.RevokedDate.HasValue)
        {
            return;
        }

        refreshToken.RevokedDate = DateTime.UtcNow;
        refreshToken.IsActive = false;
        refreshToken.ModifiedDate = DateTime.UtcNow;
        refreshToken.ModifiedBy = revokedBy;
        _authRepository.UpdateRefreshToken(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static AuthUserDto ToUserDto(AuthUser user, AuthRole role, IReadOnlyList<string> permissions)
    {
        return new AuthUserDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            RoleCode = role.RoleCode,
            RoleName = role.RoleName,
            Permissions = permissions
        };
    }
}

