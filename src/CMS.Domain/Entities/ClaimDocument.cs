namespace CMS.Domain.Entities;

public sealed class ClaimDocument
{
    public Guid ClaimDocumentId { get; set; }
    public Guid ClaimId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string DocumentCategory { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public Guid DocumentGroupId { get; set; }
    public int VersionNumber { get; set; }
    public bool IsLatest { get; set; }
    public Guid? UploadedByUserId { get; set; }
}
