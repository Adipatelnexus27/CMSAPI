namespace CMSAPI.Application.DTOs.Policies;

public sealed class PolicyDto
{
    public long PolicyId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public long PolicyTypeId { get; set; }
    public string InsuredName { get; set; } = string.Empty;
    public DateTime PolicyStartDate { get; set; }
    public DateTime PolicyEndDate { get; set; }
    public decimal SumInsured { get; set; }
    public long CurrencyId { get; set; }
    public string PolicyStatus { get; set; } = string.Empty;
    public IReadOnlyList<PolicyCoverageDto> Coverages { get; set; } = [];
}

