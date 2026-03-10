namespace CMS.Application.DTOs;

public sealed class ClaimDetailDto
{
    public Guid ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimStatus { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string WorkflowStep { get; set; } = string.Empty;
    public Guid? InvestigatorUserId { get; set; }
    public Guid? AdjusterUserId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public DateTime IncidentDateUtc { get; set; }
    public string IncidentLocation { get; set; } = string.Empty;
    public string IncidentDescription { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public int InvestigationProgress { get; set; }
    public IReadOnlyList<InvestigationNoteDto> InvestigationNotes { get; set; } = [];
    public IReadOnlyList<ClaimDocumentDto> Documents { get; set; } = [];
    public IReadOnlyList<RelatedClaimDto> RelatedClaims { get; set; } = [];
    public IReadOnlyList<ClaimWorkflowHistoryDto> WorkflowHistory { get; set; } = [];
}
