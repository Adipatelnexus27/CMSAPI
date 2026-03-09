namespace CMSAPI.Domain.Entities;

public sealed class ClaimNote
{
    public long ClaimNoteId { get; set; }
    public long ClaimId { get; set; }
    public string NoteCategory { get; set; } = string.Empty;
    public string NoteText { get; set; } = string.Empty;
    public DateTime NotedDate { get; set; }
    public long NotedByUserId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}
