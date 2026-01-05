using BeerCompetition.Competition.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeerCompetition.Competition.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core configuration for Competition entity.
/// Defines database schema, constraints, and relationships using Fluent API.
/// </summary>
internal class CompetitionConfiguration : IEntityTypeConfiguration<Domain.Entities.Competition>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Competition> builder)
    {
        // Table mapping with schema
        builder.ToTable("competitions", "Competition");

        // Primary key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();  // Generated in domain

        // Multi-tenancy
        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("ix_competitions_tenant_id");

        // Properties
        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(c => c.RegistrationDeadline)
            .HasColumnName("registration_deadline")
            .IsRequired();

        builder.Property(c => c.JudgingStartDate)
            .HasColumnName("judging_start_date")
            .IsRequired();

        builder.Property(c => c.JudgingEndDate)
            .HasColumnName("judging_end_date");

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()  // Store enum as string
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.MaxEntriesPerEntrant)
            .HasColumnName("max_entries_per_entrant")
            .HasDefaultValue(10)
            .IsRequired();

        // Subscription Plan (MVP: MOCK payment)
        builder.Property(c => c.SubscriptionPlanId)
            .HasColumnName("subscription_plan_id");

        builder.Property(c => c.MaxEntries)
            .HasColumnName("max_entries")
            .HasDefaultValue(10)
            .IsRequired();

        builder.Property(c => c.IsPublic)
            .HasColumnName("is_public")
            .HasDefaultValue(false)
            .IsRequired();

        // Audit fields
        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(c => new { c.TenantId, c.Status })
            .HasDatabaseName("ix_competitions_tenant_status");

        builder.HasIndex(c => c.RegistrationDeadline)
            .HasDatabaseName("ix_competitions_registration_deadline");

        // Unique index on SubscriptionPlanId to enforce one-to-one relationship
        // Each SubscriptionPlan can only be assigned to one Competition
        builder.HasIndex(c => c.SubscriptionPlanId)
            .IsUnique()
            .HasFilter("subscription_plan_id IS NOT NULL")  // Allow multiple nulls (competitions without plans)
            .HasDatabaseName("ix_competitions_subscription_plan_id_unique");

        // Relationships
        // Many Competitions -> One Tenant
        builder.HasOne(c => c.Tenant)
            .WithMany(t => t.Competitions)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade delete

        // One Competition -> One SubscriptionPlan (one-to-one relationship)
        builder.HasOne<SubscriptionPlan>()
            .WithOne()  // One-to-one: each SubscriptionPlan can only belong to one Competition
            .HasForeignKey<Domain.Entities.Competition>(c => c.SubscriptionPlanId)
            .IsRequired(false)  // Nullable FK (competition can exist without plan initially)
            .OnDelete(DeleteBehavior.SetNull);  // Set null if plan deleted

        // Ignore domain events (not persisted to database)
        builder.Ignore(c => c.DomainEvents);
    }
}
