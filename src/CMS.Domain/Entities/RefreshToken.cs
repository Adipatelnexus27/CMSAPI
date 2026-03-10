namespace CMS.Domain.Entities;

public sealed class RefreshToken
{
    public Guid RefreshTokenId { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? RevokedReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
