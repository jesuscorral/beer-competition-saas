using BeerCompetition.Competition.Application.Features.GetAvailablePlans;
using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace BeerCompetition.Competition.UnitTests.Features.GetAvailablePlans;

public class GetAvailablePlansQueryHandlerTests
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ILogger<GetAvailablePlansQueryHandler> _logger;
    private readonly GetAvailablePlansQueryHandler _handler;
    private readonly Guid _tenantId;

    public GetAvailablePlansQueryHandlerTests()
    {
        _planRepository = Substitute.For<ISubscriptionPlanRepository>();
        _logger = Substitute.For<ILogger<GetAvailablePlansQueryHandler>>();
        _tenantId = Guid.NewGuid();

        _handler = new GetAvailablePlansQueryHandler(_planRepository, _logger);
    }

    [Fact]
    public async Task Handle_PlansExist_ReturnsOrderedList()
    {
        // Arrange
        var plans = new List<SubscriptionPlan>
        {
            SubscriptionPlan.Create(_tenantId, "PRO", 600, 299.00m, "EUR").Value!,
            SubscriptionPlan.Create(_tenantId, "TRIAL", 10, 0.00m, "EUR").Value!,
            SubscriptionPlan.Create(_tenantId, "STANDARD", 200, 149.00m, "EUR").Value!,
            SubscriptionPlan.Create(_tenantId, "BASIC", 50, 49.00m, "EUR").Value!
        };

        _planRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(plans));

        var query = new GetAvailablePlansQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(4);
        
        // Verify ordering by price
        result.Value[0].PriceAmount.Should().Be(0.00m); // TRIAL
        result.Value[1].PriceAmount.Should().Be(49.00m); // BASIC
        result.Value[2].PriceAmount.Should().Be(149.00m); // STANDARD
        result.Value[3].PriceAmount.Should().Be(299.00m); // PRO
    }

    [Fact]
    public async Task Handle_NoPlans_ReturnsEmptyList()
    {
        // Arrange
        _planRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<SubscriptionPlan>()));

        var query = new GetAvailablePlansQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_BasicPlanExists_IsMarkedAsRecommended()
    {
        // Arrange
        var plans = new List<SubscriptionPlan>
        {
            SubscriptionPlan.Create(_tenantId, "TRIAL", 10, 0.00m, "EUR").Value!,
            SubscriptionPlan.Create(_tenantId, "BASIC", 50, 49.00m, "EUR").Value!,
            SubscriptionPlan.Create(_tenantId, "STANDARD", 200, 149.00m, "EUR").Value!
        };

        _planRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(plans));

        var query = new GetAvailablePlansQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var basicPlan = result.Value.First(p => p.Name == "BASIC");
        basicPlan.IsRecommended.Should().BeTrue();
        
        result.Value.Where(p => p.Name != "BASIC").Should().AllSatisfy(p => 
            p.IsRecommended.Should().BeFalse());
    }

    [Fact]
    public async Task Handle_ReturnsPlansWithDescriptions()
    {
        // Arrange
        var plans = new List<SubscriptionPlan>
        {
            SubscriptionPlan.Create(_tenantId, "TRIAL", 10, 0.00m, "EUR").Value!,
            SubscriptionPlan.Create(_tenantId, "BASIC", 50, 49.00m, "EUR").Value!
        };

        _planRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(plans));

        var query = new GetAvailablePlansQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().AllSatisfy(p => p.Description.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public async Task Handle_MapsAllPlanProperties()
    {
        // Arrange
        var plan = SubscriptionPlan.Create(_tenantId, "BASIC", 50, 49.00m, "EUR").Value!;
        var plans = new List<SubscriptionPlan> { plan };

        _planRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(plans));

        var query = new GetAvailablePlansQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value.Single();
        
        dto.Id.Should().Be(plan.Id);
        dto.Name.Should().Be("BASIC");
        dto.MaxEntries.Should().Be(50);
        dto.PriceAmount.Should().Be(49.00m);
        dto.PriceCurrency.Should().Be("EUR");
    }
}
