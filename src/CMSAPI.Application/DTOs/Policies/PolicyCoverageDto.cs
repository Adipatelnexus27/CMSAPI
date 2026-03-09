namespace CMSAPI.Application.DTOs.Policies;

public sealed class PolicyCoverageDto
{
    public long PolicyCoverageId { get; set; }
    public long CoverageTypeId { get; set; }
    public string CoverageTypeName { get; set; } = string.Empty;
    public decimal CoverageLimit { get; set; }
    public decimal? DeductibleAmount { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }
}

