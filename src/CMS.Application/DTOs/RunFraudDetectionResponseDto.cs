namespace CMS.Application.DTOs;

public sealed class RunFraudDetectionResponseDto
{
    public Guid ClaimId { get; set; }
    public IReadOnlyList<FraudFlagDto> Flags { get; set; } = [];
}
