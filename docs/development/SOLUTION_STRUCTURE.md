# Solution Structure Documentation

**Project**: Beer Competition SaaS Platform  
**Architecture**: Modular Monolith with Vertical Slices and DDD  
**Last Updated**: 2026-01-04

---

## Overview

This document describes the structure of the Beer Competition SaaS solution, which implements a **modular monolith architecture** with **vertical slices** and **Domain-Driven Design (DDD)** tactical patterns.

The solution is organized as a single deployable application with clear internal module boundaries following bounded contexts from DDD. Each module can potentially be extracted into a microservice in the future.

---

## Solution Structure

```
beer-competition-saas/
├── BeerCompetition.sln                    # Solution file
├── Directory.Build.props                  # Shared build configuration
├── Directory.Packages.props               # Central Package Management (CPM)
│
├── src/
│   ├── backend/
│   │   ├── BFF/                           # Backend-for-Frontend (API Gateway)
│   │   │   └── BFF.ApiGateway/
│   │   │       ├── Program.cs             # YARP reverse proxy + Token Exchange
│   │   │       ├── appsettings.json       # Service routes, Keycloak config
│   │   │       └── Services/
│   │   │           └── TokenExchangeService.cs  # OAuth 2.0 Token Exchange
│   │   │
│   │   ├── Host/                          # Application Host (Modular Monolith)
│   │   │   └── BeerCompetition.Host/
│   │   │       ├── Program.cs             # Entry point, module registration
│   │   │       ├── appsettings.json       # Database, RabbitMQ, Keycloak
│   │   │       └── public partial class Program { }  # WebApplicationFactory support
│   │   │
│   │   └── Modules/                       # Bounded Contexts (DDD)
│   │       ├── Competition/               # Competition Module
│   │       │   ├── BeerCompetition.Competition.Domain/
│   │       │   │   ├── Aggregates/        # Aggregate roots (Competition, Tenant, Entry)
│   │       │   │   ├── Entities/          # Domain entities
│   │       │   │   ├── Events/            # Domain events
│   │       │   │   └── Repositories/      # Repository interfaces
│   │       │   │
│   │       │   ├── BeerCompetition.Competition.Application/
│   │       │   │   └── Features/          # Vertical slices (CQRS)
│   │       │   │       ├── RegisterOrganizer/
│   │       │   │       │   ├── RegisterOrganizerCommand.cs
│   │       │   │       │   ├── RegisterOrganizerHandler.cs
│   │       │   │       │   └── RegisterOrganizerValidator.cs
│   │       │   │       ├── CreateCompetition/
│   │       │   │       └── ...
│   │       │   │
│   │       │   ├── BeerCompetition.Competition.Infrastructure/
│   │       │   │   ├── Persistence/       # EF Core DbContext, configurations
│   │       │   │   │   ├── ApplicationDbContext.cs
│   │       │   │   │   ├── Configurations/
│   │       │   │   │   └── Migrations/
│   │       │   │   ├── Repositories/      # Repository implementations
│   │       │   │   └── DependencyInjection.cs  # Module registration
│   │       │   │
│   │       │   └── BeerCompetition.Competition.API/
│   │       │       └── Endpoints/         # Minimal API endpoints
│   │       │
│   │       ├── Judging/                   # Judging Module (future)
│   │       │
│   │       └── Shared/                    # Shared Kernel
│   │           ├── BeerCompetition.Shared.Kernel/
│   │           │   ├── Entity.cs          # Base entity class
│   │           │   ├── IAggregateRoot.cs  # Marker interface
│   │           │   ├── IDomainEvent.cs    # Domain event interface
│   │           │   ├── ITenantEntity.cs   # Multi-tenancy interface
│   │           │   └── Result.cs          # Result pattern for error handling
│   │           │
│   │           ├── BeerCompetition.Shared.Contracts/
│   │           │   └── IIntegrationEvent.cs  # Integration event interface
│   │           │
│   │           └── BeerCompetition.Shared.Infrastructure/
│   │               └── MultiTenancy/
│   │                   ├── ITenantProvider.cs  # Tenant context abstraction
│   │                   └── TenantProvider.cs   # Tenant context provider
│   │
│   └── frontend/                          # React PWA (future)
│
├── tests/
│   ├── BFF/                               # BFF API Gateway Tests
│   │   └── BFF.ApiGateway.Tests/
│   │       ├── Authentication/
│   │       ├── Middleware/
│   │       └── Services/
│   │
│   └── Modules/
│       └── Competition/
│           └── BeerCompetition.Competition.IntegrationTests/
│               ├── Infrastructure/         # Test infrastructure
│               │   ├── IntegrationTestWebApplicationFactory.cs
│               │   ├── IntegrationTestBase.cs
│               │   └── TestTenantProvider.cs
│               ├── Builders/              # Builder Pattern for test data
│               │   ├── TenantBuilder.cs
│               │   └── CompetitionBuilder.cs
│               └── Features/              # Feature integration tests
│                   └── RegisterOrganizer/
│                       └── RegisterOrganizerIntegrationTests.cs
│
├── infrastructure/
│   ├── docker-compose.yml                 # PostgreSQL, RabbitMQ, Keycloak
│   └── keycloak/
│       └── realm-export.json              # Keycloak realm configuration
│
└── docs/
    ├── architecture/
    │   ├── ARCHITECTURE.md
    │   ├── SERVICE_AUDIENCES_TOKEN_EXCHANGE.md
    │   └── decisions/                     # Architecture Decision Records
    ├── development/
    │   └── SOLUTION_STRUCTURE.md          # This document
    └── roadmap/
        └── IMPLEMENTATION_ROADMAP.md
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
- Framework: **xUnit + FluentAssertions + NSubstitute**

### Integration Tests (Testcontainers + WebApplicationFactory)

**Updated Implementation (2026-01-04)**: The project uses **modern integration testing infrastructure** with:

#### Infrastructure Components

1. **IntegrationTestWebApplicationFactory**:
   - Manages PostgreSQL containers (Testcontainers)
   - Provides test infrastructure (mocked services)
   - Replaces `ITenantProvider` with `TestTenantProvider`
   - Applies database migrations automatically

2. **IntegrationTestBase**:
   - Base class for all integration tests
   - Uses **Respawn** for intelligent database cleanup (preserves schema, truncates data)
   - Provides `GetFreshDbContext()` helper
   - Clears tenant context between tests

3. **TestTenantProvider**:
   - Dynamic tenant context for tests
   - `SetTenant(Guid)` to switch tenant context
   - `ClearTenant()` to reset context
   - Eliminates need for `IgnoreQueryFilters()` in most tests

4. **Builder Pattern**:
   - `TenantBuilder`: Fluent builder for Tenant entities
   - `CompetitionBuilder`: Fluent builder for Competition entities
   - Provides sensible defaults
   - Validates domain rules at build time

**Example Integration Test:**

```csharp
public class RegisterOrganizerIntegrationTests : IntegrationTestBase
{
    public RegisterOrganizerIntegrationTests(IntegrationTestWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Handle_CompleteFlow_CreatesAllEntities()
    {
        // Arrange: Mock Keycloak
        Factory.KeycloakService
            .Setup(s => s.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        // Build command
        var command = new RegisterOrganizerCommand(
            Email: "john@example.com",
            OrganizationName: "John's Homebrew Club");

        // Act: Execute command
        var result = await _mediator.Send(command);

        // Assert: Command succeeded
        result.IsSuccess.Should().BeTrue();

        // Set tenant context for verification
        Factory.TenantProvider.SetTenant(result.Value.TenantId);

        // Verify: Tenant created
        var verifyContext = GetFreshDbContext();
        var tenant = await verifyContext.Tenants.FirstOrDefaultAsync();
        tenant.Should().NotBeNull();
        tenant!.Email.Should().Be("john@example.com");
    }
}
```

**Benefits:**
- ✅ Real PostgreSQL 16 container (no mocks)
- ✅ Automatic migrations and cleanup (Respawn)
- ✅ Dynamic tenant context (TestTenantProvider)
- ✅ Builder pattern for readable test setup
- ✅ Fast execution (~1-5 seconds per test)

### E2E Tests (Cypress)
- Test critical user flows through React frontend
- Offline PWA testing (service workers + IndexedDB)

### Related Documentation
- **[ADR-006: Testing Strategy](../architecture/decisions/ADR-006-testing-strategy.md)** - Detailed testing approach
- **[Integration Tests README](../../tests/Modules/Competition/README.md)** - Test infrastructure guide

---

## Build and Run

### Prerequisites
- **.NET 10 SDK**
- **Docker** (for PostgreSQL, RabbitMQ, Keycloak, Testcontainers)
- **Node.js 20+** (for frontend)
- **IDE**: Visual Studio 2025, VS Code, or Rider

### Build Backend
```bash
cd beer-competition-saas
dotnet build BeerCompetition.sln
```

### Run Infrastructure Services
```bash
cd infrastructure
docker-compose up -d  # Starts PostgreSQL, RabbitMQ, Keycloak
```

### Run Backend Host
```bash
cd src/backend/Host/BeerCompetition.Host
dotnet run
```

API will be available at `https://localhost:5001` with Swagger UI at root (`/`).

### Run BFF (API Gateway)
```bash
cd src/backend/BFF/BFF.ApiGateway
dotnet run
```

BFF will be available at `https://localhost:5190` (reverse proxy to services).

### Database Migrations

**Create Migration:**
```bash
cd src/backend/Modules/Competition/BeerCompetition.Competition.Infrastructure
dotnet ef migrations add MyMigration --startup-project ../../../Host/BeerCompetition.Host
```

**Apply Migrations:**
```bash
dotnet ef database update --startup-project ../../../Host/BeerCompetition.Host
```

**Development Tenant**: Automatically configured (`11111111-1111-1111-1111-111111111111`) in `TenantProvider.cs`.

### Run Tests
```bash
# Unit tests only
dotnet test --filter "Category=Unit"

# Integration tests (requires Docker for Testcontainers)
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# All tests
dotnet test
```

---

## Key Design Decisions

See [Architecture Decision Records (ADRs)](../architecture/decisions/) for detailed rationale:

- **[ADR-009: Modular Monolith with Vertical Slices and DDD](../architecture/decisions/ADR-009-modular-monolith-vertical-slices.md)** - Why modular monolith vs microservices
- **[ADR-002: Multi-Tenancy Strategy](../architecture/decisions/ADR-002-multi-tenancy-strategy.md)** - PostgreSQL RLS + EF Global Filters
- **[ADR-005: CQRS Implementation](../architecture/decisions/ADR-005-cqrs-implementation.md)** - MediatR for command/query separation
- **[ADR-006: Testing Strategy](../architecture/decisions/ADR-006-testing-strategy.md)** - Testcontainers + Builder Pattern
- **[ADR-010: Token Exchange Pattern](../architecture/decisions/ADR-010-token-exchange-pattern.md)** - OAuth 2.0 Token Exchange for service security

---

## Adding New Features (Vertical Slices)

1. **Create feature folder**: `Application/Features/MyFeature/`
2. **Add command/query**: `MyFeatureCommand.cs` (implements `IRequest<Result<T>>`)
3. **Add handler**: `MyFeatureHandler.cs` (implements `IRequestHandler<>`)
4. **Add validator**: `MyFeatureValidator.cs` (extends `AbstractValidator<>`)
5. **Map endpoint**: Add to endpoints in API layer (or BFF routing)
6. **Write tests**: 
   - Unit tests for handler
   - Integration tests using `IntegrationTestBase` + builders
   - E2E tests for critical flows

**Example (CreateCompetition feature):**
```bash
src/backend/Modules/Competition/
└── BeerCompetition.Competition.Application/
    └── Features/
        └── CreateCompetition/
            ├── CreateCompetitionCommand.cs
            ├── CreateCompetitionHandler.cs
            └── CreateCompetitionValidator.cs
```

---

## Central Package Management (CPM)

The solution uses **Central Package Management** via `Directory.Packages.props`:

**Benefits:**
- ✅ Single source of truth for package versions
- ✅ Consistent versions across all projects
- ✅ Easier dependency upgrades
- ✅ Reduced `.csproj` file noise

**Example `Directory.Packages.props`:**
```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
    <PackageVersion Include="MediatR" Version="13.0.1" />
    <PackageVersion Include="Testcontainers.PostgreSql" Version="4.10.0" />
    <PackageVersion Include="Respawn" Version="7.0.0" />
  </ItemGroup>
</Project>
```

**Adding new package:**
1. Add to `Directory.Packages.props` with version
2. Reference in `.csproj` WITHOUT version:
   ```xml
   <PackageReference Include="MyNewPackage" />
   ```

---

## Future Enhancements

- **Judging Module**: Complete implementation (Domain, Application, Infrastructure, API)
- **Event Sourcing**: Store domain events for audit trail
- **Outbox Pattern**: Reliable event publishing to RabbitMQ (already partially implemented)
- **Frontend**: React PWA with offline support (in progress)
- **Migrations**: Microservices extraction when scale requires it
- **Notifications Module**: Email/SMS notifications for competition updates
- **Payment Service**: Stripe integration for entry fees

---

## References

- [Modular Monolith by Kamil Grzybek](https://www.kamilgrzybek.com/design/modular-monolith-primer/)
- [Vertical Slice Architecture by Jimmy Bogard](https://www.youtube.com/watch?v=SUiWfhAhgQw)
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [CQRS by Greg Young](https://cqrs.wordpress.com/)

---

**Maintainer**: Backend Development Team  
**Last Updated**: 2026-01-04
