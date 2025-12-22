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
        // Database
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

        // Repositories
        services.AddScoped<ICompetitionRepository, CompetitionRepository>();

        // MediatR handlers from Application layer
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateCompetitionCommand).Assembly);
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(CreateCompetitionValidator).Assembly);

        // MediatR pipeline behaviors could be added here (e.g., validation, logging, transactions)

        return services;
    }
}
