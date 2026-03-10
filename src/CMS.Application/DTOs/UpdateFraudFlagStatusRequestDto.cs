namespace CMS.Application.DTOs;

public sealed class UpdateFraudFlagStatusRequestDto
{
    public string Status { get; set; } = string.Empty;
    public string? ReviewNote { get; set; }
}
