namespace CMSAPI.Domain.Entities;

public sealed class ClaimDocument
{
    public long ClaimDocumentId { get; set; }
    public long ClaimId { get; set; }
    public long DocumentTypeId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileHash { get; set; }
    public DateTime UploadedDate { get; set; }
    public long UploadedByUserId { get; set; }
    public int VersionNo { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}
