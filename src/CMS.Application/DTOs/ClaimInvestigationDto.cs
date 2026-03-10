namespace CMS.Application.DTOs;

public sealed class ClaimInvestigationDto
{
    public Guid ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string ClaimStatus { get; set; } = string.Empty;
    public int InvestigationProgress { get; set; }
    public IReadOnlyList<ClaimDocumentDto> Documents { get; set; } = [];
    public IReadOnlyList<InvestigationNoteDto> Notes { get; set; } = [];
}
