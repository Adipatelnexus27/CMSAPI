namespace CMS.Application.DTOs;

public sealed class AuditLogFilterDto
{
    public DateTime? FromDateUtc { get; set; }
    public DateTime? ToDateUtc { get; set; }
    public string? EventType { get; set; }
    public Guid? UserId { get; set; }
    public Guid? ClaimId { get; set; }
    public bool? IsSuccess { get; set; }
    public string? ActionContains { get; set; }
    public int? Take { get; set; }
}
