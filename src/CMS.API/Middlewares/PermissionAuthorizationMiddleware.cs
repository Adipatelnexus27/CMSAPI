namespace CMS.API.Middlewares;

public sealed class PermissionAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public PermissionAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var permissions = endpoint?.Metadata.GetOrderedMetadata<RequirePermissionAttribute>()
            .Select(x => x.Permission)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (permissions is { Length: > 0 })
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var granted = context.User.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var missing = permissions.Where(permission => !granted.Contains(permission)).ToArray();
            if (missing.Length > 0)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "Permission denied.", missingPermissions = missing });
                return;
            }
        }

        await _next(context);
    }
}
