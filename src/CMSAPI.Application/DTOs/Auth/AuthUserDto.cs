namespace CMSAPI.Application.DTOs.Auth;

public sealed class AuthUserDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public IReadOnlyList<string> Permissions { get; set; } = [];
}

