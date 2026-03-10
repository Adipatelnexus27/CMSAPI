namespace CMS.Application.DTOs;

public sealed class RelatedClaimDto
{
    public Guid ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string ClaimStatus { get; set; } = string.Empty;
}
