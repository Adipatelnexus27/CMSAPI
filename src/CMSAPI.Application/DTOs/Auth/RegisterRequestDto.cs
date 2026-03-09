namespace CMSAPI.Application.DTOs.Auth;

public sealed class RegisterRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
}

