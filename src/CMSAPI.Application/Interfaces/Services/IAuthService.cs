using CMSAPI.Application.DTOs.Auth;

namespace CMSAPI.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthUserDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string ipAddress, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(RevokeTokenRequestDto request, string revokedBy, CancellationToken cancellationToken = default);
}

