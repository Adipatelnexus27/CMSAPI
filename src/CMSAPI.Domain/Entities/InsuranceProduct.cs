namespace CMSAPI.Domain.Entities;

public sealed class InsuranceProduct
{
    public long PolicyTypeId { get; set; }
    public string PolicyTypeCode { get; set; } = string.Empty;
    public string PolicyTypeName { get; set; } = string.Empty;
    public string? PolicyTypeDescription { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

