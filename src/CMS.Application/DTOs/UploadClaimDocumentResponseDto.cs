namespace CMS.Application.DTOs;

public sealed class UploadClaimDocumentResponseDto
{
    public Guid ClaimDocumentId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}
