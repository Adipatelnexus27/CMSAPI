namespace CMS.Application.DTOs;

public sealed class RevokeTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
    public string Reason { get; set; } = "Manual logout";
}
