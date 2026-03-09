namespace CMSAPI.Application.DTOs.Auth;

public sealed class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}

