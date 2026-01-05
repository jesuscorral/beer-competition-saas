using BeerCompetition.Competition.Application.Features.SelectPlan;
using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace BeerCompetition.Competition.UnitTests.Features.SelectPlan;

public class SelectPlanCommandHandlerTests
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ILogger<SelectPlanCommandHandler> _logger;
    private readonly SelectPlanCommandHandler _handler;
    private readonly Guid _tenantId;

    public SelectPlanCommandHandlerTests()
    {
        _competitionRepository = Substitute.For<ICompetitionRepository>();
        _planRepository = Substitute.For<ISubscriptionPlanRepository>();
        _logger = Substitute.For<ILogger<SelectPlanCommandHandler>>();
        _tenantId = Guid.NewGuid();

        _handler = new SelectPlanCommandHandler(
            _competitionRepository,
            _planRepository,
            _logger
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesCompetitionAndReturnsSuccess()
    {
        // Arrange
        var competition = Competition.Domain.Entities.Competition.Create(
            _tenantId,
            "Spring Classic",
            "A classic competition for spring brews",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(31),
            DateTime.UtcNow.AddDays(32)).Value!;

        var plan = SubscriptionPlan.Create(_tenantId, "BASIC", 50, 49.00m, "EUR").Value!;

        _competitionRepository.GetByIdAsync(competition.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Competition.Domain.Entities.Competition?>(competition));
        
        _planRepository.GetByNameAsync("BASIC", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SubscriptionPlan?>(plan));

        _competitionRepository.UpdateAsync(Arg.Any<Competition.Domain.Entities.Competition>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var command = new SelectPlanCommand(competition.Id, "BASIC");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PlanId.Should().Be(plan.Id);
        result.Value.MaxEntries.Should().Be(50);
        result.Value.PaymentStatus.Should().Be("MOCK_PAID");
        result.Value.IsPublic.Should().BeTrue();

        await _competitionRepository.Received(1).UpdateAsync(competition, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CompetitionNotFound_ReturnsFailure()
    {
        // Arrange
        _competitionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Competition.Domain.Entities.Competition?>(null));

        var command = new SelectPlanCommand(Guid.NewGuid(), "BASIC");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Competition not found");
        
        await _competitionRepository.DidNotReceiveWithAnyArgs()
            .UpdateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_PlanNotFound_ReturnsFailure()
    {
        // Arrange
        var competition = Competition.Domain.Entities.Competition.Create(
            _tenantId,
            "Competition",
            "aaa",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(31),
            DateTime.UtcNow.AddDays(32)).Value!;

        _competitionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Competition.Domain.Entities.Competition?>(competition));
        
        _planRepository.GetByNameAsync("INVALID", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SubscriptionPlan?>(null));

        var command = new SelectPlanCommand(competition.Id, "INVALID");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        
        await _competitionRepository.DidNotReceiveWithAnyArgs()
            .UpdateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_CompetitionAlreadyHasPlan_ReturnsFailure()
    {
        // Arrange
        var competition = Competition.Domain.Entities.Competition.Create(
            _tenantId,
            "Competition",
            "A classic competition for spring brews",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(31),
            DateTime.UtcNow.AddDays(32)).Value!;

        var existingPlan = SubscriptionPlan.Create(_tenantId, "TRIAL", 10, 0.00m, "EUR").Value!;
        competition.SetSubscriptionPlan(existingPlan);

        _competitionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Competition.Domain.Entities.Competition?>(competition));

        var command = new SelectPlanCommand(competition.Id, "BASIC");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already has a subscription plan");
        
        await _competitionRepository.DidNotReceiveWithAnyArgs()
            .UpdateAsync(default!, default);
    }

    [Fact]
    public async Task Handle_TenantMismatch_ReturnsFailure()
    {
        // Arrange
        var competition = Competition.Domain.Entities.Competition.Create(
            _tenantId,
            "Competition",
            "aaa",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(31),
            DateTime.UtcNow.AddDays(32)).Value!;

        var differentTenantId = Guid.NewGuid();
        var plan = SubscriptionPlan.Create(differentTenantId, "BASIC", 50, 49.00m, "EUR").Value!;

        _competitionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Competition.Domain.Entities.Competition?>(competition));
        
        _planRepository.GetByNameAsync("BASIC", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SubscriptionPlan?>(plan));

        var command = new SelectPlanCommand(competition.Id, "BASIC");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("tenant mismatch");
        
        await _competitionRepository.DidNotReceiveWithAnyArgs()
            .UpdateAsync(default!, default);
    }

    [Theory]
    [InlineData("TRIAL", 10, 0.00)]
    [InlineData("BASIC", 50, 49.00)]
    [InlineData("STANDARD", 200, 149.00)]
    [InlineData("PRO", 600, 299.00)]
    public async Task Handle_AllPlanTypes_WorkCorrectly(string planName, int expectedMaxEntries, decimal expectedPrice)
    {
        // Arrange
        var competition = Competition.Domain.Entities.Competition.Create(
            _tenantId,
            "Competition",
            "aaa",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(31),
            DateTime.UtcNow.AddDays(32)).Value!;

        var plan = SubscriptionPlan.Create(_tenantId, planName, expectedMaxEntries, expectedPrice, "EUR").Value!;

        _competitionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Competition.Domain.Entities.Competition?>(competition));
        
        _planRepository.GetByNameAsync(planName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SubscriptionPlan?>(plan));

        _competitionRepository.UpdateAsync(Arg.Any<Competition.Domain.Entities.Competition>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var command = new SelectPlanCommand(competition.Id, planName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.MaxEntries.Should().Be(expectedMaxEntries);
        result.Value.PriceAmount.Should().Be(expectedPrice);
        result.Value.PaymentStatus.Should().Be("MOCK_PAID");
    }
}
