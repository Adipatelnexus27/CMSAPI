namespace CMS.Application.DTOs;

public sealed class ClaimsByProductReportDto
{
    public string ProductCode { get; set; } = string.Empty;
    public int ClaimCount { get; set; }
    public int OpenClaims { get; set; }
    public int ClosedClaims { get; set; }
}
