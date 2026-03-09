using CMSAPI.Domain.Enums;

namespace CMSAPI.Application.DTOs.Claims;

public sealed class ClaimWorkflowTransitionDto
{
    public ClaimStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
}
