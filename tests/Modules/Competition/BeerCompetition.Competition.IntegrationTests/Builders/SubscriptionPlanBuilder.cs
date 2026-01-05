using BeerCompetition.Competition.Domain.Entities;

namespace BeerCompetition.Competition.IntegrationTests.Builders;

public class SubscriptionPlanBuilder
{
    private Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private string _name = "TRIAL";
    private int _maxEntries = 10;
    private decimal _priceAmount = 0.00m;
    private string _priceCurrency = "EUR";

    public static SubscriptionPlanBuilder Default() => new();

    public SubscriptionPlanBuilder WithTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public SubscriptionPlanBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SubscriptionPlanBuilder WithMaxEntries(int maxEntries)
    {
        _maxEntries = maxEntries;
        return this;
    }

    public SubscriptionPlanBuilder WithPrice(decimal amount, string currency = "EUR")
    {
        _priceAmount = amount;
        _priceCurrency = currency;
        return this;
    }

    public SubscriptionPlanBuilder AsTrialPlan()
    {
        _name = "TRIAL";
        _maxEntries = 10;
        _priceAmount = 0.00m;
        return this;
    }

    public SubscriptionPlanBuilder AsBasicPlan()
    {
        _name = "BASIC";
        _maxEntries = 50;
        _priceAmount = 49.00m;
        return this;
    }

    public SubscriptionPlanBuilder AsStandardPlan()
    {
        _name = "STANDARD";
        _maxEntries = 200;
        _priceAmount = 149.00m;
        return this;
    }

    public SubscriptionPlanBuilder AsProPlan()
    {
        _name = "PRO";
        _maxEntries = 600;
        _priceAmount = 299.00m;
        return this;
    }

    public SubscriptionPlan Build()
    {
        return SubscriptionPlan.Create(
            _tenantId, _name, _maxEntries, _priceAmount, _priceCurrency).Value;
    }
}
