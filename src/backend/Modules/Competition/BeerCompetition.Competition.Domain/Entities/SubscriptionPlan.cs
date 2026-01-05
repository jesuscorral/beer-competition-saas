using BeerCompetition.Shared.Kernel;

namespace BeerCompetition.Competition.Domain.Entities;

/// <summary>
/// Subscription plan template for competitions (MVP: MOCK payment).
/// Defines available plans with pricing and entry limits.
/// </summary>
public class SubscriptionPlan : Entity, ITenantEntity
{
    // Note: TenantId, CreatedAt, UpdatedAt inherited from Entity base class
    
    public string Name { get; private set; } = string.Empty;
    public int MaxEntries { get; private set; }
    public decimal PriceAmount { get; private set; } // Decimal for pricing
    public string PriceCurrency { get; private set; } = "EUR";

    // Private constructor for EF Core
    private SubscriptionPlan() { }

    // Factory method
    public static Result<SubscriptionPlan> Create(
        Guid tenantId,
        string name,
        int maxEntries,
        decimal priceAmount,
        string priceCurrency)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(name))
            return Result<SubscriptionPlan>.Failure("Name cannot be empty");

        if (maxEntries <= 0)
            return Result<SubscriptionPlan>.Failure("MaxEntries must be positive");

        if (priceAmount < 0)
            return Result<SubscriptionPlan>.Failure("PriceAmount cannot be negative");

        if (!IsValidCurrency(priceCurrency))
            return Result<SubscriptionPlan>.Failure($"Invalid currency: {priceCurrency}. Valid: EUR, USD, GBP");

        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name.ToUpperInvariant(),
            MaxEntries = maxEntries,
            PriceAmount = priceAmount,
            PriceCurrency = priceCurrency.ToUpperInvariant()
        };

        return Result<SubscriptionPlan>.Success(plan);
    }

    private static bool IsValidCurrency(string currency)
    {
        var validCurrencies = new[] { "EUR", "USD", "GBP" };
        return !string.IsNullOrWhiteSpace(currency) && 
               validCurrencies.Contains(currency.ToUpperInvariant());
    }
}
