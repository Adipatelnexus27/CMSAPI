namespace CMS.Application.DTOs;

public sealed class CreateAuditLogRequestDto
{
    public string EventType { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string? EntityName { get; set; }
    public Guid? EntityId { get; set; }
    public Guid? ClaimId { get; set; }
    public string? Description { get; set; }
    public string? RequestMethod { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestQuery { get; set; }
    public int? HttpStatusCode { get; set; }
    public bool IsSuccess { get; set; } = true;
    public int? DurationMs { get; set; }
    public Guid? CorrelationId { get; set; }
}
