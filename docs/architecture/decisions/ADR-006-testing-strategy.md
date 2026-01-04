# ADR-006: Testing Strategy

**Date**: 2025-12-19  
**Status**: Accepted (Updated 2026-01-04)  
**Deciders**: Architecture Team  
**Context**: Beer Competition SaaS Platform

---

## Context and Problem Statement

A robust testing strategy is critical for a multi-tenant SaaS platform handling critical competition data. We need to ensure:
- **Correctness**: Business logic works as expected (scoring, blind judging, conflict-of-interest)
- **Reliability**: System handles failures gracefully (database outages, message queue failures)
- **Multi-tenancy**: Data isolation guaranteed across tenants
- **Regression Prevention**: Changes don't break existing functionality
- **Continuous Deployment**: Fast feedback loop for CI/CD

**Challenges:**
- **Microservices Complexity**: Testing event-driven architecture requires message bus
- **Database Dependencies**: Integration tests need real PostgreSQL (not mocks)
- **Multi-tenancy**: Must validate RLS policies work correctly
- **Offline Capabilities**: Frontend PWA requires service worker testing
- **Test Speed**: Balance thoroughness with fast feedback

How do we implement a **comprehensive testing strategy** that provides high confidence without slowing down development?

---

## Decision Drivers

- **Confidence**: High coverage of critical paths (scoring, payments, data isolation)
- **Speed**: Fast feedback loop (<5 minutes for unit tests, <15 minutes for full suite)
- **Realism**: Integration tests use real infrastructure (PostgreSQL, RabbitMQ)
- **Maintainability**: Tests easy to read, write, and maintain
- **CI/CD Integration**: Automated testing in GitHub Actions
- **Cost**: Minimize infrastructure costs for test environments
- **Developer Experience**: Simple test setup (no manual infrastructure provisioning)

---

## Considered Options

### 1. Unit Tests Only
**Approach**: Mock all dependencies, test business logic in isolation

**Pros:**
- ✅ Fast execution (milliseconds)
- ✅ No infrastructure needed

**Cons:**
- ❌ **Low confidence**: Mocks don't catch integration issues
- ❌ **Database logic untested**: RLS policies, SQL queries ignored
- ❌ **Event-driven flows untested**: Message bus interactions missed

---

### 2. Full E2E Tests Only
**Approach**: Test via UI, spin up entire system for every test

**Pros:**
- ✅ High confidence (tests real system)

**Cons:**
- ❌ **Extremely slow**: Minutes per test
- ❌ **Brittle**: UI changes break tests
- ❌ **Hard to debug**: Failures difficult to isolate

---

### 3. Test Pyramid (Unit + Integration + E2E)
**Approach**: Many unit tests, moderate integration tests, few E2E tests

**Pros:**
- ✅ **Balanced**: Speed + confidence
- ✅ **Fast feedback**: Unit tests catch most issues
- ✅ **Realistic**: Integration tests validate real infrastructure

**Cons:**
- ❌ **Complexity**: Must manage test infrastructure

---

### 4. Test Pyramid + Testcontainers
**Approach**: Test Pyramid + Docker containers for integration tests (PostgreSQL, RabbitMQ)

**Pros:**
- ✅ **Real infrastructure**: No mocks for databases/message buses
- ✅ **Fast setup**: Containers start automatically
- ✅ **Isolated**: Each test run gets fresh containers
- ✅ **CI/CD friendly**: Works in GitHub Actions

**Cons:**
- ❌ **Docker dependency**: Requires Docker installed
- ❌ **Slower than mocks**: Container startup overhead (~5-10s)

---

## Decision Outcome

**Chosen Option**: **#4 - Test Pyramid + Testcontainers**

---

## Test Pyramid Structure

```
         ┌─────────┐
         │   E2E   │  ~10 tests (critical user flows)
         │ Cypress │  Slow (2-5 min each)
         └─────────┘
            ▲
       ┌────────────┐
       │ Integration│  ~100 tests (API + database + events)
       │Testcontainers Moderate (1-5 sec each)
       └────────────┘
          ▲
    ┌──────────────┐
    │  Unit Tests  │  ~500 tests (business logic, handlers)
    │   xUnit      │  Fast (<1ms each)
    └──────────────┘
```

### Test Distribution
- **70% Unit Tests**: Business logic, domain models, validators
- **20% Integration Tests**: Database, message bus, API endpoints
- **10% E2E Tests**: Critical user flows (submit entry → judge → view results)

---

## Implementation Details

### 1. Unit Tests (xUnit + FluentAssertions)

#### Test Structure
```csharp
public class CreateCompetitionHandlerTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<ILogger<CreateCompetitionHandler>> _loggerMock;
    private readonly CreateCompetitionHandler _handler;

    public CreateCompetitionHandlerTests()
    {
        _dbContextMock = new Mock<ApplicationDbContext>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _loggerMock = new Mock<ILogger<CreateCompetitionHandler>>();
        
        _tenantProviderMock.Setup(p => p.CurrentTenantId)
            .Returns(Guid.Parse("123e4567-e89b-12d3-a456-426614174000"));

        _handler = new CreateCompetitionHandler(
            _dbContextMock.Object,
            _tenantProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateCompetitionCommand(
            Name: "Spring Classic 2025",
            RegistrationDeadline: DateTime.UtcNow.AddDays(30),
            JudgingStartDate: DateTime.UtcNow.AddDays(35),
            StylesSupported: "IPA,Stout");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_JudgingBeforeRegistrationCloses_ReturnsFailure()
    {
        // Arrange
        var command = new CreateCompetitionCommand(
            Name: "Invalid Competition",
            RegistrationDeadline: DateTime.UtcNow.AddDays(30),
            JudgingStartDate: DateTime.UtcNow.AddDays(25),  // Before deadline
            StylesSupported: "IPA");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("before judging starts");
    }
}
```

#### Domain Model Tests
```csharp
public class EntryTests
{
    [Fact]
    public void Create_GeneratesJudgingNumber()
    {
        // Act
        var entry = Entry.Create(
            competitionId: Guid.NewGuid(),
            brewerName: "John Doe",
            styleId: "21A");

        // Assert
        entry.JudgingNumber.Should().BeGreaterThan(0);
        entry.BrewerName.Should().Be("[REDACTED]");  // Anonymous during judging
    }

    [Theory]
    [InlineData(42, 12, 54)]  // Aroma + Appearance
    [InlineData(20, 5, 25)]   // Flavor + Mouthfeel
    public void CalculateFinalScore_SumsComponents(
        int component1, 
        int component2, 
        int expected)
    {
        // Arrange
        var scoresheet = new Scoresheet
        {
            Aroma = component1,
            Appearance = component2
        };

        // Act
        var score = scoresheet.CalculateFinalScore();

        // Assert
        score.Should().Be(expected);
    }
}
```

---

### 2. Integration Tests (Testcontainers + WebApplicationFactory + Respawn)

**Updated Implementation (2026-01-04)**: The project uses **WebApplicationFactory**, **Testcontainers**, **Respawn**, and **Builder Pattern** for clean, maintainable integration tests.

#### Key Components

1. **IntegrationTestWebApplicationFactory**: Custom `WebApplicationFactory` managing PostgreSQL containers (Testcontainers), applies migrations, and provides test infrastructure (mocked services, `TestTenantProvider`)

2. **IntegrationTestBase**: Base class for all integration tests with **Respawn** for intelligent database cleanup (truncates data, preserves schema)

3. **TestTenantProvider**: Dynamic tenant context provider for tests - allows switching tenant context with `SetTenant(Guid)` and `ClearTenant()`, eliminating need for `IgnoreQueryFilters()` in most tests

4. **Builder Pattern**: Fluent builders for test data:
   - `TenantBuilder`: Creates Tenant entities with sensible defaults
   - `CompetitionBuilder`: Creates Competition entities with sensible defaults
   - Validates domain rules at build time

#### Example Integration Test

```csharp
public class RegisterOrganizerIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task Handle_CompleteFlow_CreatesAllEntities()
    {
        // Arrange: Mock external services
        Factory.KeycloakService
            .Setup(s => s.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        var command = new RegisterOrganizerCommand(
            Email: "john@example.com",
            OrganizationName: "John's Homebrew Club");

        // Act: Execute command via MediatR
        var result = await _mediator.Send(command);

        // Assert: Set tenant context and verify
        result.IsSuccess.Should().BeTrue();
        Factory.TenantProvider.SetTenant(result.Value.TenantId);

        var verifyContext = GetFreshDbContext();
        var tenant = await verifyContext.Tenants.FirstOrDefaultAsync();
        tenant.Should().NotBeNull();
        tenant!.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task Handle_MultipleOrganizers_IsolatesTenants()
    {
        // Arrange: Use builders for test data
        var tenant1 = TenantBuilder.Default()
            .WithEmail("org1@example.com")
            .Build();

        var context = GetFreshDbContext();
        await context.Tenants.AddAsync(tenant1);
        await context.SaveChangesAsync();

        Factory.TenantProvider.SetTenant(tenant1.Id);
        var competition1 = CompetitionBuilder.Default()
            .WithTenantId(tenant1.Id)
            .WithName("Tenant 1 Competition")
            .Build();
        await context.Competitions.AddAsync(competition1);
        await context.SaveChangesAsync();

        // Act: Query as Tenant 1
        var verifyContext = GetFreshDbContext();
        var competitions = await verifyContext.Competitions.ToListAsync();

        // Assert: Multi-tenancy enforced
        competitions.Should().HaveCount(1);
        competitions[0].Name.Should().Be("Tenant 1 Competition");
    }
}
```

#### Benefits

- ✅ Real PostgreSQL 16 container (no database mocks)
- ✅ Automatic migrations and cleanup (Respawn)
- ✅ Dynamic tenant context (TestTenantProvider)
- ✅ Builder pattern for readable test setup
- ✅ Fast execution (~1-5 seconds per test)

---

### 2. Integration Tests (Original Testcontainers Approach)

#### Test Fixture (PostgreSQL Container)
```csharp
public class DatabaseFixture : IAsyncLifetime
{
    public PostgreSqlContainer PostgresContainer { get; private set; }
    public string ConnectionString => PostgresContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        // Start PostgreSQL container
        PostgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("beercomp_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await PostgresContainer.StartAsync();

        // Run migrations
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        using var context = new ApplicationDbContext(options, null);
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await PostgresContainer.DisposeAsync();
    }
}
```

#### Integration Test Example
```csharp
public class CompetitionIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ApplicationDbContext _dbContext;

    public CompetitionIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        _dbContext = new ApplicationDbContext(
            options, 
            new TenantProvider(Guid.Parse("123e4567-e89b-12d3-a456-426614174000")));
    }

    [Fact]
    public async Task CreateCompetition_PersistsToDatabase()
    {
        // Arrange
        var handler = new CreateCompetitionHandler(_dbContext, ...);
        var command = new CreateCompetitionCommand(...);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var competition = await _dbContext.Competitions
            .FirstOrDefaultAsync(c => c.Id == result.Value);
        
        competition.Should().NotBeNull();
        competition.Name.Should().Be("Spring Classic 2025");
    }

    [Fact]
    public async Task RLS_BlocksCrossTenantQuery()
    {
        // Arrange: Insert competitions for two tenants
        var tenantA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var tenantB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        await _dbContext.Competitions.AddAsync(new Competition 
        { 
            TenantId = tenantA, 
            Name = "Tenant A Competition" 
        });
        await _dbContext.Competitions.AddAsync(new Competition 
        { 
            TenantId = tenantB, 
            Name = "Tenant B Competition" 
        });
        await _dbContext.SaveChangesAsync();

        // Act: Query as Tenant A
        await _dbContext.Database.ExecuteSqlRawAsync(
            $"SET LOCAL app.current_tenant = '{tenantA}'");
        
        var competitions = await _dbContext.Competitions.ToListAsync();

        // Assert: RLS blocks Tenant B's data
        competitions.Should().HaveCount(1);
        competitions[0].Name.Should().Be("Tenant A Competition");
    }
}
```

#### RabbitMQ Integration Tests
```csharp
public class RabbitMQFixture : IAsyncLifetime
{
    public RabbitMqContainer RabbitMqContainer { get; private set; }

    public async Task InitializeAsync()
    {
        RabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management-alpine")
            .Build();

        await RabbitMqContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await RabbitMqContainer.DisposeAsync();
    }
}

public class EventPublishingTests : IClassFixture<RabbitMQFixture>
{
    [Fact]
    public async Task OutboxWorker_PublishesEventToRabbitMQ()
    {
        // Arrange: Insert event in outbox
        await _dbContext.EventStore.AddAsync(new EventStoreEntry
        {
            EventType = "entry.submitted",
            EventData = JsonSerializer.Serialize(cloudEvent),
            PublishedStatus = "PENDING"
        });
        await _dbContext.SaveChangesAsync();

        // Act: Run background worker
        await _worker.ProcessBatchAsync();

        // Assert: Event marked as published
        var evt = await _dbContext.EventStore.FirstAsync();
        evt.PublishedStatus.Should().Be("PUBLISHED");

        // Assert: Message in RabbitMQ
        var factory = new ConnectionFactory 
        { 
            Uri = new Uri(_fixture.RabbitMqContainer.GetConnectionString()) 
        };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        
        var message = channel.BasicGet("judging_service_queue", autoAck: true);
        message.Should().NotBeNull();
    }
}
```

---

### 3. API Integration Tests (WebApplicationFactory)

```csharp
public class CompetitionApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CompetitionApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real DB with Testcontainers
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(_testConnectionString));
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task POST_Competition_Returns201Created()
    {
        // Arrange
        var request = new CreateCompetitionRequest
        {
            Name = "Spring Classic 2025",
            RegistrationDeadline = DateTime.UtcNow.AddDays(30),
            JudgingStartDate = DateTime.UtcNow.AddDays(35)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/competitions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var competitionId = await response.Content.ReadFromJsonAsync<Guid>();
        competitionId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GET_Competition_WithoutAuth_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/competitions/123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

---

### 4. E2E Tests (Cypress)

#### Test Structure
```javascript
// cypress/e2e/competition-flow.cy.js
describe('Competition Flow', () => {
  beforeEach(() => {
    cy.login('organizer@example.com', 'password');
  });

  it('should create competition, submit entry, and view results', () => {
    // 1. Create competition
    cy.visit('/competitions/new');
    cy.get('[data-testid="competition-name"]').type('Spring Classic 2025');
    cy.get('[data-testid="registration-deadline"]').type('2025-03-15');
    cy.get('[data-testid="judging-start-date"]').type('2025-03-20');
    cy.get('[data-testid="submit-button"]').click();
    cy.url().should('include', '/competitions/');

    // 2. Submit entry
    cy.get('[data-testid="submit-entry-button"]').click();
    cy.get('[data-testid="entry-name"]').type('West Coast IPA');
    cy.get('[data-testid="style-select"]').select('21A - American IPA');
    cy.get('[data-testid="submit-button"]').click();
    cy.contains('Entry submitted successfully');

    // 3. View results (after judging)
    cy.visit('/competitions/results');
    cy.contains('West Coast IPA');
    cy.get('[data-testid="final-score"]').should('be.visible');
  });
});
```

#### Offline PWA Test
```javascript
describe('Offline Scoresheet Entry', () => {
  it('should save scoresheet offline and sync when online', () => {
    cy.login('judge@example.com', 'password');
    cy.visit('/judging/flight/123');

    // Go offline
    cy.window().then(win => {
      win.navigator.onLine = false;
    });

    // Fill scoresheet
    cy.get('[data-testid="aroma-score"]').type('10');
    cy.get('[data-testid="flavor-score"]').type('18');
    cy.get('[data-testid="submit-button"]').click();
    cy.contains('Saved offline');

    // Go online
    cy.window().then(win => {
      win.navigator.onLine = true;
    });

    // Wait for sync
    cy.contains('Synced to server', { timeout: 10000 });
  });
});
```

---

### 5. Performance Tests (NBomber)

```csharp
public class PerformanceTests
{
    [Fact]
    public void LoadTest_50ConcurrentJudges_ShouldHandleLoad()
    {
        var scenario = Scenario.Create("submit_scoresheet", async context =>
        {
            var request = Http.CreateRequest("POST", "https://localhost:5001/api/scoresheets")
                .WithHeader("Authorization", $"Bearer {_token}")
                .WithJsonBody(new SubmitScoresheetRequest
                {
                    FlightId = Guid.NewGuid(),
                    Aroma = 10,
                    Flavor = 18,
                    // ...
                });

            var response = await Http.Send(request);
            return response;
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(5))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Assert: 95th percentile < 500ms
        stats.ScenarioStats[0].Ok.Latency.Percent95.Should().BeLessThan(500);
    }
}
```

---

## CI/CD Integration (GitHub Actions)

```yaml
name: CI/CD Pipeline

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Run Unit Tests
        run: dotnet test --filter "Category=Unit" --logger "console;verbosity=detailed"
      
      - name: Start Docker Compose (for integration tests)
        run: docker-compose -f docker-compose.test.yml up -d
      
      - name: Run Integration Tests
        run: dotnet test --filter "Category=Integration" --logger "console;verbosity=detailed"
      
      - name: Run E2E Tests
        run: |
          npm install --prefix ./frontend
          npx cypress run --spec "cypress/e2e/**/*.cy.js"
      
      - name: Publish Test Results
        uses: dorny/test-reporter@v1
        with:
          name: Test Results
          path: '**/TestResults/*.xml'
          reporter: dotnet-trx
```

---

## Consequences

### Positive
✅ **High Confidence**: Real infrastructure catches integration issues  
✅ **Fast Feedback**: Unit tests run in <1 minute  
✅ **Realistic**: Testcontainers use actual PostgreSQL/RabbitMQ  
✅ **CI/CD Friendly**: Automated testing in GitHub Actions  
✅ **Maintainable**: Tests easy to read and understand  
✅ **Isolated**: Each test run gets fresh containers  
✅ **Multi-tenancy Validated**: RLS policies tested with real DB  

### Negative
❌ **Docker Dependency**: Requires Docker installed locally + CI  
❌ **Slower Integration Tests**: Container startup adds 5-10s overhead  
❌ **E2E Test Fragility**: UI changes can break tests  
❌ **Maintenance Burden**: Must update tests when features change  

### Risks
⚠️ **Testcontainers Stability**: Occasional Docker networking issues  
⚠️ **CI Resource Usage**: Many parallel tests consume GitHub Actions minutes  
⚠️ **Test Data Management**: Must seed realistic test data  

---

## Alternatives Considered

### Why not mocks for integration tests?
- ❌ **Low confidence**: Mocks don't catch SQL errors, RLS issues
- ❌ **Fragile**: Mocks diverge from real behavior over time
- ✅ **Testcontainers**: Real infrastructure with minimal overhead

### Why not manual infrastructure for tests?
- ❌ **Slow setup**: Developers must provision PostgreSQL locally
- ❌ **Inconsistent**: Different DB versions across machines
- ✅ **Testcontainers**: Automated, consistent, isolated

### Why not E2E tests only?
- ❌ **Too slow**: Full system startup takes minutes
- ❌ **Hard to debug**: Failures difficult to isolate
- ✅ **Test Pyramid**: Fast unit tests catch most issues

---

## Related Decisions

- [ADR-001: Tech Stack Selection](ADR-001-tech-stack-selection.md) (xUnit, Cypress)
- [ADR-002: Multi-Tenancy Strategy](ADR-002-multi-tenancy-strategy.md) (RLS testing)
- [ADR-003: Event-Driven Architecture](ADR-003-event-driven-architecture.md) (RabbitMQ testing)

---

## References

- [Testcontainers Documentation](https://dotnet.testcontainers.org/)
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
- [Cypress Documentation](https://www.cypress.io/)
- [NBomber Performance Testing](https://nbomber.com/)
- [Test Pyramid (Martin Fowler)](https://martinfowler.com/articles/practical-test-pyramid.html)
