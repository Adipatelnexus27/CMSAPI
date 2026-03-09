using CMSAPI.Application.Interfaces.Services;
using CMSAPI.Domain.Interfaces;
using CMSAPI.Infrastructure.Options;
using CMSAPI.Infrastructure.Persistence;
using CMSAPI.Infrastructure.Persistence.Repositories;
using CMSAPI.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CMSAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddOptions<FileStorageOptions>()
            .Bind(configuration.GetSection(FileStorageOptions.SectionName));

        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName));

        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<IClaimAssignmentRepository, ClaimAssignmentRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<ISystemConfigurationRepository, SystemConfigurationRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<AuthDataSeeder>();

        return services;
    }
}
