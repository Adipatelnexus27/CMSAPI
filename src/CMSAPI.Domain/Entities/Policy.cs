namespace CMSAPI.Domain.Entities;

public sealed class Policy
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
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

