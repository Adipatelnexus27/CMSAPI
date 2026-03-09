# CMSAPI

Enterprise Claim Management backend API built with .NET Clean Architecture.

## Architecture
- API: controllers, authentication, request/response mapping
- Application: use cases, services, DTOs, validation
- Domain: entities, business rules, value objects
- Infrastructure: repository implementations, integrations, data access

## Core Rules
- Backend is the single source of truth.
- Repository + Service pattern.
- DTO-based API contracts.
- JWT authentication and role-based authorization.
