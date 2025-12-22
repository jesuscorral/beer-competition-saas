# Solution Structure Documentation

**Project**: Beer Competition SaaS Platform  
**Architecture**: Modular Monolith with Vertical Slices and DDD  
**Last Updated**: 2025-12-22

---

## Overview

This document describes the structure of the `BeerCompetition.Monolith` solution, which implements a **modular monolith architecture** with **vertical slices** and **Domain-Driven Design (DDD)** tactical patterns.

The solution is organized as a single deployable application with clear internal module boundaries following bounded contexts from DDD. Each module can potentially be extracted into a microservice in the future.

---

## Solution Structure

```
backend/
├── BeerCompetition.Monolith.sln          # Solution file
├── .editorconfig                          # Code style configuration
│
├── Modules/                               # Bounded Contexts (DDD)
│   ├── Competition/                       # Competition Module
│   │   ├── BeerCompetition.Competition.Domain/
│   │   │   ├── Entities/                  # Domain entities (Competition, Entry, etc.)
│   │   │   ├── Events/                    # Domain events
│   │   │   └── Repositories/              # Repository interfaces
│   │   │
│   │   ├── BeerCompetition.Competition.Application/
│   │   │   └── Features/                  # Vertical slices
│   │   │       └── CreateCompetition/     # Example vertical slice
│   │   │           ├── CreateCompetitionCommand.cs
│   │   │           ├── CreateCompetitionHandler.cs
│   │   │           └── CreateCompetitionValidator.cs
│   │   │
│   │   ├── BeerCompetition.Competition.Infrastructure/
│   │   │   ├── Persistence/               # EF Core DbContext, configurations
│   │   │   ├── Repositories/              # Repository implementations
│   │   │   └── DependencyInjection.cs     # Module registration
│   │   │
│   │   └── BeerCompetition.Competition.API/
│   │       └── Endpoints/                 # Minimal API endpoints
│   │
│   ├── Judging/                           # Judging Module (future)
│   │
│   └── Shared/                            # Shared Kernel
│       ├── BeerCompetition.Shared.Kernel/
│       │   ├── Entity.cs                  # Base entity class
│       │   ├── IAggregateRoot.cs          # Marker interface
│       │   ├── IDomainEvent.cs            # Domain event interface
│       │   ├── ITenantEntity.cs           # Multi-tenancy interface
│       │   └── Result.cs                  # Result pattern for error handling
│       │
│       ├── BeerCompetition.Shared.Contracts/
│       │   └── IIntegrationEvent.cs       # Integration event interface
│       │
│       └── BeerCompetition.Shared.Infrastructure/
│           └── MultiTenancy/
│               └── TenantProvider.cs      # Tenant context provider
│
├── Host/
│   └── BeerCompetition.Host/
│       ├── Program.cs                     # Application entry point
│       ├── appsettings.json              # Configuration
│       └── appsettings.Development.json
│
└── Tests/                                 # Test projects (future)
```

---

## Architecture Principles

### 1. Modular Monolith

The solution is organized as a **modular monolith**, which means:

- **Single Deployment Unit**: All modules run in the same process and are deployed together
- **Clear Module Boundaries**: Each module represents a bounded context from DDD
- **Shared Database**: All modules share the same PostgreSQL database (but with separate tables)
- **Future Microservices**: Modules can be extracted into separate services when needed

**Benefits:**
- Simpler deployment and operations (single service)
- Easier development and debugging (no distributed systems complexity)
- Lower infrastructure costs (one container vs. multiple)
- Migration path to microservices when team/scale requires it

### 2. Vertical Slice Architecture

Each feature is organized as a **vertical slice** containing all layers needed for that feature:

```
Features/CreateCompetition/
├── CreateCompetitionCommand.cs      # MediatR command (what the user wants)
├── CreateCompetitionHandler.cs      # Business logic implementation
├── CreateCompetitionValidator.cs    # FluentValidation rules
└── CreateCompetitionEndpoint.cs     # HTTP endpoint (mapped in API layer)
```

**Benefits:**
- **High Cohesion**: All code for a feature is in one folder
- **Low Coupling**: Features don't depend on each other
- **Easy Navigation**: Find all related code quickly
- **Parallel Development**: Teams can work on different slices without conflicts
- **Independent Testing**: Each slice can be tested independently

### 3. Domain-Driven Design (DDD)

Each module follows DDD tactical patterns:

#### **Entities** (`Domain/Entities/`)
Objects with identity that persist over time. Example: `Competition`, `Entry`.

```csharp
public class Competition : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    // Factory method enforces business rules
    public static Result<Competition> Create(...)
}
```

#### **Value Objects** (`Domain/ValueObjects/`)
Immutable objects compared by value, not identity. Example: `JudgingNumber`, `Score`.

```csharp
public record JudgingNumber
{
    public int Value { get; }
    private JudgingNumber(int value) => Value = value;
    public static Result<JudgingNumber> Create(int value) => ...;
}
```

#### **Aggregates** (`Domain/Entities/`)
Clusters of entities treated as a single unit for data changes. Example: `Flight` (contains `FlightEntry` children).

```csharp
public class Flight : Entity, IAggregateRoot
{
    private readonly List<FlightEntry> _entries = new();
    
    // Enforce invariant: max 12 entries per flight
    public Result AddEntry(FlightEntry entry)
    {
        if (_entries.Count >= 12)
            return Result.Failure("Flight is full");
        _entries.Add(entry);
        return Result.Success();
    }
}
```

#### **Domain Events** (`Domain/Events/`)
Facts that happened in the domain that other parts of the system may need to react to.

```csharp
public record CompetitionCreatedEvent(Guid CompetitionId, Guid TenantId) : IDomainEvent;
```

#### **Repositories** (`Domain/Repositories/` interface, `Infrastructure/Repositories/` implementation)
Abstractions for data access, hiding persistence details from domain.

```csharp
public interface ICompetitionRepository
{
    Task<Competition?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Competition competition, CancellationToken ct = default);
}
```

---

## CQRS Pattern (MediatR)

The application uses **MediatR** to implement **Command Query Responsibility Segregation (CQRS)**:

### Commands (Change State)
```csharp
public record CreateCompetitionCommand(...) : IRequest<Result<Guid>>;

public class CreateCompetitionHandler : IRequestHandler<CreateCompetitionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCompetitionCommand cmd, CancellationToken ct)
    {
        // 1. Validate
        // 2. Create domain entity
        // 3. Persist
        // 4. Publish events
        return Result<Guid>.Success(competition.Id);
    }
}
```

### Queries (Return Data)
```csharp
public record GetCompetitionQuery(Guid Id) : IRequest<Result<CompetitionDto>>;

public class GetCompetitionHandler : IRequestHandler<GetCompetitionQuery, Result<CompetitionDto>>
{
    public async Task<Result<CompetitionDto>> Handle(...)
    {
        var competition = await _repository.GetByIdAsync(query.Id);
        return Result<CompetitionDto>.Success(competition.ToDto());
    }
}
```

**Minimal API Endpoints** map HTTP requests to MediatR commands/queries:

```csharp
app.MapPost("/api/competitions", async (CreateCompetitionCommand cmd, IMediator mediator) =>
{
    var result = await mediator.Send(cmd);
    return result.IsSuccess ? Results.Created(...) : Results.BadRequest(...);
});
```

---

## Multi-Tenancy

All entities implement `ITenantEntity` interface:

```csharp
public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
```

**Enforcement happens at multiple levels:**

1. **PostgreSQL Row-Level Security (RLS)**: Database enforces tenant isolation
2. **Entity Framework Global Query Filters**: Auto-injects `WHERE tenant_id = @current_tenant`
3. **BFF/API Gateway**: Extracts `tenant_id` from JWT and injects via `X-Tenant-ID` header

---

## Dependency Injection

Each module registers its services via `DependencyInjection.cs`:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddCompetitionModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext, Repositories, MediatR handlers, Validators
        return services;
    }
}
```

Host `Program.cs` calls module registration methods:

```csharp
builder.Services.AddCompetitionModule(builder.Configuration);
builder.Services.AddJudgingModule(builder.Configuration);
```

---

## Testing Strategy

### Unit Tests
- Test domain entities, value objects, business rules
- Test command/query handlers in isolation (mock repositories)
- Test validators

### Integration Tests
- Test vertical slices end-to-end (HTTP request → database)
- Use Testcontainers for real PostgreSQL

### E2E Tests (Cypress)
- Test critical user flows through React frontend

---

## Build and Run

### Prerequisites
- .NET 10 SDK
- PostgreSQL 16+ (or Docker)
- IDE: Visual Studio 2025, VS Code, or Rider

### Build
```bash
cd backend
dotnet build BeerCompetition.Monolith.sln
```

### Run
```bash
cd backend/Host/BeerCompetition.Host
dotnet run
```

API will be available at `https://localhost:5001` with Swagger UI at root (`/`).

### Database Migrations
```bash
cd backend/Modules/Competition/BeerCompetition.Competition.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../../Host/BeerCompetition.Host
dotnet ef database update --startup-project ../../Host/BeerCompetition.Host
```

---

## Key Design Decisions

See [Architecture Decision Records (ADRs)](../architecture/decisions/) for detailed rationale:

- **[ADR-009: Modular Monolith with Vertical Slices and DDD](../architecture/decisions/ADR-009-modular-monolith-vertical-slices.md)** - Why modular monolith vs microservices
- **[ADR-002: Multi-Tenancy Strategy](../architecture/decisions/ADR-002-multi-tenancy-strategy.md)** - PostgreSQL RLS + EF Global Filters
- **[ADR-005: CQRS Implementation](../architecture/decisions/ADR-005-cqrs-implementation.md)** - MediatR for command/query separation

---

## Adding New Features (Vertical Slices)

1. **Create feature folder**: `Application/Features/MyFeature/`
2. **Add command/query**: `MyFeatureCommand.cs` (implements `IRequest<Result<T>>`)
3. **Add handler**: `MyFeatureHandler.cs` (implements `IRequestHandler<>`)
4. **Add validator**: `MyFeatureValidator.cs` (extends `AbstractValidator<>`)
5. **Map endpoint**: Add to `CompetitionEndpoints.cs` in API layer
6. **Write tests**: Unit tests for handler, integration tests for endpoint

---

## Future Enhancements

- **Judging Module**: Complete implementation (Domain, Application, Infrastructure, API)
- **Event Sourcing**: Store domain events for audit trail
- **Outbox Pattern**: Reliable event publishing to RabbitMQ
- **Authentication**: Keycloak integration with JWT validation
- **Authorization**: Role-based access control (Organizer, Judge, Entrant)
- **Migrations**: Microservices extraction when scale requires it

---

## References

- [Modular Monolith by Kamil Grzybek](https://www.kamilgrzybek.com/design/modular-monolith-primer/)
- [Vertical Slice Architecture by Jimmy Bogard](https://www.youtube.com/watch?v=SUiWfhAhgQw)
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [CQRS by Greg Young](https://cqrs.wordpress.com/)

---

**Maintainer**: Backend Development Team  
**Last Updated**: 2025-12-22
