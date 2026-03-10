namespace CMS.Domain.Entities;

public sealed class ClaimDocument
{
    public Guid ClaimDocumentId { get; set; }
    public Guid ClaimId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}
