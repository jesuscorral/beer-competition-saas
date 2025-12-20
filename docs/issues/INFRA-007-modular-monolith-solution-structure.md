# Issue: [INFRA-007] Setup Modular Monolith Solution Structure with Vertical Slices

## 🎯 User Story

As a backend developer,
I want the solution structured as a modular monolith with vertical slices and DDD tactical patterns,
So that the codebase is organized for maintainability, testability, and future microservices extraction.

## ✅ Acceptance Criteria

- [ ] Solution file created: **BeerCompetition.Monolith.sln**
- [ ] **Modules** folder structure with Competition, Judging, and Shared modules
- [ ] Each module follows **Domain/Application/Infrastructure/API** structure (Clean Architecture)
- [ ] **Vertical slices** implemented: Feature folders contain command/query, handler, validator, endpoint
- [ ] **DDD tactical patterns**: Base classes for Entity, AggregateRoot, ValueObject, DomainEvent
- [ ] **Result<T> pattern** implemented for error handling (no exceptions for business logic)
- [ ] **ITenantEntity interface** for multi-tenancy enforcement
- [ ] **Shared Kernel** with common abstractions (Kernel, Contracts, Infrastructure)
- [ ] **Host project** with Program.cs, dependency injection, and middleware configuration
- [ ] All projects compile successfully with **dotnet build**
- [ ] Solution structure documented in **docs/development/SOLUTION_STRUCTURE.md**

## 🔧 Technical Requirements

- **Architecture**: Modular Monolith (ADR-009)
- **Framework**: .NET 10
- **Organization**: Vertical Slices within each module
- **DDD Patterns**: Entities, Aggregates, Value Objects, Repositories, Domain Events
- **CQRS**: MediatR for command/query separation
- **Dependency Injection**: Each module registers services via DependencyInjection.cs

## 📋 Technical Notes

- Follow **[ADR-009: Modular Monolith with Vertical Slices and DDD](docs/architecture/decisions/ADR-009-modular-monolith-vertical-slices.md)**
- Reference project structure in **docs/architecture/ARCHITECTURE.md**
- Use **.editorconfig** for consistent code formatting
- Enable **nullable reference types** in all projects
- Configure **AssemblyInfo** with InternalsVisibleTo for testing

## 🗂️ Directory Structure

```
backend/
├── BeerCompetition.Monolith.sln
├── Modules/
│   ├── Competition/
│   │   ├── BeerCompetition.Competition.Domain/
│   │   │   ├── Entities/
│   │   │   ├── ValueObjects/
│   │   │   ├── Events/
│   │   │   └── Repositories/
│   │   ├── BeerCompetition.Competition.Application/
│   │   │   └── Features/
│   │   │       ├── CreateCompetition/
│   │   │       │   ├── CreateCompetitionCommand.cs
│   │   │       │   ├── CreateCompetitionHandler.cs
│   │   │       │   ├── CreateCompetitionValidator.cs
│   │   │       │   └── CreateCompetitionEndpoint.cs
│   │   │       └── ... (other features)
│   │   ├── BeerCompetition.Competition.Infrastructure/
│   │   │   ├── EntityConfigurations/
│   │   │   ├── Migrations/
│   │   │   ├── Repositories/
│   │   │   └── DependencyInjection.cs
│   │   └── BeerCompetition.Competition.API/
│   │       └── Endpoints/
│   ├── Judging/
│   │   └── [same structure as Competition]
│   └── Shared/
│       ├── BeerCompetition.Shared.Kernel/
│       │   ├── Entity.cs
│       │   ├── IAggregateRoot.cs
│       │   ├── IDomainEvent.cs
│       │   ├── ITenantEntity.cs
│       │   └── Result.cs
│       ├── BeerCompetition.Shared.Contracts/
│       │   └── (Integration events)
│       └── BeerCompetition.Shared.Infrastructure/
│           ├── Logging/
│           ├── Telemetry/
│           └── MultiTenancy/
├── BFF/
│   └── BeerCompetition.BFF.Gateway/
└── Host/
    ├── BeerCompetition.Host/
    │   ├── Program.cs
    │   ├── appsettings.json
    │   └── Dockerfile
```

## 🔗 Integration Contracts

**Result<T> Pattern Example:**

```csharp
public record Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }

    public static Result<T> Success(T value) => new(true, value, string.Empty);
    public static Result<T> Failure(string error) => new(false, default, error);
}
```

**Base Entity:**

```csharp
public abstract class Entity : ITenantEntity
{
    public Guid Id { get; protected set; }
    public Guid TenantId { get; set; }
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

**Vertical Slice Example (CreateCompetition):**

```csharp
// Command
public record CreateCompetitionCommand(string Name, DateTime RegistrationDeadline) : IRequest<Result<Guid>>;

// Handler
public class CreateCompetitionHandler : IRequestHandler<CreateCompetitionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCompetitionCommand request, CancellationToken cancellationToken)
    {
        // Business logic here
    }
}

// Validator
public class CreateCompetitionValidator : AbstractValidator<CreateCompetitionCommand>
{
    public CreateCompetitionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}

// Endpoint
public static class CompetitionEndpoints
{
    public static void MapCompetitionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/competitions", async (CreateCompetitionCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();
    }
}
```

## 🔗 Dependencies

- **Blocks**: Issue #6 (PostgreSQL schema), Issue #7 (Event Outbox), all feature development
- **Requires**: Issue #2 (Docker Compose environment)
- **References**: ADR-009 (Modular Monolith), ADR-008 (Code-First Migrations), ADR-005 (CQRS)

## ✔️ Definition of Done

- [ ] All acceptance criteria met
- [ ] Solution compiles without errors: **dotnet build**
- [ ] Base classes and interfaces implemented (Entity, Result<T>, ITenantEntity)
- [ ] Example vertical slice created (CreateCompetition) as template
- [ ] Dependency injection configured for all modules
- [ ] **docs/development/SOLUTION_STRUCTURE.md** created with detailed explanation
- [ ] **README.md** updated with solution structure section
- [ ] **copilot-instructions.md** updated with solution structure guidance
- [ ] Unit tests pass for base classes
- [ ] Peer review completed by @backend agent

## 📊 Estimated Complexity

Medium (M) - 2-3 days

## 🏷️ Epic

Epic: Infrastructure & Platform Setup

## 👥 Assigned Agents

Primary: @backend
Secondary: @devops (for Dockerfile configuration)

## 📌 Sprint/Milestone

Sprint 0 - Foundation / MVP Release

## 📚 Related Documentation

- [ADR-009: Modular Monolith with Vertical Slices and DDD](docs/architecture/decisions/ADR-009-modular-monolith-vertical-slices.md)
- [ADR-008: Code-First Database Migrations](docs/architecture/decisions/ADR-008-database-migrations-strategy.md)
- [ADR-005: CQRS Implementation](docs/architecture/decisions/ADR-005-cqrs-implementation.md)
- [ARCHITECTURE.md - Project Structure](docs/architecture/ARCHITECTURE.md#project-structure)

---

**Labels**: feature, priority:P0, complexity:M, epic:infrastructure, agent:backend, Sprint 0
