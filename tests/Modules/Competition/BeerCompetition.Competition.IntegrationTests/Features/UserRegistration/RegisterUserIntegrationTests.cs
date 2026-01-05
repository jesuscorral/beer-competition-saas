using BeerCompetition.Competition.Application.Features.UserRegistration;
using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Competition.IntegrationTests.Infrastructure;
using BeerCompetition.Shared.Infrastructure.ExternalServices;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BeerCompetition.Competition.IntegrationTests.Features.UserRegistration;

public class RegisterUserIntegrationTests : IntegrationTestBase
{
    private readonly IMediator _mediator;
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
    private readonly ICompetitionUserRepository _competitionUserRepository;
    private readonly IKeycloakService _keycloakService;
    private readonly ITenantRepository _tenantRepository;
    private readonly Guid _testTenantId;

    public RegisterUserIntegrationTests(IntegrationTestWebApplicationFactory factory) 
        : base(factory)
    {
        _mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        _competitionRepository = Scope.ServiceProvider.GetRequiredService<ICompetitionRepository>();
        _subscriptionPlanRepository = Scope.ServiceProvider.GetRequiredService<ISubscriptionPlanRepository>();
        _competitionUserRepository = Scope.ServiceProvider.GetRequiredService<ICompetitionUserRepository>();
        _keycloakService = Scope.ServiceProvider.GetRequiredService<IKeycloakService>();
        _tenantRepository = Scope.ServiceProvider.GetRequiredService<ITenantRepository>();
        
        // Setup test tenant
        _testTenantId = Guid.NewGuid();
        Factory.TenantProvider.SetTenant(_testTenantId);
        
        // Configure Keycloak mock defaults for all tests
        ConfigureKeycloakMockDefaults();
    }
    
    private void ConfigureKeycloakMockDefaults()
    {
        // Default: SetUserAttributeAsync succeeds
        _keycloakService.SetUserAttributeAsync(
            Arg.Any<string>(), 
            Arg.Any<string>(), 
            Arg.Any<string>(), 
            Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Result.Success());
        
        // Default: DeleteUserAsync succeeds (for rollback scenarios)
        _keycloakService.DeleteUserAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Result.Success());
    }

    [Fact]
    public async Task RegisterUser_AsEntrant_ShouldCreateActiveUser()
    {
        // Arrange
        var competition = await CreateTestCompetitionAsync();
        var userId = Guid.NewGuid().ToString();
        
        _keycloakService.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Result<string>.Success(userId));
        
        _keycloakService.AssignRoleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Result.Success());

        var command = new RegisterUserCommand(
            Email: "entrant@example.com",
            Password: "SecurePassword123!",
            FullName: "John Entrant",
            Role: CompetitionUserRole.ENTRANT,
            CompetitionId: competition.Id,
            BjcpRank: null,
            OrganizationName: null);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.Status.Should().Be("ACTIVE");
        result.Value.Message.Should().Contain("Registration successful");

        // Verify database persistence
        var freshContext = GetFreshDbContext();
        var competitionUser = await freshContext.CompetitionUsers
            .FirstOrDefaultAsync(cu => cu.UserId == userId);
        
        competitionUser.Should().NotBeNull();
        competitionUser!.Role.Should().Be(CompetitionUserRole.ENTRANT);
        competitionUser.Status.Should().Be(CompetitionUserStatus.ACTIVE);
        competitionUser.CompetitionId.Should().Be(competition.Id);
        competitionUser.TenantId.Should().Be(_testTenantId);
    }

    [Fact]
    public async Task RegisterUser_AsJudge_ShouldCreatePendingApprovalUser()
    {
        // Arrange
        var competition = await CreateTestCompetitionAsync();
        var userId = Guid.NewGuid().ToString();
        
        _keycloakService.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Result<string>.Success(userId));
        
        _keycloakService.AssignRoleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Result.Success());

        var command = new RegisterUserCommand(
            Email: "judge@example.com",
            Password: "SecurePassword123!",
            FullName: "Jane Judge",
            Role: CompetitionUserRole.JUDGE,
            CompetitionId: competition.Id,
            BjcpRank: "Certified",
            OrganizationName: null);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("PENDING_APPROVAL");
        result.Value.Message.Should().Contain("Waiting for organizer approval");

        // Verify database persistence
        var freshContext = GetFreshDbContext();
        var competitionUser = await freshContext.CompetitionUsers
            .FirstOrDefaultAsync(cu => cu.UserId == userId);
        
        competitionUser.Should().NotBeNull();
        competitionUser!.Role.Should().Be(CompetitionUserRole.JUDGE);
        competitionUser.Status.Should().Be(CompetitionUserStatus.PENDING_APPROVAL);
        // Note: BjcpRank is stored in Keycloak user attributes, not in CompetitionUser table
    }

    [Fact]
    public async Task RegisterUser_AsSteward_ShouldCreatePendingApprovalUser()
    {
        // Arrange
        var competition = await CreateTestCompetitionAsync();
        var userId = Guid.NewGuid().ToString();
        
        _keycloakService.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Result<string>.Success(userId));
        
        _keycloakService.AssignRoleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Result.Success());

        var command = new RegisterUserCommand(
            Email: "steward@example.com",
            Password: "SecurePassword123!",
            FullName: "Steve Steward",
            Role: CompetitionUserRole.STEWARD,
            CompetitionId: competition.Id,
            BjcpRank: null,
            OrganizationName: null);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("PENDING_APPROVAL");

        // Verify database persistence
        var freshContext = GetFreshDbContext();
        var competitionUser = await freshContext.CompetitionUsers
            .FirstOrDefaultAsync(cu => cu.UserId == userId);
        
        competitionUser.Should().NotBeNull();
        competitionUser!.Role.Should().Be(CompetitionUserRole.STEWARD);
        competitionUser.Status.Should().Be(CompetitionUserStatus.PENDING_APPROVAL);
    }

    [Fact]
    public async Task RegisterUser_AsOrganizer_ShouldCreateTenantAndActiveUser()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var tenantRepository = Scope.ServiceProvider.GetRequiredService<ITenantRepository>();
        
        _keycloakService.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Result<string>.Success(userId));
        
        _keycloakService.AssignRoleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Result.Success());

        var command = new RegisterUserCommand(
            Email: "organizer@brewclub.com",
            Password: "SecurePassword123!",
            FullName: "Oliver Organizer",
            Role: CompetitionUserRole.ORGANIZER,
            CompetitionId: null,
            BjcpRank: null,
            OrganizationName: "My Homebrew Club");

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("ACTIVE");

        // Verify tenant was created
        // Note: We need to ignore query filters because the organizer creates their own tenant
        // with a different tenant_id than our test context
        var freshContext = GetFreshDbContext();
        var tenant = await freshContext.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Email == "organizer@brewclub.com");
        
        tenant.Should().NotBeNull();
        tenant!.Email.Should().Be("organizer@brewclub.com");
        tenant.Status.Should().Be(TenantStatus.Active);

        // Note: Organizers don't create CompetitionUser at registration
        // They will be added to competitions when they create them
    }

    [Fact]
    public async Task RegisterUser_WithNonExistentCompetition_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        
        _keycloakService.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Result<string>.Success(userId));

        var command = new RegisterUserCommand(
            Email: "entrant@example.com",
            Password: "SecurePassword123!",
            FullName: "John Doe",
            Role: CompetitionUserRole.ENTRANT,
            CompetitionId: Guid.NewGuid(), // Non-existent competition
            BjcpRank: null,
            OrganizationName: null);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Competition not found");

        // Verify no user was created in database
        var freshContext = GetFreshDbContext();
        var competitionUser = await freshContext.CompetitionUsers
            .FirstOrDefaultAsync(cu => cu.UserId == userId);
        
        competitionUser.Should().BeNull();
    }

    [Fact]
    public async Task RegisterUser_WithDuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var competition = await CreateTestCompetitionAsync();
        
        _keycloakService.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Result<string>.Failure("User with email already exists"));

        var command = new RegisterUserCommand(
            Email: "duplicate@example.com",
            Password: "SecurePassword123!",
            FullName: "John Doe",
            Role: CompetitionUserRole.ENTRANT,
            CompetitionId: competition.Id,
            BjcpRank: null,
            OrganizationName: null);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");

        // Verify no database changes
        var freshContext = GetFreshDbContext();
        var count = await freshContext.CompetitionUsers.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task RegisterUser_WithPrivateCompetition_ShouldReturnFailure()
    {
        // Arrange
        var competition = await CreateTestCompetitionAsync(isPublic: false);
        var userId = Guid.NewGuid().ToString();
        
        _keycloakService.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Result<string>.Success(userId));

        var command = new RegisterUserCommand(
            Email: "entrant@example.com",
            Password: "SecurePassword123!",
            FullName: "John Doe",
            Role: CompetitionUserRole.ENTRANT,
            CompetitionId: competition.Id,
            BjcpRank: null,
            OrganizationName: null);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not open for registration");
    }

    [Fact]
    public async Task RegisterUser_MultipleRoles_ShouldIsolateData()
    {
        // Arrange
        var competition = await CreateTestCompetitionAsync();
        var entrantUserId = Guid.NewGuid().ToString();
        var judgeUserId = Guid.NewGuid().ToString();
        
        _keycloakService.CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Result<string>.Success(entrantUserId), Shared.Kernel.Result<string>.Success(judgeUserId));
        
        _keycloakService.AssignRoleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Shared.Kernel.Result.Success());

        var entrantCommand = new RegisterUserCommand(
            Email: "entrant@example.com",
            Password: "Password123!",
            FullName: "Entrant User",
            Role: CompetitionUserRole.ENTRANT,
            CompetitionId: competition.Id,
            BjcpRank: null,
            OrganizationName: null);

        var judgeCommand = new RegisterUserCommand(
            Email: "judge@example.com",
            Password: "Password123!",
            FullName: "Judge User",
            Role: CompetitionUserRole.JUDGE,
            CompetitionId: competition.Id,
            BjcpRank: "Grand Master",
            OrganizationName: null);

        // Act
        var entrantResult = await _mediator.Send(entrantCommand);
        var judgeResult = await _mediator.Send(judgeCommand);

        // Assert
        entrantResult.IsSuccess.Should().BeTrue();
        judgeResult.IsSuccess.Should().BeTrue();

        var freshContext = GetFreshDbContext();
        var users = await freshContext.CompetitionUsers.ToListAsync();
        
        users.Should().HaveCount(2);
        users.Should().ContainSingle(u => u.Role == CompetitionUserRole.ENTRANT && u.Status == CompetitionUserStatus.ACTIVE);
        users.Should().ContainSingle(u => u.Role == CompetitionUserRole.JUDGE && u.Status == CompetitionUserStatus.PENDING_APPROVAL);
    }

    private async Task<Domain.Entities.Competition> CreateTestCompetitionAsync(bool isPublic = true)
    {
        // First, create the tenant (required for FK constraint)
        var tenant = Tenant.Create("Test Organization", "test@example.com");
        if (tenant.IsFailure)
            throw new InvalidOperationException($"Failed to create tenant: {tenant.Error}");
        
        // Set tenant ID to match _testTenantId
        tenant.Value.TenantId = _testTenantId;
        var tenantIdProperty = typeof(Tenant).GetProperty("Id");
        tenantIdProperty?.SetValue(tenant.Value, _testTenantId);
        
        await _tenantRepository.AddAsync(tenant.Value, CancellationToken.None);
        await _tenantRepository.SaveChangesAsync(CancellationToken.None);
        
        // Now create the competition
        var competition = Domain.Entities.Competition.Create(
            tenantId: _testTenantId,
            name: "Test Competition 2026",
            description: "Test competition for integration tests",
            registrationDeadline: DateTime.UtcNow.AddMonths(1),
            judgingStartDate: DateTime.UtcNow.AddMonths(2),
            judgingEndDate: DateTime.UtcNow.AddMonths(3));

        if (competition.IsFailure)
            throw new InvalidOperationException($"Failed to create competition: {competition.Error}");

        // If isPublic is true, create a plan and assign it
        if (isPublic)
        {
            var plan = SubscriptionPlan.Create(
                _testTenantId,
                "Test Plan",
                500,
                100m,
                "EUR");
            
            if (plan.IsFailure)
                throw new InvalidOperationException($"Failed to create plan: {plan.Error}");
            
            // Save the plan to database first (required for foreign key constraint)
            await _subscriptionPlanRepository.AddAsync(plan.Value, CancellationToken.None);
            await _competitionRepository.SaveChangesAsync(CancellationToken.None);
            
            var setPlanResult = competition.Value.SetSubscriptionPlan(plan.Value);
            if (setPlanResult.IsFailure)
                throw new InvalidOperationException($"Failed to set plan: {setPlanResult.Error}");
        }

        await _competitionRepository.AddAsync(competition.Value, CancellationToken.None);
        await _competitionRepository.SaveChangesAsync(CancellationToken.None);

        return competition.Value;
    }
}
