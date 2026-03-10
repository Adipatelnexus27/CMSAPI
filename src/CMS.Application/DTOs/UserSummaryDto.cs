namespace CMS.Application.DTOs;

public sealed class UserSummaryDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = [];
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

