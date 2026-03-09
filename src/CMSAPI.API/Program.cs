using System.Text;
using CMSAPI.API.Middleware;
using CMSAPI.Application.Configuration;
using CMSAPI.Application.Security;
using CMSAPI.Application;
using CMSAPI.Infrastructure;
using CMSAPI.Infrastructure.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PermissionPolicies.UsersManage, policy => policy.RequireClaim(CustomClaimTypes.Permission, Permissions.UsersManage));
    options.AddPolicy(PermissionPolicies.ClaimsRead, policy => policy.RequireClaim(CustomClaimTypes.Permission, Permissions.ClaimsRead));
    options.AddPolicy(PermissionPolicies.ClaimsCreate, policy => policy.RequireClaim(CustomClaimTypes.Permission, Permissions.ClaimsCreate));
    options.AddPolicy(PermissionPolicies.ClaimsAssign, policy => policy.RequireClaim(CustomClaimTypes.Permission, Permissions.ClaimsAssign));
    options.AddPolicy(PermissionPolicies.ClaimsInvestigate, policy => policy.RequireClaim(CustomClaimTypes.Permission, Permissions.ClaimsInvestigate));
    options.AddPolicy(PermissionPolicies.ClaimsAdjudicate, policy => policy.RequireClaim(CustomClaimTypes.Permission, Permissions.ClaimsAdjudicate));
    options.AddPolicy(PermissionPolicies.ClaimsPay, policy => policy.RequireClaim(CustomClaimTypes.Permission, Permissions.ClaimsPay));
    options.AddPolicy(PermissionPolicies.FraudReview, policy => policy.RequireClaim(CustomClaimTypes.Permission, Permissions.FraudReview));
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CMS API",
        Version = "v1",
        Description = "Claim Management System API"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter Bearer token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseCurrentUserContext();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<AuthDataSeeder>();
    await seeder.SeedAsync();
}

app.Run();
