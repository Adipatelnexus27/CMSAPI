namespace CMS.Application.DTOs;

public sealed class ClaimsByStatusReportDto
{
    public string ClaimStatus { get; set; } = string.Empty;
    public int ClaimCount { get; set; }
    public decimal PercentageOfTotal { get; set; }
}
