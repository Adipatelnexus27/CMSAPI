namespace CMSAPI.Application.DTOs.Policies;

public sealed class CreatePolicyRequestDto
{
    public string PolicyNumber { get; set; } = string.Empty;
    public long PolicyTypeId { get; set; }
    public string InsuredName { get; set; } = string.Empty;
    public DateTime PolicyStartDate { get; set; }
    public DateTime PolicyEndDate { get; set; }
    public decimal SumInsured { get; set; }
    public long CurrencyId { get; set; }
    public string PolicyStatus { get; set; } = "Active";
    public IReadOnlyList<CreatePolicyCoverageRequestDto> Coverages { get; set; } = [];
}

