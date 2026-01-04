using BeerCompetition.Competition.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeerCompetition.Competition.Infrastructure.Persistence;

/// <summary>
/// EF Core configuration for SubscriptionPlan entity.
/// Maps to subscription_plans table with proper indexes and constraints.
/// </summary>
public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("subscription_plans", "Competition");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.MaxEntries)
            .HasColumnName("max_entries")
            .IsRequired();

        builder.Property(p => p.PriceAmount)
            .HasColumnName("price_amount")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(p => p.PriceCurrency)
            .HasColumnName("price_currency")
            .HasMaxLength(3)
            .HasDefaultValue("EUR")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(p => p.TenantId)
            .HasDatabaseName("idx_subscription_plans_tenant");

        builder.HasIndex(p => new { p.TenantId, p.Name })
            .HasDatabaseName("idx_subscription_plans_tenant_name")
            .IsUnique(); // Each tenant can only have one plan of each name

        // Ignore domain events
        builder.Ignore(p => p.DomainEvents);
    }
}
