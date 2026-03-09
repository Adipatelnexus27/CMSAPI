namespace CMSAPI.Application.DTOs.Auth;

public sealed class RevokeTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

