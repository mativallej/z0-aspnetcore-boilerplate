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
namespace Z0.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsActive { get; protected set; } = true;

    public void SetUpdated() => UpdatedAt = DateTime.UtcNow;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

// Item.cs
namespace Z0.Domain.Entities;

public class Item : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private Item() { } // EF Core

    public Item(string name, string? description = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
    }

    public void Update(string name, string? description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        SetUpdated();
    }
}
```

**Value Objects:**

```csharp
// Money.cs
namespace Z0.Domain.ValueObjects;

public record Money(decimal Amount, string Currency = "USD")
{
    public static Money Zero => new(0);
    public static Money operator +(Money a, Money b) =>
        a.Currency == b.Currency
            ? new Money(a.Amount + b.Amount, a.Currency)
            : throw new InvalidOperationException("Currency mismatch");
}
```

**Domain Exceptions:**

```csharp
// DomainException.cs
namespace Z0.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

**Repository Interface:**

```csharp
// IRepository.cs
namespace Z0.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
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

**Interfaces:**

```csharp
// IItemRepository.cs
namespace Z0.Application.Interfaces;

public interface IItemRepository : IRepository<Item>
{
    Task<IEnumerable<Item>> GetActiveItemsAsync(CancellationToken cancellationToken = default);
    Task<Item?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}

// IUnitOfWork.cs
namespace Z0.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IItemRepository Items { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// IItemService.cs
namespace Z0.Application.Services;

public interface IItemService
{
    Task<IEnumerable<ItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ItemDto> CreateAsync(CreateItemRequest request, CancellationToken cancellationToken = default);
    Task<ItemDto> UpdateAsync(Guid id, UpdateItemRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

**DTOs:**

```csharp
// ItemDto.cs
namespace Z0.Application.DTOs.Items;

public record ItemDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// CreateItemRequest.cs
namespace Z0.Application.DTOs.Items;

public record CreateItemRequest(string Name, string? Description);

// UpdateItemRequest.cs
namespace Z0.Application.DTOs.Items;

public record UpdateItemRequest(string Name, string? Description);
```

**Service Implementation:**

```csharp
// ItemService.cs
namespace Z0.Application.Services;

public class ItemService : IItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ItemService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Items.GetActiveItemsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ItemDto>>(items);
    }

    public async Task<ItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.Items.GetByIdAsync(id, cancellationToken);
        return item is null ? null : _mapper.Map<ItemDto>(item);
    }

    public async Task<ItemDto> CreateAsync(CreateItemRequest request, CancellationToken cancellationToken = default)
    {
        var item = new Item(request.Name, request.Description);
        await _unitOfWork.Items.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ItemDto>(item);
    }

    public async Task<ItemDto> UpdateAsync(Guid id, UpdateItemRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.Items.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Item), id);
        item.Update(request.Name, request.Description);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ItemDto>(item);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.Items.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Item), id);
        item.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
```

**Validators:**

```csharp
// CreateItemValidator.cs
namespace Z0.Application.Validators;

public class CreateItemValidator : AbstractValidator<CreateItemRequest>
{
    public CreateItemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
    }
}

// UpdateItemValidator.cs
namespace Z0.Application.Validators;

public class UpdateItemValidator : AbstractValidator<UpdateItemRequest>
{
    public UpdateItemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
    }
}
```

**AutoMapper Profile:**

```csharp
// MappingProfile.cs
namespace Z0.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Item, ItemDto>();
    }
}
```

**Application Exceptions:**

```csharp
// NotFoundException.cs
namespace Z0.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.") { }
}

// ValidationException.cs
namespace Z0.Application.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }
}
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

**Data Access:**

```csharp
// AppDbContext.cs
namespace Z0.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

// ItemConfiguration.cs
namespace Z0.Infrastructure.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.Name);
    }
}
```

**Repositories:**

```csharp
// BaseRepository.cs
namespace Z0.Infrastructure.Data.Repositories;

public class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync(new object[] { id }, cancellationToken);

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.ToListAsync(cancellationToken);

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual void Update(T entity) => _dbSet.Update(entity);

    public virtual void Delete(T entity) => _dbSet.Remove(entity);
}

// ItemRepository.cs
namespace Z0.Infrastructure.Data.Repositories;

public class ItemRepository : BaseRepository<Item>, IItemRepository
{
    public ItemRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Item>> GetActiveItemsAsync(CancellationToken cancellationToken = default)
        => await _dbSet.Where(x => x.IsActive).ToListAsync(cancellationToken);

    public async Task<Item?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
}

// UnitOfWork.cs
namespace Z0.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IItemRepository? _items;

    public UnitOfWork(AppDbContext context) => _context = context;

    public IItemRepository Items => _items ??= new ItemRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
```

**Serilog Configuration:**

```csharp
// SerilogConfiguration.cs
namespace Z0.Infrastructure.Logging;

public static class SerilogConfiguration
{
    public static IHostBuilder UseSerilogConfiguration(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        });
    }
}
```

**Dependency Injection:**

```csharp
// DependencyInjection.cs
namespace Z0.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Authentication
        services.AddCognitoAuthentication(configuration);

        return services;
    }
}
```

**Dependencies:**

- `Z0.Domain`
- `Z0.Application`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Serilog`
- `Serilog.AspNetCore`
- `Serilog.Sinks.Console`
- `Serilog.Sinks.File`
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

**Controllers:**

```csharp
// ItemsController.cs
namespace Z0.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;

    public ItemsController(IItemService itemService) => _itemService = itemService;

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ReadItems)]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _itemService.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ReadItems)]
    public async Task<ActionResult<ItemDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _itemService.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.WriteItems)]
    public async Task<ActionResult<ItemDto>> Create(CreateItemRequest request, CancellationToken cancellationToken)
    {
        var item = await _itemService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.WriteItems)]
    public async Task<ActionResult<ItemDto>> Update(Guid id, UpdateItemRequest request, CancellationToken cancellationToken)
    {
        var item = await _itemService.UpdateAsync(id, request, cancellationToken);
        return Ok(item);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.DeleteItems)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _itemService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

// HealthController.cs
namespace Z0.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;

    public HealthController(AppDbContext context) => _context = context;

    [HttpGet]
    public IActionResult Get() => Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });

    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            return Ok(new { status = "Ready", database = "Connected" });
        }
        catch
        {
            return StatusCode(503, new { status = "Not Ready", database = "Disconnected" });
        }
    }

    [HttpGet("live")]
    public IActionResult Live() => Ok(new { status = "Alive" });
}
```

**Middleware:**

```csharp
// ExceptionHandlingMiddleware.cs
namespace Z0.WebApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var (statusCode, response) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound,
                new { error = exception.Message }),
            ValidationException validationEx => (StatusCodes.Status400BadRequest,
                new { error = validationEx.Message, errors = validationEx.Errors } as object),
            DomainException => (StatusCodes.Status400BadRequest,
                new { error = exception.Message }),
            _ => (StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred" })
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

**Extensions:**

```csharp
// ServiceCollectionExtensions.cs
namespace Z0.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateItemValidator>();

        // Services
        services.AddScoped<IItemService, ItemService>();

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Z0 API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });
        return services;
    }
}

// ApplicationBuilderExtensions.cs
namespace Z0.WebApi.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
```

**Program.cs:**

```csharp
// Program.cs
using Z0.Infrastructure;
using Z0.Infrastructure.Logging;
using Z0.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilogConfiguration();

// Services
builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

// Authorization policies
builder.Services.AddAuthorization(AuthorizationPolicies.AddPolicies);

var app = builder.Build();

// Middleware
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Required for WebApplicationFactory
public partial class Program { }
```

**appsettings.json:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=z0db;Username=postgres;Password=postgres"
  },
  "AWS": {
    "Cognito": {
      "Region": "us-east-1",
      "UserPoolId": "us-east-1_XXXXXXXXX",
      "AppClientId": "xxxxxxxxxxxxxxxxxxxxxxxxxx",
      "Authority": "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_XXXXXXXXX"
    }
  },
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
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
  },
  "AllowedHosts": "*"
}
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

## 7. Project Files (csproj)

### 7.1 Z0.Domain.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

### 7.2 Z0.Application.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="AutoMapper" Version="13.*" />
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.*" />
    <PackageReference Include="FluentValidation" Version="11.*" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Z0.Domain\Z0.Domain.csproj" />
  </ItemGroup>
</Project>
```

### 7.3 Z0.Infrastructure.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.*" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.*" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.*" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.*" />
    <PackageReference Include="Serilog.Sinks.Console" Version="6.*" />
    <PackageReference Include="Serilog.Sinks.File" Version="6.*" />
    <PackageReference Include="Serilog.Enrichers.Environment" Version="3.*" />
    <PackageReference Include="Serilog.Enrichers.Thread" Version="4.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Z0.Domain\Z0.Domain.csproj" />
    <ProjectReference Include="..\Z0.Application\Z0.Application.csproj" />
  </ItemGroup>
</Project>
```

### 7.4 Z0.WebApi.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Z0.Application\Z0.Application.csproj" />
    <ProjectReference Include="..\Z0.Infrastructure\Z0.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

### 7.5 Test Projects

```xml
<!-- Common test project structure -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.9.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.*" />
    <PackageReference Include="Moq" Version="4.20.*" />
    <PackageReference Include="FluentAssertions" Version="6.*" />
    <PackageReference Include="coverlet.collector" Version="6.*" />
  </ItemGroup>
</Project>

<!-- Additional for Z0.Infrastructure.Tests -->
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.*" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.*" />

<!-- Additional for Z0.WebApi.Tests -->
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.*" />
```

---

## 8. Testing Strategy

### 8.1 Unit Tests

- **Domain Tests:** Entity validation, business rules
- **Application Tests:** Service logic, validators (mock repositories)

### 8.2 Integration Tests

- **Repository Tests:** Use PostgreSQL test container or EF Core InMemory
- **API Tests:** Use `WebApplicationFactory<Program>`

### 8.3 Test Libraries

```xml
<PackageReference Include="xunit" Version="2.9.*" />
<PackageReference Include="Moq" Version="4.20.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.*" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.*" />
```

---

## 9. CI/CD Pipeline (GitHub Actions)

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

## 10. Deployment Configuration

### 10.1 Dockerfile

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

### 10.2 Docker Compose (Development)

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

## 11. Verification Plan

1. **Build Verification:** `dotnet build` succeeds without errors
2. **Unit Tests:** `dotnet test --filter "Category=Unit"` passes
3. **Integration Tests:** `dotnet test --filter "Category=Integration"` passes
4. **Local Run:** `docker-compose up` starts API and database
5. **Health Check:** `GET /api/v1/health` returns 200 OK
6. **Auth Test:** Endpoints reject requests without valid Cognito JWT
7. **CRUD Operations:** All item endpoints work correctly

---

## 12. Implementation Checklist

Use this checklist to track implementation progress:

### Phase 1: Solution Setup
- [ ] Create solution file `z0-aspnetcore-boilerplate.sln`
- [ ] Create `src/Z0.Domain/Z0.Domain.csproj` (no dependencies)
- [ ] Create `src/Z0.Application/Z0.Application.csproj` (refs: Domain)
- [ ] Create `src/Z0.Infrastructure/Z0.Infrastructure.csproj` (refs: Domain, Application)
- [ ] Create `src/Z0.WebApi/Z0.WebApi.csproj` (refs: Application, Infrastructure)
- [ ] Create test projects in `tests/`
- [ ] Add all NuGet packages per layer dependencies

### Phase 2: Domain Layer
- [ ] Create `Entities/BaseEntity.cs`
- [ ] Create `Entities/Item.cs`
- [ ] Create `ValueObjects/Money.cs`
- [ ] Create `Exceptions/DomainException.cs`
- [ ] Create `Interfaces/IRepository.cs`

### Phase 3: Application Layer
- [ ] Create `Interfaces/IItemRepository.cs`
- [ ] Create `Interfaces/IUnitOfWork.cs`
- [ ] Create `DTOs/Items/ItemDto.cs`
- [ ] Create `DTOs/Items/CreateItemRequest.cs`
- [ ] Create `DTOs/Items/UpdateItemRequest.cs`
- [ ] Create `Services/IItemService.cs`
- [ ] Create `Services/ItemService.cs`
- [ ] Create `Validators/CreateItemValidator.cs`
- [ ] Create `Validators/UpdateItemValidator.cs`
- [ ] Create `Mappings/MappingProfile.cs`
- [ ] Create `Exceptions/NotFoundException.cs`
- [ ] Create `Exceptions/ValidationException.cs`

### Phase 4: Infrastructure Layer
- [ ] Create `Data/AppDbContext.cs`
- [ ] Create `Data/Configurations/ItemConfiguration.cs`
- [ ] Create `Data/Repositories/BaseRepository.cs`
- [ ] Create `Data/Repositories/ItemRepository.cs`
- [ ] Create `Data/UnitOfWork.cs`
- [ ] Create `Identity/CognitoAuthenticationExtensions.cs`
- [ ] Create `Identity/AuthorizationPolicies.cs`
- [ ] Create `Logging/SerilogConfiguration.cs`
- [ ] Create `DependencyInjection.cs`
- [ ] Generate initial EF Core migration

### Phase 5: WebApi Layer
- [ ] Create `Controllers/v1/ItemsController.cs`
- [ ] Create `Controllers/v1/HealthController.cs`
- [ ] Create `Middleware/ExceptionHandlingMiddleware.cs`
- [ ] Create `Extensions/ServiceCollectionExtensions.cs`
- [ ] Create `Extensions/ApplicationBuilderExtensions.cs`
- [ ] Create `Program.cs`
- [ ] Create `appsettings.json`
- [ ] Create `appsettings.Development.json`
- [ ] Create `appsettings.Production.json`

### Phase 6: Testing
- [ ] Add domain entity unit tests
- [ ] Add service unit tests with mocked repositories
- [ ] Add validator tests
- [ ] Add repository integration tests
- [ ] Add API integration tests with WebApplicationFactory

### Phase 7: DevOps
- [ ] Create `docker/Dockerfile`
- [ ] Create `docker/docker-compose.yml`
- [ ] Create `.github/workflows/ci-cd.yml`
- [ ] Create `.gitignore`
- [ ] Create `README.md`

### Phase 8: Verification
- [ ] Run `dotnet build` - no errors
- [ ] Run `dotnet test` - all tests pass
- [ ] Run `docker-compose up` - services start
- [ ] Test health endpoints
- [ ] Test CRUD operations via Swagger
