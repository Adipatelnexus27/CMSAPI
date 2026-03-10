namespace CMS.Application.DTOs;

public sealed class AuditReportSummaryDto
{
    public long TotalEvents { get; set; }
    public long SuccessfulEvents { get; set; }
    public long FailedEvents { get; set; }
    public long UserActions { get; set; }
    public long ClaimChanges { get; set; }
    public long ApiActivities { get; set; }
    public long DistinctUsers { get; set; }
    public long DistinctClaims { get; set; }
}
