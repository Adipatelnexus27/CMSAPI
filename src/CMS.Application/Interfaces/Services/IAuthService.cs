using CMS.Application.DTOs;

namespace CMS.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);
    Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken);
    Task RevokeAsync(RevokeTokenRequestDto request, CancellationToken cancellationToken);
}
