# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Restore and build
dotnet restore
dotnet build

# Run the API
dotnet run --project src/Z0.WebApi

# Run all tests
dotnet test

# Run specific test categories
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"

# Run a single test by name
dotnet test --filter "FullyQualifiedName~TestMethodName"

# EF Core migrations (from repo root)
dotnet ef migrations add MigrationName --project src/Z0.Infrastructure --startup-project src/Z0.WebApi
dotnet ef database update --project src/Z0.Infrastructure --startup-project src/Z0.WebApi

# Docker (recommended for full stack)
docker-compose -f docker/docker-compose.yml up -d
docker-compose -f docker/docker-compose.yml down
```

## Architecture

This is a **Clean Architecture** .NET 9.0 e-commerce boilerplate with 4 layers:

```
Z0.Domain (innermost)
    └── Z0.Application
            └── Z0.Infrastructure
                    └── Z0.WebApi (outermost)
```

**Dependency rule**: Dependencies flow inward only. Domain has no external dependencies.

### Layer Responsibilities

- **Z0.Domain**: Entities (`Product`, `Category`), repository interfaces, domain exceptions. All entities inherit from `BaseEntity` (Id, CreatedAt, UpdatedAt, IsActive).

- **Z0.Application**: Services, DTOs, FluentValidation validators, AutoMapper profiles. Contains `IUnitOfWork` and service interfaces. Custom exceptions: `NotFoundException`, `ValidationException`, `ConflictException`.

- **Z0.Infrastructure**: EF Core `AppDbContext`, repository implementations, Unit of Work, AWS Cognito authentication, Serilog logging config.

- **Z0.WebApi**: Controllers in `Controllers/v1/`, middleware for exception handling, API versioning via URL (`/api/v1/`).

### Key Patterns

- **Repository + Unit of Work**: Generic `IRepository<T>` plus specific repositories, coordinated via `IUnitOfWork`
- **DTO mapping**: AutoMapper profiles in `Z0.Application/Mappings/`
- **Validation**: FluentValidation in `Z0.Application/Validators/`
- **Scope-based auth**: AWS Cognito JWT with policies like `products:read`, `products:write`, `categories:delete`

## Adding a New Entity

1. Create entity in `Z0.Domain/Entities/` inheriting `BaseEntity`
2. Add `DbSet<T>` to `Z0.Infrastructure/Data/AppDbContext.cs`
3. Create EF config in `Z0.Infrastructure/Data/Configurations/`
4. Add migration: `dotnet ef migrations add AddEntity --project src/Z0.Infrastructure --startup-project src/Z0.WebApi`
5. Create repository interface in `Z0.Domain/Interfaces/`
6. Implement repository in `Z0.Infrastructure/Data/Repositories/`
7. Add to `UnitOfWork.cs`
8. Create DTOs in `Z0.Application/DTOs/`
9. Add AutoMapper profile in `Z0.Application/Mappings/`
10. Create service interface and implementation in `Z0.Application/Services/`
11. Create controller in `Z0.WebApi/Controllers/v1/`

## Testing

- **Z0.Domain.Tests**: Entity business rules
- **Z0.Application.Tests**: Service logic with mocked dependencies
- **Z0.Infrastructure.Tests**: Repository tests with EF Core InMemory
- **Z0.WebApi.Tests**: Integration tests with `WebApplicationFactory`

Uses xUnit, Moq, FluentAssertions, and Testcontainers for PostgreSQL.

## Configuration

- Connection string: `appsettings.json` → `ConnectionStrings:DefaultConnection`
- AWS Cognito: `appsettings.json` → `AWS:Cognito` (Region, UserPoolId, AppClientId, Authority)
- Logging: Serilog configured via `appsettings.json`, outputs to console and `logs/` directory

## Tech Stack

.NET 9.0, PostgreSQL 16, EF Core 9, AWS Cognito, FluentValidation, AutoMapper, Serilog, xUnit
