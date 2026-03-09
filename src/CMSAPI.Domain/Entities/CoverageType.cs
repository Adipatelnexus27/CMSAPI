namespace CMSAPI.Domain.Entities;

public sealed class CoverageType
{
    public long CoverageTypeId { get; set; }
    public string CoverageCode { get; set; } = string.Empty;
    public string CoverageName { get; set; } = string.Empty;
    public string? CoverageDescription { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

