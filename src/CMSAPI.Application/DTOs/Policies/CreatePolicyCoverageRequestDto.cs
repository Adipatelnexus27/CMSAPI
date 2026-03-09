namespace CMSAPI.Application.DTOs.Policies;

public sealed class CreatePolicyCoverageRequestDto
{
    public long CoverageTypeId { get; set; }
    public decimal CoverageLimit { get; set; }
    public decimal? DeductibleAmount { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }
}

