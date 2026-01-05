using BeerCompetition.Competition.Application.Features.GetAvailablePlans;
using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.IntegrationTests.Builders;
using BeerCompetition.Competition.IntegrationTests.Infrastructure;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BeerCompetition.Competition.IntegrationTests.Features.GetAvailablePlans;

[Collection("Integration Tests")]
public class GetAvailablePlansIntegrationTests : IntegrationTestBase
{
    private readonly IMediator _mediator;

    public GetAvailablePlansIntegrationTests(IntegrationTestWebApplicationFactory factory)
        : base(factory)
    {
        _mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Handle_PlansExist_ReturnsAllPlansForTenant()
    {
        // Arrange
        var tenant = TenantBuilder.Default().Build();
        await DbContext.Tenants.AddAsync(tenant);

        var plans = new[]
        {
            SubscriptionPlan.Create(tenant.Id, "TRIAL", 10, 0.00m, "EUR").Value,
            SubscriptionPlan.Create(tenant.Id, "BASIC", 50, 49.00m, "EUR").Value,
            SubscriptionPlan.Create(tenant.Id, "STANDARD", 200, 149.00m, "EUR").Value,
            SubscriptionPlan.Create(tenant.Id, "PRO", 600, 299.00m, "EUR").Value
        };
        await DbContext.SubscriptionPlans.AddRangeAsync(plans);
        await DbContext.SaveChangesAsync();

        Factory.TenantProvider.SetTenant(tenant.Id);

        var query = new GetAvailablePlansQuery();

        // Act
        var result = await _mediator.Send(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(4);
        result.Value.Select(p => p.Name).Should().BeEquivalentTo(new[] { "TRIAL", "BASIC", "STANDARD", "PRO" });
        
        // Verify ordering by price
        result.Value[0].PriceAmount.Should().Be(0.00m);
        result.Value[1].PriceAmount.Should().Be(49.00m);
        result.Value[2].PriceAmount.Should().Be(149.00m);
        result.Value[3].PriceAmount.Should().Be(299.00m);
    }

    [Fact]
    public async Task Handle_MultiTenancy_ReturnsOnlyCurrentTenantPlans()
    {
        // Arrange
        var tenant1 = TenantBuilder.Default().WithEmail("tenant1@test.com").Build();
        var tenant2 = TenantBuilder.Default().WithEmail("tenant2@test.com").Build();
        await DbContext.Tenants.AddRangeAsync(tenant1, tenant2);

        var plan1 = SubscriptionPlan.Create(tenant1.Id, "BASIC", 50, 49.00m, "EUR").Value;
        var plan2 = SubscriptionPlan.Create(tenant1.Id, "STANDARD", 200, 149.00m, "EUR").Value;
        var plan3 = SubscriptionPlan.Create(tenant2.Id, "BASIC", 100, 99.00m, "EUR").Value;
        await DbContext.SubscriptionPlans.AddRangeAsync(plan1, plan2, plan3);
        await DbContext.SaveChangesAsync();

        Factory.TenantProvider.SetTenant(tenant1.Id);

        var query = new GetAvailablePlansQuery();

        // Act
        var result = await _mediator.Send(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2); // Only tenant1's plans
        result.Value.Select(p => p.Name).Should().BeEquivalentTo(new[] { "BASIC", "STANDARD" });
        result.Value.Should().AllSatisfy(p => 
        {
            // Verify it's tenant1's BASIC plan (49.00) not tenant2's (99.00)
            if (p.Name == "BASIC")
                p.PriceAmount.Should().Be(49.00m);
        });
    }

    [Fact]
    public async Task Handle_NoPlansExist_ReturnsEmptyList()
    {
        // Arrange
        var tenant = TenantBuilder.Default().Build();
        await DbContext.Tenants.AddAsync(tenant);
        await DbContext.SaveChangesAsync();

        Factory.TenantProvider.SetTenant(tenant.Id);

        var query = new GetAvailablePlansQuery();

        // Act
        var result = await _mediator.Send(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_BasicPlanExists_IsMarkedAsRecommended()
    {
        // Arrange
        var tenant = TenantBuilder.Default().Build();
        await DbContext.Tenants.AddAsync(tenant);

        var plans = new[]
        {
            SubscriptionPlan.Create(tenant.Id, "TRIAL", 10, 0.00m, "EUR").Value,
            SubscriptionPlan.Create(tenant.Id, "BASIC", 50, 49.00m, "EUR").Value,
            SubscriptionPlan.Create(tenant.Id, "PRO", 600, 299.00m, "EUR").Value
        };
        await DbContext.SubscriptionPlans.AddRangeAsync(plans);
        await DbContext.SaveChangesAsync();

        Factory.TenantProvider.SetTenant(tenant.Id);

        var query = new GetAvailablePlansQuery();

        // Act
        var result = await _mediator.Send(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var basicPlan = result.Value.First(p => p.Name == "BASIC");
        basicPlan.IsRecommended.Should().BeTrue();
        
        result.Value.Where(p => p.Name != "BASIC")
            .Should().AllSatisfy(p => p.IsRecommended.Should().BeFalse());
    }

    [Fact]
    public async Task Handle_ReturnsPlansWithDescriptions()
    {
        // Arrange
        var tenant = TenantBuilder.Default().Build();
        await DbContext.Tenants.AddAsync(tenant);

        var plan = SubscriptionPlan.Create(tenant.Id, "TRIAL", 10, 0.00m, "EUR").Value;
        await DbContext.SubscriptionPlans.AddAsync(plan);
        await DbContext.SaveChangesAsync();

        Factory.TenantProvider.SetTenant(tenant.Id);

        var query = new GetAvailablePlansQuery();

        // Act
        var result = await _mediator.Send(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value.First().Description.Should().NotBeNullOrEmpty();
        result.Value.First().Description.Should().Contain("10 entries");
    }
}
