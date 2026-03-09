using System.Reflection;
using CMSAPI.Application.BusinessRules;
using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CMSAPI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<ClaimBusinessRules>();
        services.AddScoped<IClaimService, ClaimService>();

        return services;
    }
}

