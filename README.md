# Z0 ASP.NET Core Boilerplate — Clean Architecture Starter

[![X (formerly Twitter) Follow](https://img.shields.io/twitter/follow/mativallej_?style=social)](https://x.com/mativallej_)
[![GitHub top language](https://img.shields.io/github/languages/top/mativallej/z0-aspnetcore-boilerplate?color=512BD4)](https://github.com/mativallej/z0-aspnetcore-boilerplate/search?l=c%23)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=fff)](https://dotnet.microsoft.com/)
![License](https://img.shields.io/github/license/mativallej/z0-aspnetcore-boilerplate?label=license&logo=github&color=f80&logoColor=fff)
![Forks](https://img.shields.io/github/forks/mativallej/z0-aspnetcore-boilerplate.svg)
![Stars](https://img.shields.io/github/stars/mativallej/z0-aspnetcore-boilerplate.svg)
![Watchers](https://img.shields.io/github/watchers/mativallej/z0-aspnetcore-boilerplate.svg)

> A production-ready Clean Architecture boilerplate for building APIs with .NET 9.0, PostgreSQL, AWS Cognito authentication, and comprehensive testing. One-command Docker setup.

## Introduction

**Z0 ASP.NET Core Boilerplate** is a robust, scalable, and maintainable foundation for building APIs following **Clean Architecture** principles.

**What you get out of the box:**
- **4-Layer Architecture** – Domain, Application, Infrastructure, and Presentation layers with clear separation of concerns
- **AWS Cognito Integration** – JWT authentication with scope-based authorization policies
- **PostgreSQL + EF Core** – Production-ready database setup with migrations support
- **Comprehensive Testing** – Unit and integration tests with xUnit, Moq, and Testcontainers

Just clone, configure your AWS Cognito settings, and you're ready to build your next API with:

1. **Item management** with full CRUD operations as a starting example
2. **Scope-based Authorization** for fine-grained access control
3. **Swagger/OpenAPI** documentation with JWT support
4. **Health Checks** for Kubernetes-ready deployments

Inspired by *Clean Architecture* by Robert C. Martin and *Domain-Driven Design* principles, this project promotes maintainable, testable, and scalable software design.

## Key Features

- **Docker Support** – One-command setup with Docker Compose. Works on any machine.
- **Clean Architecture** – 4-layer structure with strict dependency rules and SOLID principles.
- **AWS Cognito Auth** – JWT Bearer authentication with scope-based authorization policies.
- **PostgreSQL + EF Core** – Code-first migrations, entity configurations, and repository pattern.
- **AutoMapper + FluentValidation** – Clean DTO mapping and robust request validation.
- **Serilog Logging** – Structured logging with console and file sinks.
- **Comprehensive Testing** – Unit, integration, and API tests with Testcontainers.
- **Swagger/OpenAPI** – Interactive API documentation with JWT authentication support.
- **CI/CD Ready** – GitHub Actions workflow for build and test.

## Quick Start (Docker - Recommended)

The easiest way to get started is with Docker Compose. This will set up the API and PostgreSQL database in one command.

### Prerequisites
- [Docker](https://docs.docker.com/get-docker/) and [Docker Compose](https://docs.docker.com/compose/install/) installed.
- An [AWS Cognito User Pool](https://aws.amazon.com/cognito/) configured (optional for development).

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/mativallej/z0-aspnetcore-boilerplate.git
   cd z0-aspnetcore-boilerplate
   ```

2. **Start all services**
   ```bash
   docker-compose -f docker/docker-compose.yml up -d
   ```

3. **Access the applications**
   - **API**: http://localhost:5000
   - **Swagger UI**: http://localhost:5000/swagger
   - **Health Check**: http://localhost:5000/api/v1/health
   - **pgAdmin** (optional): http://localhost:5050 (enable with `--profile tools`)

4. **Configure AWS Cognito (Production)**

   Update `appsettings.json` with your Cognito settings:
   ```json
   {
     "AWS": {
       "Cognito": {
         "Region": "us-east-1",
         "UserPoolId": "us-east-1_XXXXXXXXX",
         "AppClientId": "your-app-client-id",
         "Authority": "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_XXXXXXXXX"
       }
     }
   }
   ```

5. **Test the API**
   ```bash
   # Health check (no auth required)
   curl http://localhost:5000/api/v1/health

   # Get items (requires auth in production)
   curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
        http://localhost:5000/api/v1/items
   ```

### Managing Your Docker Application

```bash
# Stop all services
docker-compose -f docker/docker-compose.yml down

# View logs
docker-compose -f docker/docker-compose.yml logs -f

# View logs for specific service
docker-compose -f docker/docker-compose.yml logs -f api
docker-compose -f docker/docker-compose.yml logs -f postgres

# Restart services
docker-compose -f docker/docker-compose.yml restart

# Rebuild and restart (after code changes)
docker-compose -f docker/docker-compose.yml up --build -d

# Start with pgAdmin for database management
docker-compose -f docker/docker-compose.yml --profile tools up -d

# Stop and remove everything (including volumes - deletes database)
docker-compose -f docker/docker-compose.yml down -v
```

---

## Alternative Setup (Local Development)

If you prefer to run the application without Docker:

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- An [AWS Cognito User Pool](https://aws.amazon.com/cognito/) (optional for development)

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/mativallej/z0-aspnetcore-boilerplate.git
   cd z0-aspnetcore-boilerplate
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update connection string**

   Edit `src/Z0.WebApi/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=z0db;Username=postgres;Password=postgres"
     }
   }
   ```

4. **Run database migrations**
   ```bash
   dotnet ef database update --project src/Z0.Infrastructure --startup-project src/Z0.WebApi
   ```

5. **Run the application**
   ```bash
   dotnet run --project src/Z0.WebApi
   ```

6. **Access the API**
   - **Swagger UI**: https://localhost:5001/swagger
   - **Health Check**: https://localhost:5001/api/v1/health

---

## Project Structure

```
z0-aspnetcore-boilerplate/
├── src/
│   ├── Z0.Domain/                    # Layer 1: Enterprise Business Rules
│   │   ├── Entities/                 # Item, BaseEntity
│   │   ├── ValueObjects/             # Money, etc.
│   │   ├── Exceptions/               # DomainException
│   │   └── Interfaces/               # IRepository<T>
│   │
│   ├── Z0.Application/               # Layer 2: Application Business Rules
│   │   ├── DTOs/                     # Request/Response DTOs
│   │   ├── Services/                 # IItemService
│   │   ├── Validators/               # FluentValidation validators
│   │   ├── Mappings/                 # AutoMapper profiles
│   │   ├── Interfaces/               # Repository interfaces
│   │   └── Exceptions/               # NotFoundException, ValidationException
│   │
│   ├── Z0.Infrastructure/            # Layer 3: External Concerns
│   │   ├── Data/                     # AppDbContext, Repositories
│   │   ├── Identity/                 # Cognito auth, Authorization policies
│   │   └── Logging/                  # Serilog configuration
│   │
│   └── Z0.WebApi/                    # Layer 4: Presentation
│       ├── Controllers/v1/           # Versioned API controllers
│       ├── Middleware/               # Exception handling
│       ├── Extensions/               # Service collection extensions
│       └── Program.cs                # Application entry point
│
├── tests/
│   ├── Z0.Domain.Tests/              # Entity & value object tests
│   ├── Z0.Application.Tests/         # Service & validator tests
│   ├── Z0.Infrastructure.Tests/      # Repository integration tests
│   └── Z0.WebApi.Tests/              # API integration tests
│
├── docker/
│   ├── Dockerfile                    # Multi-stage build
│   └── docker-compose.yml            # Full stack setup
│
└── .github/workflows/
    └── ci-cd.yml                     # GitHub Actions pipeline
```

---

## API Endpoints

### Items API (`/api/v1/items`)

| Method | Endpoint | Policy | Description |
|--------|----------|--------|-------------|
| `GET` | `/api/v1/items` | `items:read` | List all items (paginated) |
| `GET` | `/api/v1/items/{id}` | `items:read` | Get item by ID |
| `POST` | `/api/v1/items` | `items:write` | Create new item |
| `PUT` | `/api/v1/items/{id}` | `items:write` | Update item |
| `DELETE` | `/api/v1/items/{id}` | `items:delete` | Delete item |

### Health API (`/api/v1/health`)

| Method | Endpoint | Policy | Description |
|--------|----------|--------|-------------|
| `GET` | `/api/v1/health` | Anonymous | Basic health check |
| `GET` | `/api/v1/health/ready` | Anonymous | Readiness probe (DB check) |
| `GET` | `/api/v1/health/live` | Anonymous | Liveness probe |

---

## Authorization Scopes

The API uses scope-based authorization with AWS Cognito. Configure these scopes in your Cognito User Pool:

| Scope | Description |
|-------|-------------|
| `items:read` | Read item information |
| `items:write` | Create and update items |
| `items:delete` | Delete items |

---

## Testing

### Run All Tests

```bash
dotnet test
```

### Run by Category

```bash
# Unit tests only
dotnet test --filter "Category=Unit"

# Integration tests only
dotnet test --filter "Category=Integration"
```

### Test Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure

| Project | Type | Description |
|---------|------|-------------|
| `Z0.Domain.Tests` | Unit | Entity validation, business rules |
| `Z0.Application.Tests` | Unit | Service logic, validators |
| `Z0.Infrastructure.Tests` | Integration | Repository tests with in-memory DB |
| `Z0.WebApi.Tests` | Integration | API endpoint tests |

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| **Framework** | .NET 9.0 |
| **Database** | PostgreSQL 16 + EF Core 9 |
| **Authentication** | AWS Cognito + JWT Bearer |
| **Validation** | FluentValidation 11 |
| **Mapping** | AutoMapper 12 |
| **Logging** | Serilog |
| **API Docs** | Swagger/OpenAPI |
| **Testing** | xUnit, Moq, FluentAssertions, Testcontainers |
| **CI/CD** | GitHub Actions |
| **Container** | Docker + Docker Compose |

---

## Deployment

### GitHub Actions CI/CD

The included workflow (`.github/workflows/ci-cd.yml`) provides:

1. **Build & Test** – Restores, builds, and runs all tests on every push and PR

### Docker Deployment

Build and run with Docker:

```bash
# Build the image
docker build -f docker/Dockerfile -t z0-api .

# Run with environment variables
docker run -d -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="your-connection-string" \
  z0-api
```

Or use Docker Compose for the full stack:

```bash
docker-compose -f docker/docker-compose.yml up -d
```

---

## Database Schema

### Items Table

```sql
CREATE TABLE "Items" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP
);
```

---

## Contributing

Contributions are welcome! If you want to improve the architecture, add new features, or fix bugs:

1. **Fork** the repository.
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`).
3. **Make** your changes following the existing code style.
4. **Test** your changes:
   ```bash
   dotnet test
   ```
5. **Commit** your changes (`git commit -m 'Add amazing feature'`).
6. **Push** to the branch (`git push origin feature/amazing-feature`).
7. **Open** a Pull Request.

**Guidelines:**
- Follow Clean Architecture principles – keep dependencies flowing inward
- Write unit tests for new business logic
- Write integration tests for new endpoints
- Update the README if you add new features
- Use conventional commit messages

---

## Contact

If you have questions, suggestions, or want to collaborate:

- **Name:** Matías Vallejos
- [matiasvallejos.com](https://matiasvallejos.com)
- [@mativallej_](https://x.com/mativallej_)

---

## License

This project is open source and available under the [MIT License](LICENSE).
