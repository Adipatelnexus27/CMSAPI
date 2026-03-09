using CMSAPI.Domain.Enums;

namespace CMSAPI.Application.DTOs.ClaimAssignment;

public sealed class ClaimAssignmentDashboardDto
{
    public DateTime GeneratedAtUtc { get; set; }
    public IReadOnlyList<ClaimTriageItemDto> TriageQueue { get; set; } = [];
    public IReadOnlyList<AssigneeWorkloadDto> Investigators { get; set; } = [];
    public IReadOnlyList<AssigneeWorkloadDto> Adjusters { get; set; } = [];
}

public sealed class ClaimTriageItemDto
{
    public long ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClaimantName { get; set; } = string.Empty;
    public DateTime LossDateUtc { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public ClaimPriority Priority { get; set; } = ClaimPriority.Medium;
    public decimal EstimatedLossAmount { get; set; }
    public AssignmentSummaryDto? Investigator { get; set; }
    public AssignmentSummaryDto? Adjuster { get; set; }
}

public sealed class AssigneeWorkloadDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public int ActiveAssignments { get; set; }
}

public sealed class AssignmentSummaryDto
{
    public long UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public DateTime AssignedAtUtc { get; set; }
}

public sealed class AssignmentResultDto
{
    public long ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string AssignmentRole { get; set; } = string.Empty;
    public long AssignedToUserId { get; set; }
    public string AssignedToDisplayName { get; set; } = string.Empty;
    public DateTime AssignedAtUtc { get; set; }
    public int AssignedUserWorkload { get; set; }
}
