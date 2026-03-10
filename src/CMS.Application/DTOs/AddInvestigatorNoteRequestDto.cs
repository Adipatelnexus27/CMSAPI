namespace CMS.Application.DTOs;

public sealed class AddInvestigatorNoteRequestDto
{
    public string NoteText { get; set; } = string.Empty;
    public int? ProgressPercentSnapshot { get; set; }
}
