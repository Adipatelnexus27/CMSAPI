using System.Security.Claims;
using CMSAPI.Application.Security;

namespace CMSAPI.API.Middleware;

public sealed class CurrentUserContextMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(CustomClaimTypes.UserId) ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = context.User.FindFirstValue(ClaimTypes.Name);
            var role = context.User.FindFirstValue(ClaimTypes.Role);

            context.Items["CurrentUserId"] = userId;
            context.Items["CurrentUserName"] = userName;
            context.Items["CurrentUserRole"] = role;
        }

        await _next(context);
    }
}

