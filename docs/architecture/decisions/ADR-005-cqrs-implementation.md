# ADR-005: CQRS Pattern Implementation

**Date**: 2025-12-19  
**Status**: Accepted  
**Deciders**: Architecture Team  
**Context**: Beer Competition SaaS Platform

---

## Context and Problem Statement

Our application has distinct read and write patterns:

**Write Operations (Commands):**
- Create competition, submit entry, assign judges to flights, submit scoresheet
- Business rule validation, consistency checks, transaction boundaries
- Low frequency (hundreds per hour)
- Must be consistent and durable

**Read Operations (Queries):**
- List competitions, view flight assignments, display leaderboards, generate reports
- No side effects, data aggregation, filtering, sorting
- High frequency (thousands per hour)
- Can tolerate eventual consistency

**Challenges with Traditional CRUD:**
- Single model for reads and writes creates complexity
- Tight coupling between query and command logic
- Difficult to optimize separately (indexes for reads vs writes)
- Hard to scale (reads often 10x more frequent than writes)

How do we **separate read and write concerns** while maintaining:
- **Clean Architecture**: Clear separation of responsibilities
- **Performance**: Optimize queries independently of writes
- **Maintainability**: Simple mental model for developers
- **Testability**: Easy to unit test commands and queries separately

---

## Decision Drivers

- **Separation of Concerns**: Decouple reads from writes
- **Performance**: Optimize queries without impacting command performance
- **Scalability**: Scale read and write layers independently
- **Developer Experience**: Simple, consistent pattern
- **Testability**: Easy to unit test handlers
- **Event Sourcing Future**: Potential to add event sourcing later
- **Complexity**: Avoid over-engineering for MVP

---

## Considered Options

### 1. Traditional CRUD (No CQRS)
**Approach**: Single model for reads and writes, controllers call repositories directly

**Pros:**
- ✅ Simple, well-understood pattern
- ✅ No additional libraries

**Cons:**
- ❌ **Tight coupling**: Query logic mixed with command logic
- ❌ **Hard to optimize**: Single model for both reads and writes
- ❌ **Scalability**: Can't scale reads independently
- ❌ **Complexity grows**: Large models with many concerns

---

### 2. CQRS with Separate Databases (Write DB + Read DB)
**Approach**: Commands write to PostgreSQL, queries read from separate read-optimized store (e.g., Elasticsearch)

**Pros:**
- ✅ **Extreme scalability**: Read and write DBs scaled independently
- ✅ **Optimized models**: Write DB normalized, read DB denormalized

**Cons:**
- ❌ **Operational complexity**: Two databases to manage
- ❌ **Eventual consistency**: Read DB lags behind write DB
- ❌ **Synchronization overhead**: Events must update read DB
- ❌ **Cost**: Double storage costs

---

### 3. CQRS with Single Database (Shared PostgreSQL)
**Approach**: Commands and queries use same PostgreSQL database, separated by MediatR pattern

**Pros:**
- ✅ **Simplified operations**: Single database
- ✅ **Strong consistency**: Queries see committed data immediately
- ✅ **Clean separation**: Commands and queries have distinct handlers
- ✅ **Optimizable**: Can add read replicas later if needed

**Cons:**
- ❌ **Shared infrastructure**: Can't scale reads/writes independently (without read replicas)
- ❌ **Indexing trade-offs**: Must balance read and write index overhead

---

### 4. CQRS with Event Sourcing
**Approach**: Commands append events to event store, queries built from event replay

**Pros:**
- ✅ Complete audit trail
- ✅ Temporal queries (state at any point in time)

**Cons:**
- ❌ **Massive complexity**: Event versioning, snapshots, replays
- ❌ **Learning curve**: Steep for team
- ❌ **Overkill for MVP**: Not needed initially

---

## Decision Outcome

**Chosen Option**: **#3 - CQRS with Single Database (PostgreSQL) using MediatR**

---

## Implementation Details

### 1. MediatR Library

**Installation:**
```bash
dotnet add package MediatR
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection
```

**Startup Configuration:**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register MediatR handlers from assembly
    services.AddMediatR(cfg => 
        cfg.RegisterServicesFromAssembly(typeof(Startup).Assembly));
    
    // Register behaviors (cross-cutting concerns)
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
}
```

---

### 2. Command Pattern

#### Command Definition
```csharp
// Command: Represents a write operation with intent
public record CreateCompetitionCommand(
    string Name,
    DateTime RegistrationDeadline,
    DateTime JudgingStartDate,
    string StylesSupported
) : IRequest<Result<Guid>>;  // Returns competition ID or error
```

#### Command Handler
```csharp
public class CreateCompetitionHandler 
    : IRequestHandler<CreateCompetitionCommand, Result<Guid>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CreateCompetitionHandler> _logger;

    public CreateCompetitionHandler(
        ApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        ILogger<CreateCompetitionHandler> logger)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateCompetitionCommand cmd, 
        CancellationToken ct)
    {
        // 1. Business rule validation
        if (cmd.RegistrationDeadline >= cmd.JudgingStartDate)
        {
            return Result.Failure<Guid>("Registration must close before judging starts");
        }

        // 2. Create domain entity (rich domain model)
        var competition = Competition.Create(
            tenantId: _tenantProvider.CurrentTenantId,
            name: cmd.Name,
            registrationDeadline: cmd.RegistrationDeadline,
            judgingStartDate: cmd.JudgingStartDate,
            stylesSupported: cmd.StylesSupported);

        // 3. Persist to database
        await _dbContext.Competitions.AddAsync(competition, ct);

        // 4. Store event in Outbox (for event-driven architecture)
        await _dbContext.EventStore.AddAsync(new EventStoreEntry
        {
            TenantId = competition.TenantId,
            AggregateId = competition.Id,
            EventType = "competition.created",
            EventData = JsonSerializer.Serialize(new CompetitionCreatedEvent
            {
                CompetitionId = competition.Id,
                Name = competition.Name,
                TenantId = competition.TenantId
            })
        }, ct);

        // 5. Commit transaction (atomic: entity + event)
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Competition {CompetitionId} created by tenant {TenantId}", 
            competition.Id, 
            competition.TenantId);

        return Result.Success(competition.Id);
    }
}
```

#### Controller Usage
```csharp
[ApiController]
[Route("api/competitions")]
public class CompetitionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompetitionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Policy = "OrganizerOnly")]
    public async Task<IActionResult> CreateCompetition(
        [FromBody] CreateCompetitionRequest request)
    {
        var command = new CreateCompetitionCommand(
            Name: request.Name,
            RegistrationDeadline: request.RegistrationDeadline,
            JudgingStartDate: request.JudgingStartDate,
            StylesSupported: request.StylesSupported);

        var result = await _mediator.Send(command);

        return result.IsSuccess 
            ? CreatedAtAction(nameof(GetCompetition), new { id = result.Value }, result.Value)
            : BadRequest(new { error = result.Error });
    }
}
```

---

### 3. Query Pattern

#### Query Definition
```csharp
// Query: Represents a read operation with filters
public record GetCompetitionQuery(
    Guid CompetitionId
) : IRequest<Result<CompetitionDto>>;  // Returns DTO or error
```

#### Query Handler
```csharp
public class GetCompetitionHandler 
    : IRequestHandler<GetCompetitionQuery, Result<CompetitionDto>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public async Task<Result<CompetitionDto>> Handle(
        GetCompetitionQuery query, 
        CancellationToken ct)
    {
        // 1. Query database (read-optimized projection)
        var competition = await _dbContext.Competitions
            .AsNoTracking()  // Read-only, no change tracking overhead
            .Where(c => c.Id == query.CompetitionId)
            .Where(c => c.TenantId == _tenantProvider.CurrentTenantId)  // Tenant isolation
            .Select(c => new CompetitionDto  // Project to DTO (only needed fields)
            {
                Id = c.Id,
                Name = c.Name,
                RegistrationDeadline = c.RegistrationDeadline,
                JudgingStartDate = c.JudgingStartDate,
                Status = c.Status,
                EntryCount = c.Entries.Count,  // Aggregate data
                JudgeCount = c.Flights
                    .SelectMany(f => f.JudgesFlights)
                    .Select(jf => jf.JudgeId)
                    .Distinct()
                    .Count()
            })
            .FirstOrDefaultAsync(ct);

        return competition != null
            ? Result.Success(competition)
            : Result.Failure<CompetitionDto>("Competition not found");
    }
}
```

#### Controller Usage
```csharp
[HttpGet("{id}")]
[Authorize]
public async Task<IActionResult> GetCompetition(Guid id)
{
    var query = new GetCompetitionQuery(CompetitionId: id);
    var result = await _mediator.Send(query);

    return result.IsSuccess 
        ? Ok(result.Value)
        : NotFound(new { error = result.Error });
}
```

---

### 4. Complex Query Example (Leaderboard)

```csharp
public record GetLeaderboardQuery(
    Guid CompetitionId,
    string? StyleId,  // Optional filter
    int Page,
    int PageSize
) : IRequest<Result<PagedResult<LeaderboardEntryDto>>>;

public class GetLeaderboardHandler 
    : IRequestHandler<GetLeaderboardQuery, Result<PagedResult<LeaderboardEntryDto>>>
{
    public async Task<Result<PagedResult<LeaderboardEntryDto>>> Handle(
        GetLeaderboardQuery query, 
        CancellationToken ct)
    {
        var baseQuery = _dbContext.Entries
            .AsNoTracking()
            .Where(e => e.CompetitionId == query.CompetitionId)
            .Where(e => e.Status == EntryStatus.Scored);

        // Optional filter
        if (query.StyleId != null)
        {
            baseQuery = baseQuery.Where(e => e.StyleId == query.StyleId);
        }

        var totalCount = await baseQuery.CountAsync(ct);

        // Pagination + sorting + projection
        var entries = await baseQuery
            .OrderByDescending(e => e.FinalScore)  // Highest score first
            .ThenBy(e => e.JudgingNumber)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new LeaderboardEntryDto
            {
                JudgingNumber = e.JudgingNumber,
                EntryName = e.EntryName,
                StyleId = e.StyleId,
                StyleName = e.Style.Name,
                FinalScore = e.FinalScore,
                Placement = 0  // Calculated below
            })
            .ToListAsync(ct);

        // Calculate placements (1st, 2nd, 3rd within style)
        var placement = (query.Page - 1) * query.PageSize + 1;
        foreach (var entry in entries)
        {
            entry.Placement = placement++;
        }

        return Result.Success(new PagedResult<LeaderboardEntryDto>
        {
            Items = entries,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }
}
```

---

### 5. Pipeline Behaviors (Cross-Cutting Concerns)

#### Validation Behavior (FluentValidation)
```csharp
public class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken ct)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}

// Validator example
public class CreateCompetitionValidator : AbstractValidator<CreateCompetitionCommand>
{
    public CreateCompetitionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.RegistrationDeadline)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Registration deadline must be in the future");

        RuleFor(x => x.JudgingStartDate)
            .GreaterThan(x => x.RegistrationDeadline)
            .WithMessage("Judging must start after registration closes");
    }
}
```

#### Logging Behavior
```csharp
public class LoggingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}: {@Request}", requestName, request);

        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        _logger.LogInformation(
            "Handled {RequestName} in {ElapsedMs}ms", 
            requestName, 
            stopwatch.ElapsedMilliseconds);

        return response;
    }
}
```

---

## Design Patterns

### Command Structure
```
Commands/
├── CreateCompetitionCommand.cs
├── CreateCompetitionHandler.cs
├── CreateCompetitionValidator.cs
├── SubmitEntryCommand.cs
├── SubmitEntryHandler.cs
└── SubmitEntryValidator.cs
```

### Query Structure
```
Queries/
├── GetCompetitionQuery.cs
├── GetCompetitionHandler.cs
├── GetLeaderboardQuery.cs
├── GetLeaderboardHandler.cs
└── DTOs/
    ├── CompetitionDto.cs
    └── LeaderboardEntryDto.cs
```

---

## Consequences

### Positive
✅ **Separation of Concerns**: Commands and queries have distinct responsibilities  
✅ **Testability**: Easy to unit test handlers in isolation  
✅ **Performance**: Queries optimized independently (AsNoTracking, projections)  
✅ **Scalability**: Can add read replicas later without changing code  
✅ **Consistency**: Single database ensures immediate consistency  
✅ **Pipeline Behaviors**: Validation, logging, transactions as cross-cutting concerns  
✅ **Developer Experience**: Consistent pattern across all features  

### Negative
❌ **Learning Curve**: Team must learn MediatR + CQRS patterns  
❌ **Boilerplate**: More files (command + handler + validator) vs traditional CRUD  
❌ **Shared Database**: Can't scale reads/writes independently (without read replicas)  
❌ **Eventual Consistency**: If we add read replicas, queries may lag  

### Risks
⚠️ **Over-Engineering**: CQRS adds complexity for simple CRUD (acceptable trade-off)  
⚠️ **Consistency Confusion**: If we later add separate read DB, eventual consistency may surprise developers  
⚠️ **Testing**: Must mock IMediator in integration tests  

---

## Alternatives Considered

### Why not traditional CRUD?
- ❌ **Tight coupling**: Query logic mixed with command logic
- ❌ **Hard to optimize**: Single model for both reads and writes
- ❌ **Scalability**: Can't scale reads independently

### Why not CQRS with separate read DB?
- ❌ **Premature optimization**: Adds complexity without proven need
- ❌ **Eventual consistency**: MVP needs strong consistency
- ❌ **Operational overhead**: Two databases to manage

### Why not Event Sourcing?
- ❌ **Massive complexity**: Event versioning, snapshots, replays
- ❌ **Learning curve**: Too steep for team and MVP timeline
- ❌ **Overkill**: Audit trail can be achieved with event store table

---

## Future Enhancements

### Read Replicas (When Needed)
```csharp
// Command: Write to primary
services.AddDbContext<WriteDbContext>(options =>
    options.UseNpgsql(Configuration.GetConnectionString("Primary")));

// Query: Read from replica
services.AddDbContext<ReadDbContext>(options =>
    options.UseNpgsql(Configuration.GetConnectionString("ReadReplica")));
```

### Caching Layer (Redis)
```csharp
public class GetCompetitionHandler : IRequestHandler<GetCompetitionQuery, Result<CompetitionDto>>
{
    private readonly IDistributedCache _cache;

    public async Task<Result<CompetitionDto>> Handle(...)
    {
        var cacheKey = $"competition:{query.CompetitionId}";
        var cachedValue = await _cache.GetStringAsync(cacheKey);
        
        if (cachedValue != null)
        {
            return Result.Success(JsonSerializer.Deserialize<CompetitionDto>(cachedValue));
        }

        var competition = await _dbContext.Competitions...
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(competition));
        
        return Result.Success(competition);
    }
}
```

---

## Testing Strategy

### Unit Tests (Commands)
```csharp
[Fact]
public async Task CreateCompetition_ValidData_ReturnsCompetitionId()
{
    // Arrange
    var command = new CreateCompetitionCommand(
        Name: "Spring Classic 2025",
        RegistrationDeadline: DateTime.UtcNow.AddDays(30),
        JudgingStartDate: DateTime.UtcNow.AddDays(35),
        StylesSupported: "IPA,Stout");
    
    var handler = new CreateCompetitionHandler(_dbContext, _tenantProvider, _logger);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
    
    var competition = await _dbContext.Competitions.FindAsync(result.Value);
    competition.Should().NotBeNull();
    competition.Name.Should().Be("Spring Classic 2025");
}
```

### Unit Tests (Queries)
```csharp
[Fact]
public async Task GetCompetition_ExistingId_ReturnsDto()
{
    // Arrange
    var competitionId = await SeedCompetitionAsync();
    var query = new GetCompetitionQuery(CompetitionId: competitionId);
    var handler = new GetCompetitionHandler(_dbContext, _tenantProvider);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Name.Should().Be("Spring Classic 2025");
}
```

---

## Related Decisions

- [ADR-001: Tech Stack Selection](ADR-001-tech-stack-selection.md) (MediatR in .NET)
- [ADR-003: Event-Driven Architecture](ADR-003-event-driven-architecture.md) (Commands publish events)

---

## References

- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [CQRS Pattern (Martin Fowler)](https://martinfowler.com/bliki/CQRS.html)
- [Microsoft CQRS Guidance](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [FluentValidation](https://docs.fluentvalidation.net/)
