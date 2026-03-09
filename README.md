# CMSAPI

ASP.NET Core Web API for Claim Management System using Clean Architecture.

## Tech Stack
- .NET 8
- ASP.NET Core Web API
- EF Core + SQL Server
- JWT Authentication + Role-based Authorization
- FluentValidation
- AutoMapper
- Serilog

## Layers
- `CMSAPI.Domain`
  - Entities
  - Enums
  - Interfaces
- `CMSAPI.Application`
  - DTOs
  - Business Rules
  - Services
  - Validators
- `CMSAPI.Infrastructure`
  - EF Core DbContext
  - Repository Pattern
  - File Storage Service
  - Email Service
  - Notification Service
- `CMSAPI.API`
  - Controllers
  - Middleware
  - Authentication
  - Swagger

## Project Structure
```text
CMSAPI/
  CMSAPI.sln
  src/
    CMSAPI.Domain/
      Entities/
      Enums/
      Interfaces/
    CMSAPI.Application/
      DTOs/
        Auth/
        Claims/
      Interfaces/
        Services/
      BusinessRules/
      Services/
      Validators/
      Mapping/
      DependencyInjection.cs
    CMSAPI.Infrastructure/
      Persistence/
        Configurations/
        Repositories/
      Services/
      Options/
      DependencyInjection.cs
    CMSAPI.API/
      Controllers/
      Middleware/
      Options/
      Program.cs
      appsettings.json
      appsettings.Development.json
```

## Run
```bash
dotnet restore CMSAPI.sln
dotnet build CMSAPI.sln
dotnet run --project src/CMSAPI.API/CMSAPI.API.csproj
```

## Authentication
- `POST /api/auth/login`
  - Sample users:
    - `admin / Admin@123`
    - `adjuster / Adjuster@123`
    - `supervisor / Supervisor@123`

Use returned `accessToken` in `Authorization: Bearer <token>` header.

