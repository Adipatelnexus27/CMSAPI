using System.Diagnostics;
using System.Security.Claims;
using CMS.Application.DTOs;
using CMS.Application.Interfaces.Services;

namespace CMS.API.Middlewares;

public sealed class AuditTrailMiddleware
{
    private static readonly string[] ClaimChangePrefixes =
    [
        "/api/claims",
        "/api/claimreserves",
        "/api/claimsettlements"
    ];

    private readonly RequestDelegate _next;

    public AuditTrailMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (path.StartsWith("/api/auditlogs", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        Exception? exception = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            var statusCode = ResolveStatusCode(context.Response.StatusCode, exception);
            var method = context.Request.Method.ToUpperInvariant();
            var eventType = ResolveEventType(path, method);
            var (entityName, entityId, claimId) = ResolveEntityDetails(context, path);

            var request = new CreateAuditLogRequestDto
            {
                EventType = eventType,
                ActionName = $"{method} {path}",
                EntityName = entityName,
                EntityId = entityId,
                ClaimId = claimId,
                Description = exception?.Message,
                RequestMethod = method,
                RequestPath = path,
                RequestQuery = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                HttpStatusCode = statusCode,
                IsSuccess = statusCode is >= 200 and < 400,
                DurationMs = stopwatch.ElapsedMilliseconds > int.MaxValue ? int.MaxValue : (int)stopwatch.ElapsedMilliseconds,
                CorrelationId = ResolveCorrelationId(context)
            };

            try
            {
                await auditService.CreateAuditLogAsync(
                    request,
                    ResolveUserId(context.User),
                    ResolveUserEmail(context.User),
                    ResolveUserRolesCsv(context.User),
                    context.Connection.RemoteIpAddress?.ToString(),
                    context.Request.Headers.UserAgent.ToString(),
                    request.CorrelationId,
                    CancellationToken.None);
            }
            catch
            {
                // Audit logging must never break the primary request.
            }
        }
    }

    private static string ResolveEventType(string path, string method)
    {
        var isClaimChange = ClaimChangePrefixes.Any(prefix =>
            path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            && method is not "GET" and not "HEAD" and not "OPTIONS";

        return isClaimChange ? "ClaimChange" : "ApiActivity";
    }

    private static int ResolveStatusCode(int responseStatusCode, Exception? exception)
    {
        if (responseStatusCode > 0)
        {
            return responseStatusCode;
        }

        if (exception is UnauthorizedAccessException)
        {
            return StatusCodes.Status401Unauthorized;
        }

        if (exception is InvalidOperationException)
        {
            return StatusCodes.Status400BadRequest;
        }

        return exception is null ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;
    }

    private static (string? EntityName, Guid? EntityId, Guid? ClaimId) ResolveEntityDetails(HttpContext context, string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var entityName = segments.Length >= 2 ? segments[1] : null;
        Guid? entityId = null;
        Guid? claimId = null;

        foreach (var routeValue in context.Request.RouteValues)
        {
            if (routeValue.Value is null)
            {
                continue;
            }

            if (!Guid.TryParse(routeValue.Value.ToString(), out var parsedGuid))
            {
                continue;
            }

            entityId ??= parsedGuid;
            if (string.Equals(routeValue.Key, "claimId", StringComparison.OrdinalIgnoreCase))
            {
                claimId = parsedGuid;
            }
        }

        if (!claimId.HasValue && context.Request.Query.TryGetValue("claimId", out var claimIdQuery))
        {
            if (Guid.TryParse(claimIdQuery.ToString(), out var parsedClaimId))
            {
                claimId = parsedClaimId;
            }
        }

        if (!claimId.HasValue)
        {
            claimId = TryExtractClaimIdFromPath(path);
        }

        return (entityName, entityId, claimId);
    }

    private static Guid? TryExtractClaimIdFromPath(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        for (var index = 0; index < segments.Length - 1; index++)
        {
            if (!string.Equals(segments[index], "claims", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (Guid.TryParse(segments[index + 1], out var claimId))
            {
                return claimId;
            }
        }

        return null;
    }

    private static Guid? ResolveUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }

    private static string? ResolveUserEmail(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Email)
            ?? user.FindFirstValue("email");
    }

    private static string? ResolveUserRolesCsv(ClaimsPrincipal user)
    {
        var roles = user.Claims
            .Where(claim => claim.Type == ClaimTypes.Role || claim.Type == "role")
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return roles.Length == 0 ? null : string.Join(",", roles);
    }

    private static Guid? ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var headerValue)
            && Guid.TryParse(headerValue.ToString(), out var parsedHeader))
        {
            return parsedHeader;
        }

        return Guid.TryParse(context.TraceIdentifier, out var parsedTraceIdentifier)
            ? parsedTraceIdentifier
            : null;
    }
}
