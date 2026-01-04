using BeerCompetition.Competition.Domain.Entities;
using BeerCompetition.Competition.Domain.Events;
using FluentAssertions;
using Xunit;

namespace BeerCompetition.Competition.UnitTests.Domain.Entities;

public class SubscriptionPlanTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void Create_ValidData_ReturnsSuccessResult()
    {
        // Act
        var result = SubscriptionPlan.Create(
            tenantId: _tenantId,
            name: "TRIAL",
            maxEntries: 10,
            priceAmount: 0.00m,
            priceCurrency: "EUR"
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("TRIAL");
        result.Value.MaxEntries.Should().Be(10);
        result.Value.PriceAmount.Should().Be(0.00m);
        result.Value.PriceCurrency.Should().Be("EUR");
        result.Value.TenantId.Should().Be(_tenantId);
    }

    [Theory]
    [InlineData("", 10, 0.00, "Name cannot be empty")]
    [InlineData("TRIAL", 0, 0.00, "MaxEntries must be positive")]
    [InlineData("TRIAL", -5, 0.00, "MaxEntries must be positive")]
    [InlineData("TRIAL", 10, -10.00, "PriceAmount cannot be negative")]
    public void Create_InvalidData_ReturnsFailureResult(
        string name, int maxEntries, decimal price, string expectedErrorPart)
    {
        // Act
        var result = SubscriptionPlan.Create(_tenantId, name, maxEntries, price, "EUR");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain(expectedErrorPart);
    }

    [Theory]
    [InlineData("EUR")]
    [InlineData("USD")]
    [InlineData("GBP")]
    public void Create_ValidCurrencies_AllAccepted(string currency)
    {
        // Act
        var result = SubscriptionPlan.Create(_tenantId, "BASIC", 50, 49.00m, currency);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PriceCurrency.Should().Be(currency);
    }

    [Theory]
    [InlineData("XYZ")]
    [InlineData("ABC")]
    [InlineData("")]
    public void Create_InvalidCurrency_ReturnsFailure(string currency)
    {
        // Act
        var result = SubscriptionPlan.Create(_tenantId, "BASIC", 50, 49.00m, currency);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid currency");
    }

    [Theory]
    [InlineData("TRIAL", 10, 0.00)]
    [InlineData("BASIC", 50, 49.00)]
    [InlineData("STANDARD", 200, 149.00)]
    [InlineData("PRO", 600, 299.00)]
    public void Create_PredefinedPlans_CreatesCorrectly(string planName, int expectedMaxEntries, decimal expectedPrice)
    {
        // Act
        var result = SubscriptionPlan.Create(_tenantId, planName, expectedMaxEntries, expectedPrice, "EUR");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(planName);
        result.Value.MaxEntries.Should().Be(expectedMaxEntries);
        result.Value.PriceAmount.Should().Be(expectedPrice);
    }

    [Fact]
    public void Create_SetsIdAndTimestamps()
    {
        // Act
        var result = SubscriptionPlan.Create(_tenantId, "TRIAL", 10, 0.00m, "EUR");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBe(Guid.Empty);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
