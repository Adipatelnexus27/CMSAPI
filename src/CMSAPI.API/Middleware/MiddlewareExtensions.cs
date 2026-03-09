namespace CMSAPI.API.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    public static IApplicationBuilder UseCurrentUserContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CurrentUserContextMiddleware>();
    }
}
