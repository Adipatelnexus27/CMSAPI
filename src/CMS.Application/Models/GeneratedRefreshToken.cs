namespace CMS.Application.Models;

public sealed class GeneratedRefreshToken
{
    public string PlainToken { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
