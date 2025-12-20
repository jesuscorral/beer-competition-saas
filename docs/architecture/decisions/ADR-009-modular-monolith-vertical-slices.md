# ADR-009: Modular Monolith Architecture with Vertical Slices and DDD

**Date**: 2025-12-19  
**Status**: Accepted  
**Deciders**: Architecture Team  
**Context**: Beer Competition SaaS Platform

---

## Context and Problem Statement

The Beer Competition platform consists of multiple bounded contexts (Competition, Judging, Analytics). We need to decide on the optimal architecture pattern that:
- Supports independent development of domain features
- Maintains clear module boundaries
- Enables potential future microservices extraction
- Simplifies deployment and operations (especially for MVP)
- Scales with team growth
- Applies Domain-Driven Design (DDD) principles

Should we build **distributed microservices from day one**, or start with a **modular monolith** that can evolve into microservices?

---

## Decision Drivers

- **Team Size**: Initially 1-2 developers (MVP phase)
- **Operational Complexity**: Minimize infrastructure overhead
- **Development Velocity**: Faster iteration for MVP
- **Domain Boundaries**: Clear separation (Competition, Judging, Analytics)
- **Future Scalability**: Ability to extract microservices later
- **Deployment Simplicity**: Single deployment unit for MVP
- **Testing**: Easier integration testing with single process
- **Cost**: Lower infrastructure costs during MVP

---

## Considered Options

### 1. **Distributed Microservices from Day One**
Each service (Competition, Judging, BFF) deployed as separate containers from the start.

**Pros:**
- ✅ Independent scaling and deployment
- ✅ Technology heterogeneity (could use different languages)
- ✅ Team autonomy (each service owned by a team)

**Cons:**
- ❌ High operational complexity (3+ services, message bus, distributed tracing)
- ❌ Slower development velocity (cross-service changes require coordination)
- ❌ Distributed transactions and eventual consistency complexity
- ❌ Higher infrastructure costs (3+ containers, load balancers)
- ❌ Overkill for small team and MVP phase

---

### 2. **Traditional Monolith (N-Tier Architecture)**
Single application with layered architecture (Controllers → Services → Repositories).

**Pros:**
- ✅ Simple deployment (single binary)
- ✅ Fast development for small team
- ✅ Easy to reason about and debug

**Cons:**
- ❌ Tight coupling between features
- ❌ Difficult to extract microservices later
- ❌ No clear domain boundaries
- ❌ Risk of "big ball of mud" as codebase grows
- ❌ All features share the same lifecycle (can't deploy independently)

---

### 3. **Modular Monolith with Vertical Slices and DDD** (Chosen)
Single deployable application with clear internal module boundaries following DDD principles and vertical slice architecture.

**Pros:**
- ✅ Clear module boundaries (Competition, Judging, Shared)
- ✅ Simple deployment (single binary) but internally modular
- ✅ Easier to extract microservices later (module = microservice)
- ✅ Faster development than distributed microservices
- ✅ Lower operational complexity (one service to monitor)
- ✅ DDD tactical patterns (Aggregates, Entities, Value Objects, Repositories)
- ✅ Vertical slices: Each feature is self-contained (UI → Business Logic → Database)
- ✅ Scales well with team growth (teams can own modules)

**Cons:**
- ⚠️ Requires discipline to maintain module boundaries
- ⚠️ Cannot scale modules independently (yet)
- ⚠️ Refactoring to microservices requires effort (but much easier than N-tier)

---

## Decision Outcome

**Chosen Option: Modular Monolith with Vertical Slices and Domain-Driven Design (DDD)**

We will build a **single .NET application** with **clear internal module boundaries** following **DDD tactical patterns** and **vertical slice architecture** within each module.

### Architecture Overview

```
BeerCompetition.Monolith/
├── Modules/
│   ├── Competition/                # Competition Module (Bounded Context)
│   │   ├── Domain/                 # DDD: Entities, Aggregates, Value Objects, Domain Events
│   │   ├── Application/            # Vertical Slices: Feature folders with commands/queries
│   │   ├── Infrastructure/         # EF Core, Repositories, Event Publishing
│   │   └── API/                    # HTTP Endpoints (Minimal APIs or Controllers)
│   │
│   ├── Judging/                    # Judging Module (Bounded Context)
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── API/
│   │
│   └── Shared/                     # Shared Kernel (common across modules)
│       ├── Kernel/                 # Base classes, interfaces
│       ├── Contracts/              # Integration events (between modules)
│       └── Infrastructure/         # Cross-cutting concerns (logging, telemetry)
│
├── BFF/                            # Backend-for-Frontend (API Gateway)
│   └── Gateway/                    # Token validation, routing, rate limiting
│
└── Host/                           # Application entry point
    └── Program.cs                  # Startup, DI configuration, middleware
```

---

## Implementation Details

### 1. **Module Boundaries (Bounded Contexts)**

Each module represents a **bounded context** from Domain-Driven Design:

#### **Competition Module**
- **Domain**: Competitions, Entries, Styles, Payments, BottleReception
- **Responsibilities**: Lifecycle of competitions, entry submissions, payment processing, bottle tracking
- **Events Published**: `EntrySubmitted`, `EntryPaid`, `BottlesCheckedIn`, `ResultsPublished`
- **Events Consumed**: `ScoresheetSubmitted` (for progress tracking)

#### **Judging Module**
- **Domain**: Flights, FlightAssignments, Scoresheets, BestOfShow (BOS)
- **Responsibilities**: Flight creation, judge assignments, scoresheet entry, BOS judging, consensus scoring
- **Events Published**: `ScoresheetSubmitted`, `ConsensusCompleted`, `BOSCompleted`
- **Events Consumed**: `EntrySubmitted` (for flight assignment)

#### **Shared Kernel**
- **Domain**: Multi-tenancy, authentication, result patterns, domain event infrastructure
- **Responsibilities**: Cross-cutting concerns, base classes, integration event contracts

---

### 2. **Vertical Slice Architecture (Within Each Module)**

Instead of horizontal layers (Controllers → Services → Repositories), we organize by **feature slices**:

```
Modules/Competition/Application/
└── Features/
    ├── CreateCompetition/                # Vertical Slice
    │   ├── CreateCompetitionCommand.cs   # MediatR command
    │   ├── CreateCompetitionHandler.cs   # Business logic
    │   ├── CreateCompetitionValidator.cs # FluentValidation
    │   └── CreateCompetitionEndpoint.cs  # Minimal API endpoint
    │
    ├── SubmitEntry/                      # Vertical Slice
    │   ├── SubmitEntryCommand.cs
    │   ├── SubmitEntryHandler.cs
    │   ├── SubmitEntryValidator.cs
    │   └── SubmitEntryEndpoint.cs
    │
    └── GetCompetitionById/               # Vertical Slice (Query)
        ├── GetCompetitionByIdQuery.cs
        ├── GetCompetitionByIdHandler.cs
        └── GetCompetitionByIdEndpoint.cs
```

**Benefits:**
- ✅ **High Cohesion**: All code for a feature is in one folder
- ✅ **Low Coupling**: Features don't depend on each other
- ✅ **Easy to Navigate**: Find all related code quickly
- ✅ **Parallel Development**: Teams can work on different slices without conflicts
- ✅ **Testability**: Each slice can be tested independently

---

### 3. **Domain-Driven Design (DDD) Tactical Patterns**

#### **Entities** (Identity-based objects)
```csharp
// Modules/Competition/Domain/Entities/Competition.cs
public class Competition : Entity, IAggregateRoot, ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime RegistrationDeadline { get; private set; }
    public CompetitionStatus Status { get; private set; }
    
    // Domain Events
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Factory Method (prevents invalid state)
    public static Result<Competition> Create(Guid tenantId, string name, DateTime registrationDeadline)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Competition>.Failure("Competition name is required");
        
        if (registrationDeadline < DateTime.UtcNow)
            return Result<Competition>.Failure("Registration deadline must be in the future");
        
        var competition = new Competition
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            RegistrationDeadline = registrationDeadline,
            Status = CompetitionStatus.Draft
        };
        
        competition.AddDomainEvent(new CompetitionCreatedEvent(competition.Id, tenantId));
        return Result<Competition>.Success(competition);
    }

    // Business Logic Methods
    public Result OpenForRegistration()
    {
        if (Status != CompetitionStatus.Draft)
            return Result.Failure("Can only open competitions in Draft status");
        
        Status = CompetitionStatus.Open;
        AddDomainEvent(new CompetitionOpenedEvent(Id, TenantId));
        return Result.Success();
    }

    private void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

#### **Value Objects** (Immutable, compared by value)
```csharp
// Modules/Competition/Domain/ValueObjects/JudgingNumber.cs
public record JudgingNumber
{
    public int Value { get; }

    private JudgingNumber(int value) => Value = value;

    public static Result<JudgingNumber> Create(int value)
    {
        if (value <= 0)
            return Result<JudgingNumber>.Failure("Judging number must be positive");
        
        return Result<JudgingNumber>.Success(new JudgingNumber(value));
    }

    public override string ToString() => Value.ToString();
}
```

#### **Aggregates** (Consistency Boundaries)
```csharp
// Modules/Judging/Domain/Aggregates/Flight.cs
public class Flight : Entity, IAggregateRoot, ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CompetitionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    
    // Aggregate: Flight owns FlightEntries (cannot exist independently)
    private readonly List<FlightEntry> _entries = new();
    public IReadOnlyCollection<FlightEntry> Entries => _entries.AsReadOnly();

    // Invariant: Flight must have 2-12 entries
    public Result AddEntry(Guid entryId, int judgingNumber)
    {
        if (_entries.Count >= 12)
            return Result.Failure("Flight cannot exceed 12 entries");
        
        if (_entries.Any(e => e.EntryId == entryId))
            return Result.Failure("Entry already in flight");
        
        _entries.Add(new FlightEntry(Id, entryId, judgingNumber));
        AddDomainEvent(new EntryAddedToFlightEvent(Id, entryId));
        return Result.Success();
    }
}

// Child entity (part of Flight aggregate)
public class FlightEntry : Entity
{
    public Guid FlightId { get; private set; }
    public Guid EntryId { get; private set; }
    public int JudgingNumber { get; private set; }

    internal FlightEntry(Guid flightId, Guid entryId, int judgingNumber)
    {
        FlightId = flightId;
        EntryId = entryId;
        JudgingNumber = judgingNumber;
    }
}
```

#### **Domain Events** (Asynchronous communication)
```csharp
// Modules/Competition/Domain/Events/CompetitionCreatedEvent.cs
public record CompetitionCreatedEvent(Guid CompetitionId, Guid TenantId) : IDomainEvent;

// Modules/Judging/Application/EventHandlers/CompetitionCreatedEventHandler.cs
public class CompetitionCreatedEventHandler : INotificationHandler<CompetitionCreatedEvent>
{
    private readonly IFlightRepository _flightRepository;

    public async Task Handle(CompetitionCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Business logic: Prepare for flight creation when competition is created
        await _flightRepository.CreateFlightTemplateAsync(notification.CompetitionId, cancellationToken);
    }
}
```

#### **Repositories** (Persistence Abstraction)
```csharp
// Modules/Competition/Domain/Repositories/ICompetitionRepository.cs
public interface ICompetitionRepository
{
    Task<Competition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Competition>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Competition competition, CancellationToken cancellationToken = default);
    Task UpdateAsync(Competition competition, CancellationToken cancellationToken = default);
}

// Modules/Competition/Infrastructure/Repositories/CompetitionRepository.cs
public class CompetitionRepository : ICompetitionRepository
{
    private readonly ApplicationDbContext _context;

    public CompetitionRepository(ApplicationDbContext context) => _context = context;

    public async Task<Competition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Competitions.FindAsync(new object[] { id }, cancellationToken);

    public async Task AddAsync(Competition competition, CancellationToken cancellationToken = default)
    {
        await _context.Competitions.AddAsync(competition, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

---

### 4. **CQRS with MediatR (Within Vertical Slices)**

```csharp
// Modules/Competition/Application/Features/CreateCompetition/CreateCompetitionCommand.cs
public record CreateCompetitionCommand(
    string Name,
    DateTime RegistrationDeadline
) : IRequest<Result<Guid>>;

// Modules/Competition/Application/Features/CreateCompetition/CreateCompetitionHandler.cs
public class CreateCompetitionHandler : IRequestHandler<CreateCompetitionCommand, Result<Guid>>
{
    private readonly ICompetitionRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEventPublisher _eventPublisher;

    public async Task<Result<Guid>> Handle(CreateCompetitionCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate (FluentValidation runs via MediatR pipeline behavior)
        
        // 2. Create domain entity
        var tenantId = _tenantProvider.CurrentTenantId;
        var competitionResult = Competition.Create(tenantId, request.Name, request.RegistrationDeadline);
        
        if (competitionResult.IsFailure)
            return Result<Guid>.Failure(competitionResult.Error);
        
        // 3. Persist
        await _repository.AddAsync(competitionResult.Value, cancellationToken);
        
        // 4. Publish domain events (Outbox Pattern)
        foreach (var domainEvent in competitionResult.Value.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }
        
        competitionResult.Value.ClearDomainEvents();
        
        return Result<Guid>.Success(competitionResult.Value.Id);
    }
}

// Modules/Competition/Application/Features/CreateCompetition/CreateCompetitionValidator.cs
public class CreateCompetitionValidator : AbstractValidator<CreateCompetitionCommand>
{
    public CreateCompetitionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.RegistrationDeadline).GreaterThan(DateTime.UtcNow);
    }
}

// Modules/Competition/API/Endpoints/CompetitionEndpoints.cs
public static class CompetitionEndpoints
{
    public static void MapCompetitionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/competitions")
            .RequireAuthorization("OrganizerOnly")
            .WithTags("Competitions");

        group.MapPost("/", async (CreateCompetitionCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}
```

---

### 5. **Module Communication**

#### **In-Process Events (Domain Events)**
Modules communicate via **domain events** published through MediatR:

```csharp
// Modules/Competition/Application/Features/SubmitEntry/SubmitEntryHandler.cs
public class SubmitEntryHandler : IRequestHandler<SubmitEntryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SubmitEntryCommand request, CancellationToken cancellationToken)
    {
        // ... create entry ...
        
        // Publish domain event (handled by Judging module)
        await _mediator.Publish(new EntrySubmittedEvent(entry.Id, entry.CompetitionId, entry.JudgingNumber), cancellationToken);
        
        return Result<Guid>.Success(entry.Id);
    }
}

// Modules/Judging/Application/EventHandlers/EntrySubmittedEventHandler.cs
public class EntrySubmittedEventHandler : INotificationHandler<EntrySubmittedEvent>
{
    public async Task Handle(EntrySubmittedEvent notification, CancellationToken cancellationToken)
    {
        // Business logic: Assign entry to flight
        await _flightService.AssignEntryToFlightAsync(notification.EntryId, cancellationToken);
    }
}
```

#### **Integration Events (Between Modules via RabbitMQ)**
For **eventual consistency** and **resilience**, modules can publish integration events to RabbitMQ:

```csharp
// Modules/Shared/Contracts/EntrySubmittedIntegrationEvent.cs
public record EntrySubmittedIntegrationEvent(
    Guid EventId,
    DateTime Timestamp,
    Guid TenantId,
    Guid EntryId,
    Guid CompetitionId,
    int JudgingNumber
) : IIntegrationEvent;

// Modules/Competition/Application/Features/SubmitEntry/SubmitEntryHandler.cs
await _eventPublisher.PublishIntegrationEventAsync(
    new EntrySubmittedIntegrationEvent(
        Guid.NewGuid(),
        DateTime.UtcNow,
        entry.TenantId,
        entry.Id,
        entry.CompetitionId,
        entry.JudgingNumber
    ),
    cancellationToken
);
```

---

### 6. **Dependency Injection and Module Registration**

```csharp
// Host/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register modules
builder.Services.AddCompetitionModule(builder.Configuration);
builder.Services.AddJudgingModule(builder.Configuration);
builder.Services.AddSharedKernel(builder.Configuration);

// Modules/Competition/Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddCompetitionModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<CompetitionDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));

        // Register repositories
        services.AddScoped<ICompetitionRepository, CompetitionRepository>();

        // Register MediatR handlers (scan assembly)
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Register FluentValidation
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
```

---

### 7. **Testing Vertical Slices**

```csharp
// Tests/Integration/Competition/CreateCompetitionTests.cs
public class CreateCompetitionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    [Fact]
    public async Task CreateCompetition_ValidData_ReturnsCompetitionId()
    {
        // Arrange
        var client = _factory.CreateClient();
        var command = new CreateCompetitionCommand("Spring Classic 2025", DateTime.UtcNow.AddDays(30));

        // Act
        var response = await client.PostAsJsonAsync("/api/competitions", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var competitionId = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, competitionId);
    }
}
```

---

## Migration Path to Microservices

When the team grows or specific modules need independent scaling:

1. **Extract Module**: Move `Modules/Competition/` to `services/competition/`
2. **Replace In-Process Events**: Convert MediatR domain events to RabbitMQ integration events
3. **Deploy Independently**: Build Docker image for extracted service
4. **Update BFF**: Route `/api/competitions` requests to new service

**Example: Extract Competition Module**
```
Before (Monolith):
BeerCompetition.Monolith/Modules/Competition/ → Single deployment

After (Microservice):
services/competition/ → Separate deployment
- Domain/ (same)
- Application/ (same, but publish to RabbitMQ instead of MediatR)
- Infrastructure/ (same)
- API/ (same, but standalone ASP.NET Core app)
```

---

## Consequences

### Positive

✅ **Simple Deployment**: Single binary during MVP, easier operations  
✅ **Clear Boundaries**: Modules enforce separation (DDD bounded contexts)  
✅ **Fast Development**: No distributed systems complexity  
✅ **Vertical Slices**: High cohesion, low coupling, easy to navigate  
✅ **DDD Patterns**: Rich domain models with business logic in entities  
✅ **Future-Proof**: Easy to extract microservices when needed  
✅ **Testability**: Integration tests run in-process (fast)  
✅ **Cost-Effective**: Lower infrastructure costs during MVP  

### Negative

⚠️ **Discipline Required**: Teams must respect module boundaries  
⚠️ **Cannot Scale Modules Independently**: All modules scale together  
⚠️ **Shared Database**: Potential contention on high-traffic tables  

### Mitigation Strategies

1. **Enforce Module Boundaries**: Use `[assembly: InternalsVisibleTo]` to hide internals
2. **Architectural Tests**: Use ArchUnitNET to validate dependencies
3. **Code Reviews**: Ensure modules don't bypass abstractions
4. **Documentation**: Maintain module interaction diagrams

---

## Related Decisions

- [ADR-001: Tech Stack Selection](ADR-001-tech-stack-selection.md) - .NET 10 chosen for backend
- [ADR-005: CQRS Implementation](ADR-005-cqrs-implementation.md) - MediatR for command/query separation
- [ADR-008: Database Migrations Strategy](ADR-008-database-migrations-strategy.md) - Code-first migrations

---

## References

- [Modular Monolith by Kamil Grzybek](https://www.kamilgrzybek.com/design/modular-monolith-primer/)
- [Vertical Slice Architecture by Jimmy Bogard](https://www.youtube.com/watch?v=SUiWfhAhgQw)
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [Implementing DDD by Vaughn Vernon](https://vaughnvernon.com/)

---

**Last Updated**: 2025-12-19  
**Next Review**: Post-Sprint 0 (after initial implementation)
