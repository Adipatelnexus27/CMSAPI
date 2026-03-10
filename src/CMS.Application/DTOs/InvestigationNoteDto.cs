namespace CMS.Application.DTOs;

public sealed class InvestigationNoteDto
{
    public Guid ClaimInvestigationNoteId { get; set; }
    public Guid ClaimId { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public int? ProgressPercentSnapshot { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
