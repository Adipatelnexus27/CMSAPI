namespace CMSAPI.Domain.Entities;

public sealed class ClaimAssignment
{
    public long ClaimAssignmentId { get; set; }
    public long ClaimId { get; set; }
    public long AssignedToUserId { get; set; }
    public long AssignedByUserId { get; set; }
    public DateTime AssignmentDate { get; set; }
    public string? AssignmentReason { get; set; }
    public bool IsCurrent { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}
