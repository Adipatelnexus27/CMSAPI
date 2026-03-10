namespace CMS.Application.Models;

public sealed class DocumentPreviewResult
{
    public required Stream ContentStream { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}
