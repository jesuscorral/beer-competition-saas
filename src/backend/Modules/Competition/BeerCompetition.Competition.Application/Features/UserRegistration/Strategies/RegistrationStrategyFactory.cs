using BeerCompetition.Competition.Domain.Entities;

namespace BeerCompetition.Competition.Application.Features.UserRegistration.Strategies;

/// <summary>
/// Factory for obtaining the correct registration strategy based on user role.
/// Uses dependency injection to resolve all registered strategies.
/// </summary>
public class RegistrationStrategyFactory
{
    private readonly IEnumerable<IRegistrationStrategy> _strategies;
    
    public RegistrationStrategyFactory(IEnumerable<IRegistrationStrategy> strategies)
    {
        _strategies = strategies;
    }
    
    /// <summary>
    /// Gets the registration strategy for the specified role.
    /// </summary>
    /// <param name="role">The competition user role.</param>
    /// <returns>The corresponding registration strategy.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no strategy is found for the role.</exception>
    public IRegistrationStrategy GetStrategy(CompetitionUserRole role)
    {
        var strategy = _strategies.FirstOrDefault(s => s.Role == role);
        
        if (strategy == null)
        {
            throw new InvalidOperationException(
                $"No registration strategy found for role: {role}. " +
                $"Ensure the strategy is registered in DI.");
        }
        
        return strategy;
    }
}
