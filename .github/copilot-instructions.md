# Copilot Instructions - Beer Competition SaaS Platform

**Project**: Multi-tenant SaaS platform for managing BJCP-compliant homebrew beer competitions  
**Status**: Active Development (MVP Phase)  
**Last Updated**: 2025-12-19

---

## Project Overview

You are working on a **Beer Competition SaaS Platform** that enables competition organizers to manage BJCP 2021-compliant homebrew competitions with:
- Blind judging with conflict-of-interest enforcement
- Offline scoresheet entry for judges (PWA)
- Multi-tenant data isolation (PostgreSQL RLS)
- Event-driven microservices architecture
- Support for 200+ entrants, 50+ concurrent judges, 600+ bottles per competition

---

## Architecture Quick Reference

**ALWAYS review these ADRs before implementing features:**

### Core Decisions
1. **[ADR-001: Tech Stack](docs/architecture/decisions/ADR-001-tech-stack-selection.md)**
   - Backend: .NET 10 (Competition Service, Judging Service, BFF)
   - Frontend: React 18 + TypeScript + PWA
   - Database: PostgreSQL 16+ with Row-Level Security
   - Message Bus: RabbitMQ with CloudEvents format
   - Analytics: Python 3.12 + FastAPI (Post-MVP)

2. **[ADR-002: Multi-Tenancy](docs/architecture/decisions/ADR-002-multi-tenancy-strategy.md)**
   - Every table has `tenant_id` column
   - PostgreSQL Row-Level Security (RLS) enforces isolation
   - Entity Framework Global Filters auto-inject tenant predicate
   - BFF extracts `tenant_id` from JWT and passes via `X-Tenant-ID` header

3. **[ADR-003: Event-Driven Architecture](docs/architecture/decisions/ADR-003-event-driven-architecture.md)**
   - Outbox Pattern: Events stored in DB, background worker publishes to RabbitMQ
   - CloudEvents 1.0 format for all events
   - At-least-once delivery with idempotency
   - Dead Letter Queue for failed messages

4. **[ADR-004: Authentication](docs/architecture/decisions/ADR-004-authentication-authorization.md)**
   - Keycloak OIDC provider
   - BFF (Backend-for-Frontend) pattern for token validation
   - JWT with `tenant_id` + `roles` claims
   - Roles: Organizer, Judge, Entrant, Steward

5. **[ADR-005: CQRS](docs/architecture/decisions/ADR-005-cqrs-implementation.md)**
   - MediatR for command/query separation
   - Single PostgreSQL database (shared schema)
   - Pipeline behaviors for validation (FluentValidation), logging

6. **[ADR-006: Testing](docs/architecture/decisions/ADR-006-testing-strategy.md)**
   - Test Pyramid: 70% unit, 20% integration, 10% E2E
   - Testcontainers for real PostgreSQL/RabbitMQ in tests
   - xUnit + FluentAssertions (.NET), Cypress (E2E)

7. **[ADR-007: Frontend](docs/architecture/decisions/ADR-007-frontend-architecture.md)**
   - React 18 + TypeScript + Vite
   - Progressive Web App (PWA) with offline support
   - TanStack Query (server state), Zustand (client state)
   - IndexedDB (Dexie.js) for offline scoresheet storage

---

## CRITICAL Implementation Rules

### 1. Multi-Tenancy (MANDATORY)
**NEVER query/write data without tenant isolation:**

```csharp
// ✅ CORRECT: Entity Framework Global Filter auto-applies
var competitions = await _dbContext.Competitions
    .Where(c => c.Status == Status.Active)
    .ToListAsync();

// ❌ WRONG: Never bypass filters without explicit justification
var allData = await _dbContext.Competitions
    .IgnoreQueryFilters()  // DANGEROUS!
    .ToListAsync();

// ✅ CORRECT: Always include tenant_id in inserts
var competition = new Competition
{
    TenantId = _tenantProvider.CurrentTenantId,  // REQUIRED
    Name = "Spring Classic 2025"
};
```

**PostgreSQL RLS Validation:**
```sql
-- ALWAYS enable RLS on new tables
ALTER TABLE new_table ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation ON new_table
    USING (tenant_id = current_setting('app.current_tenant')::uuid);
```

### 2. Event-Driven Patterns (REQUIRED)
**NEVER publish events directly to RabbitMQ from handlers:**

```csharp
// ✅ CORRECT: Outbox Pattern (atomic with DB transaction)
public async Task<Result<Guid>> Handle(CreateEntryCommand cmd, CancellationToken ct)
{
    var entry = Entry.Create(...);
    await _dbContext.Entries.AddAsync(entry, ct);
    
    // Store event in Outbox (same transaction)
    await _dbContext.EventStore.AddAsync(new EventStoreEntry
    {
        EventType = "entry.submitted",
        EventData = JsonSerializer.Serialize(cloudEvent),
        PublishedStatus = "PENDING"
    }, ct);
    
    await _dbContext.SaveChangesAsync(ct);  // Atomic
    return Result.Success(entry.Id);
}

// ❌ WRONG: Direct publish (dual-write problem)
await _rabbitMQ.PublishAsync("entry.submitted", entry);  // NOT ATOMIC!
```

**CloudEvents Format:**
```json
{
  "specversion": "1.0",
  "type": "com.beercomp.entry.submitted",
  "source": "/services/competition",
  "id": "uuid",
  "time": "2025-12-19T10:00:00Z",
  "datacontenttype": "application/json",
  "data": {
    "tenant_id": "uuid",
    "competition_id": "uuid",
    "entry_id": "uuid",
    "judging_number": 42
  }
}
```

### 3. CQRS Pattern (REQUIRED)
**ALWAYS use MediatR for business logic:**

```csharp
// ✅ CORRECT: Command Handler
public record CreateCompetitionCommand(
    string Name,
    DateTime RegistrationDeadline
) : IRequest<Result<Guid>>;

public class CreateCompetitionHandler : IRequestHandler<CreateCompetitionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCompetitionCommand cmd, CancellationToken ct)
    {
        // 1. Validate
        if (cmd.RegistrationDeadline < DateTime.UtcNow)
            return Result.Failure<Guid>("Deadline must be in future");
        
        // 2. Create entity
        var competition = Competition.Create(cmd.Name, cmd.RegistrationDeadline);
        
        // 3. Persist + event
        await _dbContext.Competitions.AddAsync(competition, ct);
        await _dbContext.EventStore.AddAsync(...);
        await _dbContext.SaveChangesAsync(ct);
        
        return Result.Success(competition.Id);
    }
}

// ✅ CORRECT: Controller usage
[HttpPost]
public async Task<IActionResult> CreateCompetition([FromBody] CreateCompetitionRequest request)
{
    var command = new CreateCompetitionCommand(request.Name, request.RegistrationDeadline);
    var result = await _mediator.Send(command);
    return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
}

// ❌ WRONG: Business logic in controller
[HttpPost]
public async Task<IActionResult> CreateCompetition([FromBody] CreateCompetitionRequest request)
{
    var competition = new Competition { Name = request.Name };
    _dbContext.Competitions.Add(competition);
    await _dbContext.SaveChangesAsync();
    return Ok(competition.Id);  // NO validation, NO events!
}
```

### 4. Authentication (REQUIRED)
**ALWAYS enforce authorization on endpoints:**

```csharp
// ✅ CORRECT: Role-based authorization
[HttpPost]
[Authorize(Policy = "OrganizerOnly")]
public async Task<IActionResult> CreateCompetition(...)
{
    // Only organizers can create competitions
}

// ✅ CORRECT: Extract tenant from claims
var tenantId = User.FindFirstValue("tenant_id");
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

// ❌ WRONG: No authorization
[HttpPost]
public async Task<IActionResult> CreateCompetition(...)  // Anyone can call!
```

### 5. Offline PWA (Frontend - CRITICAL)
**ALWAYS support offline for judge workflows:**

```typescript
// ✅ CORRECT: Offline-first scoresheet submission
const submitScoresheet = useMutation({
  mutationFn: async (scoresheet: Scoresheet) => {
    if (navigator.onLine) {
      return apiClient.post('/api/scoresheets', scoresheet);
    } else {
      // Save to IndexedDB, sync later
      return db.scoresheets.add({ ...scoresheet, syncStatus: 'pending' });
    }
  }
});

// ❌ WRONG: Network-only (fails offline)
const submitScoresheet = async (scoresheet: Scoresheet) => {
  await fetch('/api/scoresheets', { method: 'POST', body: JSON.stringify(scoresheet) });
};
```

---

## Coding Standards

### .NET Backend
```csharp
// ✅ Use Result<T> pattern for errors (no exceptions for business logic)
public async Task<Result<Guid>> Handle(...)
{
    if (invalid) return Result.Failure<Guid>("Error message");
    return Result.Success(value);
}

// ✅ Use records for DTOs and commands
public record CreateCompetitionCommand(string Name, DateTime Deadline) : IRequest<Result<Guid>>;

// ✅ Use FluentValidation
public class CreateCompetitionValidator : AbstractValidator<CreateCompetitionCommand>
{
    public CreateCompetitionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}

// ✅ Use AsNoTracking for queries
var competitions = await _dbContext.Competitions
    .AsNoTracking()
    .ToListAsync();
```

### React Frontend
```typescript
// ✅ Use TypeScript strict mode
export interface Competition {
  id: string;
  name: string;
  registrationDeadline: Date;
}

// ✅ Use TanStack Query for server state
const { data, isLoading } = useQuery({
  queryKey: ['competitions'],
  queryFn: fetchCompetitions
});

// ✅ Use Zustand for client state
const useAuthStore = create<AuthState>((set) => ({
  user: null,
  login: (user) => set({ user })
}));

// ✅ Use Tailwind CSS
<button className="px-4 py-2 bg-amber-600 text-white rounded-lg hover:bg-amber-700">
  Submit
</button>
```

---

## Testing Requirements

**EVERY feature must have:**
1. **Unit Tests**: Business logic, handlers, validators (70% coverage)
2. **Integration Tests**: API endpoints + database (Testcontainers)
3. **E2E Tests**: Critical user flows (Cypress)

```csharp
// ✅ CORRECT: Integration test with Testcontainers
[Fact]
public async Task CreateCompetition_ValidData_PersistsToDatabase()
{
    // Arrange: Uses real PostgreSQL container
    var handler = new CreateCompetitionHandler(_dbContext, _tenantProvider);
    var command = new CreateCompetitionCommand("Spring Classic", DateTime.UtcNow.AddDays(30));
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    var competition = await _dbContext.Competitions.FindAsync(result.Value);
    competition.Should().NotBeNull();
}
```

---

## Project Structure

```
beer-competition-saas/
├── backend/
│   ├── Competition.Service/        # Competitions, entries, payments
│   ├── Judging.Service/            # Flights, scoresheets, Best of Show
│   ├── BFF.ApiGateway/             # Token validation, routing
│   └── Shared/                     # Common models, Result<T>
├── frontend/
│   ├── src/
│   │   ├── pages/                  # Route components
│   │   ├── components/             # Reusable UI
│   │   ├── hooks/                  # Custom hooks
│   │   ├── store/                  # Zustand stores
│   │   └── db/                     # IndexedDB schema
├── infrastructure/
│   └── docker-compose.yml          # PostgreSQL, RabbitMQ, Keycloak
└── docs/
    ├── architecture/
    │   ├── ARCHITECTURE.md
    │   └── decisions/              # ADRs
    └── BACKLOG.md
```

---

## Common Pitfalls to Avoid

❌ **NEVER forget tenant_id** in queries/inserts  
❌ **NEVER bypass Entity Framework query filters** without review  
❌ **NEVER publish events directly** (use Outbox Pattern)  
❌ **NEVER put business logic in controllers** (use MediatR handlers)  
❌ **NEVER skip authorization** on API endpoints  
❌ **NEVER assume network availability** (PWA must work offline)  
❌ **NEVER commit secrets** (use environment variables)  

---

## Before Starting Work

1. **Read the relevant ADR** from `docs/architecture/decisions/`
2. **Check existing patterns** in codebase
3. **Verify multi-tenancy** is enforced
4. **Write tests first** (TDD approach)
5. **Run integration tests** with Testcontainers before PR

---

## Getting Help

- **Architecture questions** → Review `docs/architecture/ARCHITECTURE.md`
- **Decisions context** → Read ADRs in `docs/architecture/decisions/`
- **Feature planning** → Check `docs/BACKLOG.md`
- **Agent-specific help** → Use specialized agents (@backend, @frontend, etc.)

---

## Success Criteria

✅ All code follows multi-tenancy rules (tenant_id always present)  
✅ Events published via Outbox Pattern (no dual-write)  
✅ CQRS pattern used (MediatR handlers)  
✅ Authorization enforced on all endpoints  
✅ PWA works offline (service workers + IndexedDB)  
✅ Tests pass (unit + integration + E2E)  
✅ No secrets committed to repo  

**When in doubt, ask the specialized agent or review the ADRs!**
