namespace CMSAPI.Domain.Entities;

public sealed class ClaimTypeMaster
{
    public long ClaimTypeId { get; set; }
    public string ClaimTypeCode { get; set; } = string.Empty;
    public string ClaimTypeName { get; set; } = string.Empty;
    public string? ClaimTypeDescription { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

