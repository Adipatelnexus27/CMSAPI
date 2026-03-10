using System.Text;
using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Application.Interfaces.Services;
using CMS.Application.Models;

namespace CMS.Application.Services;

public sealed class ReportingService : IReportingService
{
    private const string ClaimsByStatusType = "ClaimsByStatus";
    private const string ClaimsByProductType = "ClaimsByProduct";
    private const string FraudType = "Fraud";
    private const string InvestigatorPerformanceType = "InvestigatorPerformance";
    private const string SettlementType = "Settlement";

    private static readonly HashSet<string> AllowedReportTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ClaimsByStatusType,
        ClaimsByProductType,
        FraudType,
        InvestigatorPerformanceType,
        SettlementType
    };

    private readonly IReportingRepository _reportingRepository;

    public ReportingService(IReportingRepository reportingRepository)
    {
        _reportingRepository = reportingRepository;
    }

    public async Task<IReadOnlyList<ClaimsByStatusReportDto>> GetClaimsByStatusAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        ValidateDateRange(fromDateUtc, toDateUtc);
        return await _reportingRepository.GetClaimsByStatusAsync(fromDateUtc, toDateUtc, cancellationToken);
    }

    public async Task<IReadOnlyList<ClaimsByProductReportDto>> GetClaimsByProductAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        ValidateDateRange(fromDateUtc, toDateUtc);
        return await _reportingRepository.GetClaimsByProductAsync(fromDateUtc, toDateUtc, cancellationToken);
    }

    public async Task<IReadOnlyList<FraudReportDto>> GetFraudReportAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        ValidateDateRange(fromDateUtc, toDateUtc);
        return await _reportingRepository.GetFraudReportAsync(fromDateUtc, toDateUtc, cancellationToken);
    }

    public async Task<IReadOnlyList<InvestigatorPerformanceReportDto>> GetInvestigatorPerformanceReportAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        ValidateDateRange(fromDateUtc, toDateUtc);
        return await _reportingRepository.GetInvestigatorPerformanceReportAsync(fromDateUtc, toDateUtc, cancellationToken);
    }

    public async Task<IReadOnlyList<SettlementReportDto>> GetSettlementReportAsync(DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        ValidateDateRange(fromDateUtc, toDateUtc);
        return await _reportingRepository.GetSettlementReportAsync(fromDateUtc, toDateUtc, cancellationToken);
    }

    public async Task<ReportExportFile> ExportExcelAsync(string reportType, DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        ValidateDateRange(fromDateUtc, toDateUtc);
        var normalizedType = NormalizeReportType(reportType);
        var table = await BuildReportTableAsync(normalizedType, fromDateUtc, toDateUtc, cancellationToken);
        var csvBytes = BuildCsv(table.Headers, table.Rows);

        return new ReportExportFile
        {
            Content = csvBytes,
            ContentType = "application/vnd.ms-excel",
            FileName = $"report-{normalizedType}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv"
        };
    }

    public async Task<ReportExportFile> ExportPdfAsync(string reportType, DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        ValidateDateRange(fromDateUtc, toDateUtc);
        var normalizedType = NormalizeReportType(reportType);
        var table = await BuildReportTableAsync(normalizedType, fromDateUtc, toDateUtc, cancellationToken);
        var lines = BuildPdfLines(table);
        var pdfContent = BuildSimplePdf(lines);

        return new ReportExportFile
        {
            Content = pdfContent,
            ContentType = "application/pdf",
            FileName = $"report-{normalizedType}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf"
        };
    }

    private async Task<ReportTable> BuildReportTableAsync(string reportType, DateTime? fromDateUtc, DateTime? toDateUtc, CancellationToken cancellationToken)
    {
        if (string.Equals(reportType, ClaimsByStatusType, StringComparison.OrdinalIgnoreCase))
        {
            var rows = await _reportingRepository.GetClaimsByStatusAsync(fromDateUtc, toDateUtc, cancellationToken);
            return new ReportTable(
                "Claims By Status",
                ["Claim Status", "Claim Count", "Percentage Of Total"],
                rows.Select(row => new[] { row.ClaimStatus, row.ClaimCount.ToString(), row.PercentageOfTotal.ToString("0.00") }).ToList());
        }

        if (string.Equals(reportType, ClaimsByProductType, StringComparison.OrdinalIgnoreCase))
        {
            var rows = await _reportingRepository.GetClaimsByProductAsync(fromDateUtc, toDateUtc, cancellationToken);
            return new ReportTable(
                "Claims By Product",
                ["Product Code", "Claim Count", "Open Claims", "Closed Claims"],
                rows.Select(row => new[] { row.ProductCode, row.ClaimCount.ToString(), row.OpenClaims.ToString(), row.ClosedClaims.ToString() }).ToList());
        }

        if (string.Equals(reportType, FraudType, StringComparison.OrdinalIgnoreCase))
        {
            var rows = await _reportingRepository.GetFraudReportAsync(fromDateUtc, toDateUtc, cancellationToken);
            return new ReportTable(
                "Fraud Report",
                ["Fraud Status", "Flag Count", "Duplicate Flags", "Suspicious Flags", "Average Severity"],
                rows.Select(row => new[] { row.FraudStatus, row.FlagCount.ToString(), row.DuplicateFlags.ToString(), row.SuspiciousFlags.ToString(), row.AverageSeverityScore.ToString("0.00") }).ToList());
        }

        if (string.Equals(reportType, InvestigatorPerformanceType, StringComparison.OrdinalIgnoreCase))
        {
            var rows = await _reportingRepository.GetInvestigatorPerformanceReportAsync(fromDateUtc, toDateUtc, cancellationToken);
            return new ReportTable(
                "Investigator Performance",
                ["Investigator", "Assigned Claims", "Closed Claims", "Average Progress", "Total Notes", "Fraud Flags"],
                rows.Select(row => new[]
                {
                    row.InvestigatorName,
                    row.AssignedClaims.ToString(),
                    row.ClosedClaims.ToString(),
                    row.AverageInvestigationProgress.ToString("0.00"),
                    row.TotalNotes.ToString(),
                    row.FraudFlagsOnAssignedClaims.ToString()
                }).ToList());
        }

        var settlementRows = await _reportingRepository.GetSettlementReportAsync(fromDateUtc, toDateUtc, cancellationToken);
        return new ReportTable(
            "Settlement Report",
            ["Payment Status", "Payment Count", "Total Payment Amount", "Average Payment Amount"],
            settlementRows.Select(row => new[]
            {
                row.PaymentStatus,
                row.PaymentCount.ToString(),
                row.TotalPaymentAmount.ToString("0.00"),
                row.AveragePaymentAmount.ToString("0.00")
            }).ToList());
    }

    private static byte[] BuildCsv(IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(",", headers.Select(EscapeCsvValue)));

        foreach (var row in rows)
        {
            builder.AppendLine(string.Join(",", row.Select(EscapeCsvValue)));
        }

        var content = Encoding.UTF8.GetBytes(builder.ToString());
        var preamble = Encoding.UTF8.GetPreamble();
        return preamble.Concat(content).ToArray();
    }

    private static IReadOnlyList<string> BuildPdfLines(ReportTable table)
    {
        var lines = new List<string>
        {
            table.Title,
            $"Generated (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
            string.Empty,
            string.Join(" | ", table.Headers),
            new string('-', 90)
        };

        foreach (var row in table.Rows.Take(65))
        {
            lines.Add(string.Join(" | ", row));
        }

        return lines;
    }

    private static byte[] BuildSimplePdf(IReadOnlyList<string> lines)
    {
        var textBuilder = new StringBuilder();
        textBuilder.AppendLine("BT");
        textBuilder.AppendLine("/F1 10 Tf");
        textBuilder.AppendLine("40 800 Td");

        var isFirstLine = true;
        foreach (var line in lines)
        {
            var escaped = EscapePdfText(line);
            if (!isFirstLine)
            {
                textBuilder.AppendLine("0 -12 Td");
            }

            textBuilder.AppendLine($"({escaped}) Tj");
            isFirstLine = false;
        }

        textBuilder.AppendLine("ET");

        var streamContent = Encoding.ASCII.GetBytes(textBuilder.ToString());

        using var stream = new MemoryStream();
        var offsets = new List<long>();

        WriteAscii(stream, "%PDF-1.4\n");

        offsets.Add(stream.Position);
        WriteAscii(stream, "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        offsets.Add(stream.Position);
        WriteAscii(stream, "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

        offsets.Add(stream.Position);
        WriteAscii(stream, "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n");

        offsets.Add(stream.Position);
        WriteAscii(stream, "4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");

        offsets.Add(stream.Position);
        WriteAscii(stream, $"5 0 obj\n<< /Length {streamContent.Length} >>\nstream\n");
        stream.Write(streamContent, 0, streamContent.Length);
        WriteAscii(stream, "endstream\nendobj\n");

        var xrefPosition = stream.Position;
        WriteAscii(stream, "xref\n0 6\n0000000000 65535 f \n");
        foreach (var offset in offsets)
        {
            WriteAscii(stream, $"{offset:D10} 00000 n \n");
        }

        WriteAscii(stream, "trailer\n<< /Size 6 /Root 1 0 R >>\n");
        WriteAscii(stream, $"startxref\n{xrefPosition}\n%%EOF");

        return stream.ToArray();
    }

    private static void WriteAscii(Stream stream, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static string EscapeCsvValue(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }

    private static string EscapePdfText(string value)
    {
        var ascii = new string(value.Where(character => character <= 126).ToArray());
        return ascii
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }

    private static void ValidateDateRange(DateTime? fromDateUtc, DateTime? toDateUtc)
    {
        if (fromDateUtc.HasValue && toDateUtc.HasValue && fromDateUtc.Value > toDateUtc.Value)
        {
            throw new InvalidOperationException("From date must be earlier than or equal to To date.");
        }
    }

    private static string NormalizeReportType(string reportType)
    {
        if (string.IsNullOrWhiteSpace(reportType))
        {
            throw new InvalidOperationException("Report type is required.");
        }

        var normalized = reportType.Trim();
        if (!AllowedReportTypes.Contains(normalized))
        {
            throw new InvalidOperationException("Invalid report type. Allowed values: ClaimsByStatus, ClaimsByProduct, Fraud, InvestigatorPerformance, Settlement.");
        }

        return normalized;
    }

    private sealed record ReportTable(string Title, string[] Headers, List<string[]> Rows);
}
