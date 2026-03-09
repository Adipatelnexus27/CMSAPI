namespace CMSAPI.Application.DTOs.Claims;

public sealed class UploadClaimDocumentRequestDto
{
    public long DocumentTypeId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = [];
}
