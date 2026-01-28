# Spec-Driven Development: z0-aspnetcore-boilerplate

## Project Overview

| Attribute | Value |
| --- | --- |
| **Project Name** | z0-aspnetcore-boilerplate |
| **Domain** | Clean Architecture Starter |
| **Framework** | .NET 9.0 |
| **Architecture** | Clean Architecture (4 Layers) |
| **Database** | PostgreSQL |
| **Authentication** | AWS Cognito (User Pools) |
| **Authorization** | Scope-based policies |

---

## 1. Solution Structure

```
z0-aspnetcore-boilerplate/
├── src/
│   ├── Z0.Domain/                    # Layer 1: Enterprise Business Rules
│   ├── Z0.Application/               # Layer 2: Application Business Rules
│   ├── Z0.Infrastructure/            # Layer 3: External Concerns
│   └── Z0.WebApi/                    # Layer 4: Presentation
├── tests/
│   ├── Z0.Domain.Tests/
│   ├── Z0.Application.Tests/
│   ├── Z0.Infrastructure.Tests/
│   └── Z0.WebApi.Tests/
├── .github/
│   └── workflows/
│       └── ci-cd.yml
├── docker/
│   ├── Dockerfile
│   └── docker-compose.yml
└── z0-aspnetcore-boilerplate.sln
```

---

## 2. Layer Specifications

### 2.1 Domain Layer (`Z0.Domain`)

**Purpose:** Core business entities and domain logic. Zero external dependencies.

**Structure:**

```
Z0.Domain/
├── Entities/
│   ├── Item.cs
│   └── BaseEntity.cs
├── ValueObjects/
│   └── Money.cs
├── Exceptions/
│   └── DomainException.cs
└── Interfaces/
    └── IRepository.cs
```

**Entity Specifications:**

```csharp
// BaseEntity.cs
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsActive { get; protected set; } = true;
}

// Item.cs
public class Item : BaseEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
}
```

**Dependencies:** None

---

### 2.2 Application Layer (`Z0.Application`)

**Purpose:** Use cases, DTOs, interfaces, and application logic.

**Structure:**

```
Z0.Application/
├── Interfaces/
│   ├── IItemRepository.cs
│   └── IUnitOfWork.cs
├── DTOs/
│   └── Items/
│       ├── ItemDto.cs
│       ├── CreateItemRequest.cs
│       └── UpdateItemRequest.cs
├── Services/
│   ├── IItemService.cs
│   └── ItemService.cs
├── Validators/
│   ├── CreateItemValidator.cs
│   └── UpdateItemValidator.cs
├── Mappings/
│   └── MappingProfile.cs
└── Exceptions/
    ├── NotFoundException.cs
    └── ValidationException.cs
```

**Dependencies:**

- `Z0.Domain`
- `AutoMapper`
- `FluentValidation`

---

### 2.3 Infrastructure Layer (`Z0.Infrastructure`)

**Purpose:** Data access, external services, AWS Cognito integration.

**Structure:**

```
Z0.Infrastructure/
├── Data/
│   ├── AppDbContext.cs
│   ├── Configurations/
│   │   └── ItemConfiguration.cs
│   ├── Repositories/
│   │   ├── BaseRepository.cs
│   │   └── ItemRepository.cs
│   └── Migrations/
├── Identity/
│   ├── CognitoAuthenticationExtensions.cs
│   └── AuthorizationPolicies.cs
├── Logging/
│   └── SerilogConfiguration.cs
└── DependencyInjection.cs
```

**Dependencies:**

- `Z0.Domain`
- `Z0.Application`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Serilog`
- `Microsoft.AspNetCore.Authentication.JwtBearer`

---

### 2.4 Presentation Layer (`Z0.WebApi`)

**Purpose:** API controllers, middleware, configuration.

**Structure:**

```
Z0.WebApi/
├── Controllers/
│   └── v1/
│       ├── ItemsController.cs
│       └── HealthController.cs
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── Extensions/
│   ├── ServiceCollectionExtensions.cs
│   └── ApplicationBuilderExtensions.cs
├── appsettings.json
├── appsettings.Development.json
├── appsettings.Production.json
└── Program.cs
```

**Dependencies:**

- `Z0.Application`
- `Z0.Infrastructure`
- `Swashbuckle.AspNetCore`

---

## 3. AWS Cognito Integration

### 3.1 Configuration Schema

```json
{
  "AWS": {
    "Cognito": {
      "Region": "us-east-1",
      "UserPoolId": "us-east-1_XXXXXXXXX",
      "AppClientId": "xxxxxxxxxxxxxxxxxxxxxxxxxx",
      "Authority": "https://cognito-idp.{region}.amazonaws.com/{userPoolId}"
    }
  }
}
```

### 3.2 Authentication Setup

```csharp
// CognitoAuthenticationExtensions.cs
public static IServiceCollection AddCognitoAuthentication(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var cognitoSettings = configuration.GetSection("AWS:Cognito");

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = cognitoSettings["Authority"];
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        });

    return services;
}
```

### 3.3 Authorization Policies (Scope-Based)

```csharp
// AuthorizationPolicies.cs
public static class AuthorizationPolicies
{
    public const string ReadItems = "items:read";
    public const string WriteItems = "items:write";
    public const string DeleteItems = "items:delete";

    public static void AddPolicies(AuthorizationOptions options)
    {
        options.AddPolicy(ReadItems, policy =>
            policy.RequireClaim("scope", ReadItems));
        options.AddPolicy(WriteItems, policy =>
            policy.RequireClaim("scope", WriteItems));
        options.AddPolicy(DeleteItems, policy =>
            policy.RequireClaim("scope", DeleteItems));
    }
}
```

---

## 4. API Endpoints Specification

### 4.1 Items API (`/api/v1/items`)

| Method | Endpoint | Policy | Description |
| --- | --- | --- | --- |
| GET | `/api/v1/items` | `items:read` | List all items (paginated) |
| GET | `/api/v1/items/{id}` | `items:read` | Get item by ID |
| POST | `/api/v1/items` | `items:write` | Create new item |
| PUT | `/api/v1/items/{id}` | `items:write` | Update item |
| DELETE | `/api/v1/items/{id}` | `items:delete` | Delete item |

### 4.2 Health API (`/api/v1/health`)

| Method | Endpoint | Policy | Description |
| --- | --- | --- | --- |
| GET | `/api/v1/health` | Anonymous | Basic health check |
| GET | `/api/v1/health/ready` | Anonymous | Readiness probe (DB check) |
| GET | `/api/v1/health/live` | Anonymous | Liveness probe |

---

## 5. Database Schema

### 5.1 PostgreSQL Tables

```sql
-- Items table
CREATE TABLE "Items" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP
);

-- Indexes
CREATE INDEX "IX_Items_Name" ON "Items"("Name");
```

---

## 6. Logging Configuration

### 6.1 Serilog Setup

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

---

## 7. Testing Strategy

### 7.1 Unit Tests

- **Domain Tests:** Entity validation, business rules
- **Application Tests:** Service logic, validators (mock repositories)

### 7.2 Integration Tests

- **Repository Tests:** Use PostgreSQL test container or EF Core InMemory
- **API Tests:** Use `WebApplicationFactory<Program>`

### 7.3 Test Libraries

```xml
<PackageReference Include="xunit" Version="2.9.*" />
<PackageReference Include="Moq" Version="4.20.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.*" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.*" />
```

---

## 8. CI/CD Pipeline (GitHub Actions)

```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  DOTNET_VERSION: '9.0.x'

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Run Unit Tests
        run: dotnet test --no-build --configuration Release --filter "Category=Unit"

      - name: Run Integration Tests
        run: dotnet test --no-build --configuration Release --filter "Category=Integration"
```

---

## 9. Deployment Configuration

### 9.1 Dockerfile

```docker
# docker/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Z0.WebApi/Z0.WebApi.csproj", "Z0.WebApi/"]
COPY ["src/Z0.Application/Z0.Application.csproj", "Z0.Application/"]
COPY ["src/Z0.Infrastructure/Z0.Infrastructure.csproj", "Z0.Infrastructure/"]
COPY ["src/Z0.Domain/Z0.Domain.csproj", "Z0.Domain/"]
RUN dotnet restore "Z0.WebApi/Z0.WebApi.csproj"
COPY src/ .
RUN dotnet build "Z0.WebApi/Z0.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Z0.WebApi/Z0.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Z0.WebApi.dll"]
```

### 9.2 Docker Compose (Development)

```yaml
# docker/docker-compose.yml
services:
  api:
    build:
      context: ..
      dockerfile: docker/Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=z0db;Username=postgres;Password=postgres
    depends_on:
      - postgres

  postgres:
    image: postgres:16-alpine
    ports:
      - "5432:5432"
    environment:
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
      - POSTGRES_DB=z0db
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

---

## 10. Verification Plan

1. **Build Verification:** `dotnet build` succeeds without errors
2. **Unit Tests:** `dotnet test --filter "Category=Unit"` passes
3. **Integration Tests:** `dotnet test --filter "Category=Integration"` passes
4. **Local Run:** `docker-compose up` starts API and database
5. **Health Check:** `GET /api/v1/health` returns 200 OK
6. **Auth Test:** Endpoints reject requests without valid Cognito JWT
7. **CRUD Operations:** All item endpoints work correctly
