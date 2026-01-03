# Copilot Instructions - Beer Competition SaaS Platform

**Project**: Multi-tenant SaaS platform for managing BJCP-compliant homebrew beer competitions  
**Status**: Active Development (MVP Phase)  
**Last Updated**: 2025-12-25

---

## Project Overview

You are working on a **Beer Competition SaaS Platform** that enables competition organizers to manage BJCP 2021-compliant homebrew competitions with:
- Blind judging with conflict-of-interest enforcement
- Offline scoresheet entry for judges (PWA)
- Multi-tenant data isolation (PostgreSQL RLS)
- Event-driven microservices architecture
- Service-specific audiences with OAuth 2.0 token exchange (zero-trust security)
- Support for 200+ entrants, 50+ concurrent judges, 600+ bottles per competition

---

## Language Requirements

**CRITICAL**: All project documentation, code comments, commit messages, Pull Requests, Issues, ADRs, and README files **MUST be written in English**.

This includes:
- ✅ **Commit messages**: `feat: implement entry submission API (#16)`
- ✅ **Pull Request titles and descriptions**: All in English
- ✅ **Issue titles and descriptions**: All in English
- ✅ **Code comments**: `// Calculate consensus score from all judges`
- ✅ **Documentation files**: README.md, ADRs, guides, etc.
- ✅ **API documentation**: OpenAPI/Swagger specs
- ✅ **Error messages**: Application error messages in English
- ✅ **Log messages**: All structured logs in English

**Rationale**: English is the international language for software development, enabling:
- Global collaboration and code reviews
- Better AI/Copilot assistance
- Easier onboarding for international developers
- Industry-standard documentation practices

**Exception**: User-facing UI text may be localized (future internationalization).

---

## Security and Sensitive Information

**CRITICAL**: Never include passwords, API keys, secrets, or any sensitive information in:
- ❌ **Issues or Pull Requests**: Titles, descriptions, or comments
- ❌ **Code comments or documentation**: README, ADRs, guides
- ❌ **Configuration files**: Committed to repository
- ❌ **Commit messages**: Any part of the commit history
- ❌ **Example code or snippets**: In documentation or comments

**Use placeholders instead**:
- ✅ Passwords: `<your-password>`, `<secure-password>`, `your_password_here`
- ✅ API Keys: `<your-api-key>`, `<api-key-here>`
- ✅ Secrets: `<your-secret>`, `<client-secret>`
- ✅ Connection strings: Mask passwords with `***` or placeholders
- ✅ Environment variables: Reference variable name, not actual value

**Examples**:
```bash
# ❌ WRONG
POSTGRES_PASSWORD=SuperSecret123!

# ✅ CORRECT
POSTGRES_PASSWORD=<your-password>
```

```yaml
# ❌ WRONG
Login: admin@example.com / password123

# ✅ CORRECT
Login: admin@example.com / <your-password>
```

**Exception**: 
- `.env` files (git-ignored) may contain development credentials
- Docker Compose default values for **local development only** are acceptable with clear warnings
- Production secrets **must** use Azure Key Vault or similar secret management

**Rationale**: Prevents accidental exposure of credentials in public repositories and commit history.

---

## MANDATORY Workflow: Starting New Issues

**TEAM-BASED APPROACH**: When starting work on a GitHub issue, we follow a coordinated workflow where the **Product Owner Agent** acts as the workflow coordinator, setting up the issue and delegating to specialized agents.

### Workflow Overview

```
User Request: "I want to start issue #100"
       ↓
Product Owner Agent (@product-owner)
       ↓
   [PHASE 1: Setup]
   1. Read issue details
   2. Create feature branch: {issue-number}-{description}
   3. Push branch to GitHub immediately
   4. Update issue status → "In Progress"
   5. Identify assigned agent from labels
       ↓
   [PHASE 2: Delegate]
   Hand off to specialized agent:
   - @backend for API/database work
   - @frontend for UI/UX work  
   - @devops for infrastructure
   - @qa for testing strategy
       ↓
Specialized Agent (e.g., @backend)
       ↓
   [PHASE 3: Implementation]
   - Implement feature
   - Write tests
   - Commit with convention: "feat: description (#N)"
   - Create PR: "Closes #N"
       ↓
   [PHASE 4: Review]
   Specialized Agent notifies @product-owner
       ↓
Product Owner Agent
       ↓
   - Update issue status → "In Review"
       ↓
   [PHASE 5: Complete]
   PR merged → Status auto-updates to "Done"
```

### How to Start an Issue

**User says**: "I want to start working on issue #100"

**Copilot invokes**: `@product-owner` agent

**Product Owner**:
1. Reads issue #100 using `mcp_io_github_git_issue_read`
2. Creates branch: `100-short-description` from main
3. Pushes branch: `git push -u origin 100-short-description`
4. Updates status to "In Progress" using GraphQL API
5. Identifies agent from issue labels (e.g., `agent:devops`)
6. Delegates: "@devops please implement issue #100"

**Specialized Agent** (e.g., @devops):
- Implements the feature
- Creates PR with `Closes #100`
- Notifies: "@product-owner PR created for #100"

**Product Owner**:
- Updates status to "In Review"

**After PR merge**:
- Status automatically moves to "Done"

### Step-by-Step Automated Process

#### Step 1: Read Issue Details
```typescript
// Use GitHub MCP to get issue details
await mcp_io_github_git_issue_read({
  method: "get",
  owner: "jesuscorral",
  repo: "beer-competition-saas",
  issue_number: 16
});
```

#### Step 2: Create Feature Branch
```bash
# Create branch using naming convention: {issue-number}-{short-description}
# Example for issue #16: "API endpoint for entry submission"
git checkout main
git pull origin main
git checkout -b 16-entry-submission-api

# Other examples:
# Issue #23: "Create Flight entity and repository" → 23-flight-entity-repository
# Issue #45: "Add scoresheet offline storage" → 45-scoresheet-offline-storage
```

**Branch Naming Convention**:
- **Format**: `{issue-number}-{short-description}`
- Use issue number from GitHub (e.g., #16, #23, #45)
- Use lowercase with hyphens for description
- Keep description short (3-5 words max)
- Focus on WHAT, not HOW

#### Step 3: Push Branch to GitHub (IMMEDIATE)
```bash
# CRITICAL: Push empty branch immediately to publish it on GitHub
git push -u origin 16-entry-submission-api
```

**Why push immediately?**
- Makes branch visible in GitHub UI
- Enables collaboration
- Allows status tracking
- Prevents local-only work

#### Step 4: Update Issue Status to "In Progress"

**Project Configuration**:
- Project ID: `PVT_kwHOAFw6AM4BK9-n` (Beer competition #9)
- Status Field ID: `PVTSSF_lAHOAFw6AM4BK9-nzg6rqRo`
- Owner: `jesuscorral`
- Repo: `beer-competition-saas`

**Status Option IDs**:
- Backlog: `f75ad846`
- In Progress: `47fc9ee4` ⬅️ Use this
- In Review: `df73e18b`
- Done: `98236657`

**Update using GraphQL API**:
```bash
# PowerShell (Windows)
# Step 1: Get Project Item ID
$query = @"
query {
  node(id: "PVT_kwHOAFw6AM4BK9-n") {
    ... on ProjectV2 {
      items(first: 100) {
        nodes {
          id
          content {
            ... on Issue {
              number
            }
          }
        }
      }
    }
  }
}
"@

$response = gh api graphql -f query=$query | ConvertFrom-Json
$itemId = ($response.data.node.items.nodes | Where-Object { $_.content.number -eq 16 }).id

# Step 2: Update status to "In Progress"
$mutation = @"
mutation {
  updateProjectV2ItemFieldValue(
    input: {
      projectId: "PVT_kwHOAFw6AM4BK9-n"
      itemId: "$itemId"
      fieldId: "PVTSSF_lAHOAFw6AM4BK9-nzg6rqRo"
      value: { 
        singleSelectOptionId: "47fc9ee4"
      }
    }
  ) {
    projectV2Item { id }
  }
}
"@

gh api graphql -f query=$mutation
```

#### Step 5: Implement Feature with Proper Commits
```bash
# ALWAYS reference issue number in commits
# ALWAYS write commit messages in ENGLISH
git commit -m "feat: implement entry submission API (#16)"
git commit -m "fix: resolve tenant isolation bug (#23)"
git commit -m "docs: update API documentation (#45)"

# Use conventional commit prefixes:
# feat: - New feature
# fix: - Bug fix
# docs: - Documentation only
# test: - Adding tests
# refactor: - Code refactoring
# chore: - Build/tooling changes
# security: - Security improvements

# ❌ WRONG (Spanish)
git commit -m "feat: implementar API de envío de entradas (#16)"

# ✅ CORRECT (English)
git commit -m "feat: implement entry submission API (#16)"
```

#### Step 6: Create Pull Request
```bash
# ALWAYS write PR title and body in ENGLISH
# ALWAYS link to the issue with "Closes #N"
gh pr create --title "feat: implement entry submission API (#16)" \
             --body "## Summary

Complete implementation of entry submission API endpoint.

## Changes
- Added POST /api/entries endpoint
- Implemented validation with FluentValidation
- Added unit and integration tests

## Testing
- ✅ Unit tests passing
- ✅ Integration tests with Testcontainers
- ✅ Manual testing completed

Closes #16" \
             --base main
```

#### Step 7: Update Issue Status to "In Review"
```bash
# PowerShell: Update to "In Review" after PR creation
$response = gh api graphql -f query=$query | ConvertFrom-Json
$itemId = ($response.data.node.items.nodes | Where-Object { $_.content.number -eq 16 }).id

$mutation = @"
mutation {
  updateProjectV2ItemFieldValue(
    input: {
      projectId: "PVT_kwHOAFw6AM4BK9-n"
      itemId: "$itemId"
      fieldId: "PVTSSF_lAHOAFw6AM4BK9-nzg6rqRo"
      value: { 
        singleSelectOptionId: "df73e18b"
      }
    }
  ) {
    projectV2Item { id }
  }
}
"@

gh api graphql -f query=$mutation
```

#### Step 8: PR Merge → Status Updates to "Done"

After PR is reviewed and merged:
```bash
# Status automatically should update to "Done"
# If not automatic, manually update:
$mutation = @"
mutation {
  updateProjectV2ItemFieldValue(
    input: {
      projectId: "PVT_kwHOAFw6AM4BK9-n"
      itemId: "$itemId"
      fieldId: "PVTSSF_lAHOAFw6AM4BK9-nzg6rqRo"
      value: { 
        singleSelectOptionId: "98236657"
      }
    }
  ) {
    projectV2Item { id }
  }
}
"@

gh api graphql -f query=$mutation
```

### Complete Status Workflow

```
┌─────────────┐
│   Backlog   │ (f75ad846) - Issue created
└──────┬──────┘
       │ Create branch + Push
       ↓
┌─────────────┐
│ In Progress │ (47fc9ee4) - Branch pushed, working
└──────┬──────┘
       │ Create PR
       ↓
┌─────────────┐
│  In Review  │ (df73e18b) - PR open, reviewing
└──────┬──────┘
       │ Merge PR
       ↓
┌─────────────┐
│    Done     │ (98236657) - PR merged, complete
└─────────────┘
```

### Validation Checklist

Before starting implementation, verify:
- ✅ Issue read and understood using `mcp_io_github_git_issue_read`
- ✅ Branch created from latest `main`
- ✅ Branch name follows convention: `{issue-number}-{short-description}`
- ✅ **Branch pushed to GitHub immediately** with `git push -u origin branch-name`
- ✅ **Issue status updated to "In Progress"** using GraphQL API
- ✅ Relevant ADRs reviewed (see list below)
- ✅ Multi-tenancy requirements understood
- ✅ Test strategy planned (unit + integration)

### Quick Reference Commands

**Get Issue Details:**
```bash
gh issue view 16
```

**Create and Push Branch:**
```bash
git checkout -b 16-entry-submission-api
git push -u origin 16-entry-submission-api
```

**Update to In Progress:**
```bash
# Use GraphQL mutation with status ID: 47fc9ee4
```

**Create PR:**
```bash
gh pr create --title "feat: description (#16)" --body "...Closes #16"
```

**Update to In Review:**
```bash
# Use GraphQL mutation with status ID: df73e18b
```

**NEVER skip any of these steps** - they provide critical project visibility and ensure proper collaboration.

### Agent Responsibilities

**@product-owner (Workflow Coordinator)**:
- ✅ Read issue details
- ✅ Create and push feature branch
- ✅ Update issue to "In Progress"
- ✅ Identify and delegate to specialized agent
- ✅ Update issue to "In Review" when PR created
- ✅ Coordinate releases and sprint planning
- ❌ Does NOT implement code (delegates to specialists)

**@backend (API & Business Logic)**:
- ✅ Implement API endpoints, CQRS handlers, domain models
- ✅ Write unit and integration tests
- ✅ Create database migrations
- ✅ Create PR when complete
- ✅ Notify @product-owner when PR is ready
- ❌ Does NOT create branches or update issue status

**@frontend (User Interface)**:
- ✅ Implement React components and pages
- ✅ Write component, integration, and E2E tests
- ✅ Ensure accessibility (WCAG 2.1 AA)
- ✅ Implement offline-first PWA features
- ✅ Create PR when complete
- ✅ Notify @product-owner when PR is ready
- ❌ Does NOT create branches or update issue status

**@devops (Infrastructure & CI/CD)**:
- ✅ Configure cloud infrastructure (Azure, Docker)
- ✅ Create CI/CD pipelines
- ✅ Set up monitoring and observability
- ✅ Manage secrets and configurations
- ✅ Create PR when complete
- ✅ Notify @product-owner when PR is ready
- ❌ Does NOT create branches or update issue status

**@qa (Testing & Quality Assurance)**:
- ✅ Define comprehensive test strategies
- ✅ Write E2E and load tests
- ✅ Validate acceptance criteria
- ✅ Perform security and performance testing
- ✅ Create PR when complete
- ✅ Notify @product-owner when PR is ready
- ❌ Does NOT create branches or update issue status

### Real-World Team Simulation

This workflow replicates a professional software development team:

1. **Product Owner** manages backlog, coordinates sprints, tracks progress
2. **Specialized Developers** focus on their domain expertise
3. **Clear Handoffs** with explicit communication between roles
4. **Centralized Coordination** prevents duplication and conflicts
5. **Status Visibility** for stakeholders through GitHub Projects

**Example Flow**:
```
User: "I want to start issue #100"
  ↓
@product-owner:
  - Reads issue: "Configure Keycloak user attributes"
  - Creates branch: 100-keycloak-user-attributes
  - Pushes to GitHub
  - Updates status → "In Progress"
  - Sees label: "agent:devops"
  - Delegates: "@devops please configure Keycloak for issue #100"
  ↓
@devops:
  - Updates realm-export.json with protocol mappers
  - Tests JWT token generation
  - Commits: "feat: configure Keycloak user attributes (#100)"
  - Creates PR: "Closes #100"
  - Notifies: "@product-owner PR #X created for issue #100"
  ↓
@product-owner:
  - Updates status → "In Review"
  - Notifies team: "Issue #100 ready for review"
  ↓
[PR reviewed and merged]
  ↓
Status automatically → "Done" ✅
```

### Key Principles

**DO**:
- ✅ Always invoke `@product-owner` when starting a new issue
- ✅ Let Product Owner handle ALL branch creation and status updates
- ✅ Have specialized agents focus ONLY on implementation
- ✅ Create PRs that link to issues with "Closes #N"
- ✅ Notify Product Owner when PR is created
- ✅ Use conventional commit messages referencing issue numbers

**DON'T**:
- ❌ Never have specialized agents create branches directly
- ❌ Never duplicate status update logic across agents
- ❌ Never skip the Product Owner for issue setup
- ❌ Never commit directly to main (always use feature branches)
- ❌ Never create PRs without linking to the issue

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
   - OAuth 2.0 Token Exchange (RFC 8693) for service-specific tokens
   - Service-specific audiences (bff-api, competition-service, judging-service)
   - JWT with `tenant_id` + `roles` claims
   - Roles: Organizer, Judge, Entrant, Steward (no admin role)

**CRITICAL**: When creating new microservices, **ALWAYS implement service-specific audience validation**:
- Create Keycloak bearer-only client with audience mapper
- Configure JWT authentication with service-specific audience
- Update BFF configuration for token exchange
- See: [Service Audiences & Token Exchange Guide](docs/architecture/SERVICE_AUDIENCES_TOKEN_EXCHANGE.md)

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

**MANDATORY: Follow the "MANDATORY Workflow: Starting New Issues" section at the top of this file.**

1. **Create feature branch** using `{issue-number}-{short-description}` format
2. **Read the relevant ADR** from `docs/architecture/decisions/`
3. **Check existing patterns** in codebase
4. **Verify multi-tenancy** is enforced
5. **Write tests first** (TDD approach)
6. **Run integration tests** with Testcontainers before PR
7. **Reference issue number** in all commits: `feat: description (#issue-number)`

---

## Getting Help

- **Architecture questions** → Review `docs/architecture/ARCHITECTURE.md`
- **Decisions context** → Read ADRs in `docs/architecture/decisions/`
- **Feature planning** → Check `docs/BACKLOG.md`
- **Agent-specific help** → Use specialized agents (@backend, @frontend, etc.)

---

## Success Criteria

✅ Branch created with correct naming convention: `{issue-number}-{short-description}`  
✅ All commits reference issue number: `feat: description (#issue)`  
✅ All code follows multi-tenancy rules (tenant_id always present)  
✅ Events published via Outbox Pattern (no dual-write)  
✅ CQRS pattern used (MediatR handlers)  
✅ Authorization enforced on all endpoints  
✅ PWA works offline (service workers + IndexedDB)  
✅ Tests pass (unit + integration + E2E)  
✅ No secrets committed to repo  

**When in doubt, ask the specialized agent or review the ADRs!**
