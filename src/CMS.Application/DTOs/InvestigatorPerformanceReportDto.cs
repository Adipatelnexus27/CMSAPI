namespace CMS.Application.DTOs;

public sealed class InvestigatorPerformanceReportDto
{
    public Guid InvestigatorUserId { get; set; }
    public string InvestigatorName { get; set; } = string.Empty;
    public int AssignedClaims { get; set; }
    public int ClosedClaims { get; set; }
    public decimal AverageInvestigationProgress { get; set; }
    public int TotalNotes { get; set; }
    public int FraudFlagsOnAssignedClaims { get; set; }
}
