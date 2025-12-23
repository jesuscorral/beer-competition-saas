using BeerCompetition.Competition.Application.Features.CreateCompetition;
using BeerCompetition.Competition.Domain.Repositories;
using BeerCompetition.Competition.Infrastructure.Persistence;
using BeerCompetition.Competition.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeerCompetition.Competition.Infrastructure;

/// <summary>
/// Dependency injection configuration for Competition module.
/// Registers all services required by the module (DbContext, Repositories, Validators, Handlers).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCompetitionModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add database configuration
        services.AddDatabaseConfiguration(configuration);

        // Add Repositories
        services.AddDIRepositories();

        // Add MediatR configuration
        services.AddMediatRConfiguration();

        services.AddFluentValidationConfiguration();

        // MediatR pipeline behaviors could be added here (e.g., validation, logging, transactions)

        return services;
    }

    /// <summary>
    /// Configures the database context for the Competition module.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Additional database-related configurations can be added here in the future

        services.AddDbContext<CompetitionDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("PostgreSQL")
                ?? throw new InvalidOperationException("PostgreSQL connection string not configured");

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(CompetitionDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });

            // Enable detailed errors in development
            if (configuration.GetValue<bool>("EnableSensitiveDataLogging"))
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        return services;
    }

    /// <summary>
    /// Configures the repositories for the Competition module.
    /// </summary>
    public static IServiceCollection AddDIRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICompetitionRepository, CompetitionRepository>();
        return services;
    }

    /// <summary>
    /// Configures MediatR for the Competition module.
    /// </summary>
    public static IServiceCollection AddMediatRConfiguration(this IServiceCollection services)
    {
        // MediatR handlers from Application layer
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateCompetitionCommand).Assembly);
        });

        return services;
    }

    public static IServiceCollection AddFluentValidationConfiguration(this IServiceCollection services)
    {
        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(CreateCompetitionValidator).Assembly);

        return services;
    } 
}
