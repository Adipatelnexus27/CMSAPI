using CMSAPI.Domain.Enums;

namespace CMSAPI.Application.DTOs.ClaimAssignment;

public sealed class TriageClaimRequestDto
{
    public ClaimPriority Priority { get; set; } = ClaimPriority.Medium;
    public string? Notes { get; set; }
}

public sealed class AssignClaimRoleRequestDto
{
    public long? UserId { get; set; }
    public string? AssignmentReason { get; set; }
}
