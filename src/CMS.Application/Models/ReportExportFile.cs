namespace CMS.Application.Models;

public sealed class ReportExportFile
{
    public required byte[] Content { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}
