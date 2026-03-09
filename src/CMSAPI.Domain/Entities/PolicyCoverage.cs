namespace CMSAPI.Domain.Entities;

public sealed class PolicyCoverage
{
    public long PolicyCoverageId { get; set; }
    public long PolicyId { get; set; }
    public long CoverageTypeId { get; set; }
    public decimal CoverageLimit { get; set; }
    public decimal? DeductibleAmount { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

