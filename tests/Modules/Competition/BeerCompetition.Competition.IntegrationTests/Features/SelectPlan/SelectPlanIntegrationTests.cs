using BeerCompetition.Competition.Application.Features.SelectPlan;
using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.IntegrationTests.Builders;
using BeerCompetition.Competition.IntegrationTests.Infrastructure;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BeerCompetition.Competition.IntegrationTests.Features.SelectPlan;

[Collection("Integration Tests")]
public class SelectPlanIntegrationTests : IntegrationTestBase
{
    private readonly IMediator _mediator;

    public SelectPlanIntegrationTests(IntegrationTestWebApplicationFactory factory)
        : base(factory)
    {
        _mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Handle_ValidPlanSelection_PersistsToDatabase()
    {
        // Arrange: Create tenant, competition, and plans
        var tenant = TenantBuilder.Default().Build();
        await DbContext.Tenants.AddAsync(tenant);

        var competition = CompetitionBuilder.Default()
            .WithTenantId(tenant.Id)
            .WithName("Spring Classic 2025")
            .Build();
        await DbContext.Competitions.AddAsync(competition);

        var basicPlan = SubscriptionPlan.Create(tenant.Id, "BASIC", 50, 49.00m, "EUR").Value;
        await DbContext.SubscriptionPlans.AddAsync(basicPlan);
        await DbContext.SaveChangesAsync();

        // Set tenant context
        Factory.TenantProvider.SetTenant(tenant.Id);

        var command = new SelectPlanCommand(competition.Id, "BASIC");

        // Act
        var result = await _mediator.Send(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PlanId.Should().Be(basicPlan.Id);
        result.Value.MaxEntries.Should().Be(50);
        result.Value.PaymentStatus.Should().Be("MOCK_PAID");

        // Verify database persistence
        var verifyContext = GetFreshDbContext();
        var updatedCompetition = await verifyContext.Competitions
            .FirstAsync(c => c.Id == competition.Id);

        updatedCompetition.SubscriptionPlanId.Should().Be(basicPlan.Id);
        updatedCompetition.MaxEntries.Should().Be(50);
        updatedCompetition.IsPublic.Should().BeTrue();
    }

    [Theory]
    [InlineData("TRIAL", 10, 0.00)]
    [InlineData("BASIC", 50, 49.00)]
    [InlineData("STANDARD", 200, 149.00)]
    [InlineData("PRO", 600, 299.00)]
    public async Task Handle_AllPlanTypes_WorkCorrectly(
        string planName, int expectedMaxEntries, decimal expectedPrice)
    {
        // Arrange
        var tenant = TenantBuilder.Default().Build();
        await DbContext.Tenants.AddAsync(tenant);

        var competition = CompetitionBuilder.Default()
            .WithTenantId(tenant.Id)
            .Build();
        await DbContext.Competitions.AddAsync(competition);

        var plan = SubscriptionPlan.Create(tenant.Id, planName, expectedMaxEntries, expectedPrice, "EUR").Value;
        await DbContext.SubscriptionPlans.AddAsync(plan);
        await DbContext.SaveChangesAsync();

        Factory.TenantProvider.SetTenant(tenant.Id);

        var command = new SelectPlanCommand(competition.Id, planName);

        // Act
        var result = await _mediator.Send(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MaxEntries.Should().Be(expectedMaxEntries);
        result.Value.PriceAmount.Should().Be(expectedPrice);
        result.Value.PaymentStatus.Should().Be("MOCK_PAID");

        var verifyContext = GetFreshDbContext();
        var updatedCompetition = await verifyContext.Competitions
            .FirstAsync(c => c.Id == competition.Id);
        updatedCompetition.MaxEntries.Should().Be(expectedMaxEntries);
        updatedCompetition.IsPublic.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MultiTenancy_IsolatesPlansCorrectly()
    {
        // Arrange: Two tenants with same plan names
        var tenant1 = TenantBuilder.Default().WithEmail("tenant1@test.com").Build();
        var tenant2 = TenantBuilder.Default().WithEmail("tenant2@test.com").Build();
        await DbContext.Tenants.AddRangeAsync(tenant1, tenant2);

        var comp1 = CompetitionBuilder.Default()
            .WithTenantId(tenant1.Id)
            .WithName("Tenant 1 Competition")
            .Build();
        var comp2 = CompetitionBuilder.Default()
            .WithTenantId(tenant2.Id)
            .WithName("Tenant 2 Competition")
            .Build();
        await DbContext.Competitions.AddRangeAsync(comp1, comp2);

        var plan1 = SubscriptionPlan.Create(tenant1.Id, "BASIC", 50, 49.00m, "EUR").Value;
        var plan2 = SubscriptionPlan.Create(tenant2.Id, "BASIC", 50, 49.00m, "EUR").Value;
        await DbContext.SubscriptionPlans.AddRangeAsync(plan1, plan2);
        await DbContext.SaveChangesAsync();

        // Act: Tenant 1 selects plan
        Factory.TenantProvider.SetTenant(tenant1.Id);
        var command1 = new SelectPlanCommand(comp1.Id, "BASIC");
        var result1 = await _mediator.Send(command1, CancellationToken.None);

        // Act: Tenant 2 selects plan
        Factory.TenantProvider.SetTenant(tenant2.Id);
        var command2 = new SelectPlanCommand(comp2.Id, "BASIC");
        var result2 = await _mediator.Send(command2, CancellationToken.None);

        // Assert: Both successful but isolated
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();

        var verifyContext = GetFreshDbContext();
        
        Factory.TenantProvider.SetTenant(tenant1.Id);
        var updatedComp1 = await verifyContext.Competitions
            .FirstAsync(c => c.Id == comp1.Id);
        updatedComp1.SubscriptionPlanId.Should().Be(plan1.Id);

        Factory.TenantProvider.SetTenant(tenant2.Id);
        var updatedComp2 = await verifyContext.Competitions
            .FirstAsync(c => c.Id == comp2.Id);
        updatedComp2.SubscriptionPlanId.Should().Be(plan2.Id);
    }

    [Fact]
    public async Task Handle_DuplicatePlanSelection_ReturnsFailure()
    {
        // Arrange
        var tenant = TenantBuilder.Default().Build();
        await DbContext.Tenants.AddAsync(tenant);

        var competition = CompetitionBuilder.Default()
            .WithTenantId(tenant.Id)
            .Build();
        await DbContext.Competitions.AddAsync(competition);

        var plan = SubscriptionPlan.Create(tenant.Id, "BASIC", 50, 49.00m, "EUR").Value;
        await DbContext.SubscriptionPlans.AddAsync(plan);
        await DbContext.SaveChangesAsync();

        Factory.TenantProvider.SetTenant(tenant.Id);

        var command = new SelectPlanCommand(competition.Id, "BASIC");

        // Act: First selection succeeds
        var result1 = await _mediator.Send(command, CancellationToken.None);
        result1.IsSuccess.Should().BeTrue();

        // Act: Second selection fails
        var result2 = await _mediator.Send(command, CancellationToken.None);

        // Assert
        result2.IsFailure.Should().BeTrue();
        result2.Error.Should().Contain("already has a subscription plan");
    }

    [Fact]
    public async Task Handle_CompetitionNotFound_ReturnsFailure()
    {
        // Arrange
        var tenant = TenantBuilder.Default().Build();
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        Factory.TenantProvider.SetTenant(tenant.Id);

        var command = new SelectPlanCommand(Guid.NewGuid(), "BASIC");

        // Act
        var result = await _mediator.Send(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_PlanNotFound_ReturnsFailure()
    {
        // Arrange
        var tenant = TenantBuilder.Default().Build();
        await DbContext.Tenants.AddAsync(tenant);

        var competition = CompetitionBuilder.Default()
            .WithTenantId(tenant.Id)
            .Build();
        await DbContext.Competitions.AddAsync(competition);
        await DbContext.SaveChangesAsync();

        Factory.TenantProvider.SetTenant(tenant.Id);

        var command = new SelectPlanCommand(competition.Id, "NONEXISTENT");

        // Act
        var result = await _mediator.Send(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }
}
