using System.Text;
using CMS.Application.DTOs;
using CMS.Application.Interfaces.Repositories;
using CMS.Application.Interfaces.Services;
using CMS.Application.Models;

namespace CMS.Application.Services;

public sealed class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;

    public AuditService(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task CreateAuditLogAsync(
        CreateAuditLogRequestDto request,
        Guid? userId,
        string? userEmail,
        string? userRoleCsv,
        string? ipAddress,
        string? userAgent,
        Guid? correlationId,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizeCreateRequest(request);
        await _auditRepository.CreateAuditLogAsync(
            normalized,
            userId,
            Truncate(userEmail, 320),
            Truncate(userRoleCsv, 500),
            Truncate(ipAddress, 100),
            Truncate(userAgent, 500),
            correlationId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetAuditLogsAsync(AuditLogFilterDto filter, CancellationToken cancellationToken)
    {
        var normalizedFilter = NormalizeFilter(filter);
        return await _auditRepository.GetAuditLogsAsync(normalizedFilter, cancellationToken);
    }

    public async Task<AuditReportSummaryDto> GetAuditReportAsync(AuditLogFilterDto filter, CancellationToken cancellationToken)
    {
        var normalizedFilter = NormalizeFilter(filter);
        return await _auditRepository.GetAuditReportAsync(normalizedFilter, cancellationToken);
    }

    public async Task<ReportExportFile> ExportExcelAsync(AuditLogFilterDto filter, CancellationToken cancellationToken)
    {
        var normalizedFilter = NormalizeFilter(filter);
        var rows = await _auditRepository.GetAuditLogsAsync(normalizedFilter, cancellationToken);
        var csv = BuildCsv(rows);

        return new ReportExportFile
        {
            Content = csv,
            ContentType = "application/vnd.ms-excel",
            FileName = $"audit-report-{DateTime.UtcNow:yyyyMMddHHmmss}.csv"
        };
    }

    public async Task<ReportExportFile> ExportPdfAsync(AuditLogFilterDto filter, CancellationToken cancellationToken)
    {
        var normalizedFilter = NormalizeFilter(filter);
        var rows = await _auditRepository.GetAuditLogsAsync(normalizedFilter, cancellationToken);
        var pdf = BuildSimplePdf(rows);

        return new ReportExportFile
        {
            Content = pdf,
            ContentType = "application/pdf",
            FileName = $"audit-report-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf"
        };
    }

    private static CreateAuditLogRequestDto NormalizeCreateRequest(CreateAuditLogRequestDto request)
    {
        if (request is null)
        {
            throw new InvalidOperationException("Audit log request is required.");
        }

        var eventType = string.IsNullOrWhiteSpace(request.EventType)
            ? "UserAction"
            : request.EventType.Trim();

        var actionName = request.ActionName?.Trim();
        if (string.IsNullOrWhiteSpace(actionName))
        {
            throw new InvalidOperationException("Audit ActionName is required.");
        }

        return new CreateAuditLogRequestDto
        {
            EventType = Truncate(eventType, 50)!,
            ActionName = Truncate(actionName, 200)!,
            EntityName = Truncate(request.EntityName, 100),
            EntityId = request.EntityId,
            ClaimId = request.ClaimId,
            Description = Truncate(request.Description, 2000),
            RequestMethod = Truncate(request.RequestMethod, 10),
            RequestPath = Truncate(request.RequestPath, 500),
            RequestQuery = Truncate(request.RequestQuery, 1000),
            HttpStatusCode = request.HttpStatusCode,
            IsSuccess = request.IsSuccess,
            DurationMs = request.DurationMs is < 0 ? null : request.DurationMs,
            CorrelationId = request.CorrelationId
        };
    }

    private static AuditLogFilterDto NormalizeFilter(AuditLogFilterDto? filter)
    {
        var effectiveFilter = filter ?? new AuditLogFilterDto();

        if (effectiveFilter.FromDateUtc.HasValue
            && effectiveFilter.ToDateUtc.HasValue
            && effectiveFilter.FromDateUtc.Value > effectiveFilter.ToDateUtc.Value)
        {
            throw new InvalidOperationException("From date must be earlier than or equal to To date.");
        }

        var take = effectiveFilter.Take.GetValueOrDefault(500);
        if (take < 1)
        {
            take = 500;
        }

        if (take > 2000)
        {
            take = 2000;
        }

        return new AuditLogFilterDto
        {
            FromDateUtc = effectiveFilter.FromDateUtc,
            ToDateUtc = effectiveFilter.ToDateUtc,
            EventType = string.IsNullOrWhiteSpace(effectiveFilter.EventType) ? null : Truncate(effectiveFilter.EventType.Trim(), 50),
            UserId = effectiveFilter.UserId,
            ClaimId = effectiveFilter.ClaimId,
            IsSuccess = effectiveFilter.IsSuccess,
            ActionContains = string.IsNullOrWhiteSpace(effectiveFilter.ActionContains) ? null : Truncate(effectiveFilter.ActionContains.Trim(), 200),
            Take = take
        };
    }

    private static byte[] BuildCsv(IReadOnlyList<AuditLogDto> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine("CreatedAtUtc,EventType,ActionName,ClaimId,UserEmail,IsSuccess,HttpStatusCode,DurationMs,RequestMethod,RequestPath,Description");

        foreach (var row in rows)
        {
            var values = new[]
            {
                row.CreatedAtUtc.ToString("O"),
                row.EventType,
                row.ActionName,
                row.ClaimId?.ToString() ?? string.Empty,
                row.UserEmail ?? string.Empty,
                row.IsSuccess ? "true" : "false",
                row.HttpStatusCode?.ToString() ?? string.Empty,
                row.DurationMs?.ToString() ?? string.Empty,
                row.RequestMethod ?? string.Empty,
                row.RequestPath ?? string.Empty,
                row.Description ?? string.Empty
            };

            builder.AppendLine(string.Join(",", values.Select(EscapeCsvValue)));
        }

        var content = Encoding.UTF8.GetBytes(builder.ToString());
        return Encoding.UTF8.GetPreamble().Concat(content).ToArray();
    }

    private static byte[] BuildSimplePdf(IReadOnlyList<AuditLogDto> rows)
    {
        var lines = new List<string>
        {
            "Audit Trail Report",
            $"Generated (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
            string.Empty,
            "CreatedAtUtc | EventType | ActionName | Success | Status | ClaimId",
            new string('-', 120)
        };

        foreach (var row in rows.Take(65))
        {
            lines.Add(
                $"{row.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} | {TruncateForPdf(row.EventType, 16)} | {TruncateForPdf(row.ActionName, 45)} | {(row.IsSuccess ? "Y" : "N")} | {row.HttpStatusCode?.ToString() ?? "-"} | {row.ClaimId?.ToString() ?? "-"}");
        }

        var textBuilder = new StringBuilder();
        textBuilder.AppendLine("BT");
        textBuilder.AppendLine("/F1 10 Tf");
        textBuilder.AppendLine("40 800 Td");

        var isFirstLine = true;
        foreach (var line in lines)
        {
            if (!isFirstLine)
            {
                textBuilder.AppendLine("0 -12 Td");
            }

            textBuilder.AppendLine($"({EscapePdfText(line)}) Tj");
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

    private static string TruncateForPdf(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
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

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
