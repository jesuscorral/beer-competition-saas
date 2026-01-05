using BeerCompetition.Competition.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeerCompetition.Competition.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core configuration for CompetitionUser entity.
/// Defines database schema, constraints, and relationships using Fluent API.
/// </summary>
internal class CompetitionUserConfiguration : IEntityTypeConfiguration<CompetitionUser>
{
    public void Configure(EntityTypeBuilder<CompetitionUser> builder)
    {
        // Table mapping with schema
        builder.ToTable("competition_users", "Competition");

        // Primary key
        builder.HasKey(cu => cu.Id);
        builder.Property(cu => cu.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();  // Generated in domain

        // Multi-tenancy
        builder.Property(cu => cu.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(cu => cu.TenantId)
            .HasDatabaseName("ix_competition_users_tenant_id");

        // Properties
        builder.Property(cu => cu.CompetitionId)
            .HasColumnName("competition_id")
            .IsRequired();

        builder.Property(cu => cu.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(cu => cu.Role)
            .HasColumnName("role")
            .HasConversion<string>()  // Store enum as string (ENTRANT, JUDGE, STEWARD, ORGANIZER)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cu => cu.Status)
            .HasColumnName("status")
            .HasConversion<string>()  // Store enum as string (ACTIVE, PENDING_APPROVAL, REJECTED)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cu => cu.ApprovalNote)
            .HasColumnName("approval_note")
            .HasMaxLength(1000);

        builder.Property(cu => cu.RegisteredAt)
            .HasColumnName("registered_at")
            .IsRequired();

        builder.Property(cu => cu.ApprovedAt)
            .HasColumnName("approved_at");

        builder.Property(cu => cu.ApprovedBy)
            .HasColumnName("approved_by")
            .HasMaxLength(255);

        // Audit fields (inherited from Entity)
        builder.Property(cu => cu.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(cu => cu.UpdatedAt)
            .HasColumnName("updated_at");

        // Unique constraint: one registration per competition+user+role combination
        // Prevents duplicate registrations (e.g., user can't register twice as JUDGE for same competition)
        builder.HasIndex(cu => new { cu.CompetitionId, cu.UserId, cu.Role })
            .IsUnique()
            .HasDatabaseName("ix_competition_users_competition_user_role_unique");

        // Index for organizer queries: get pending approvals by competition
        builder.HasIndex(cu => new { cu.CompetitionId, cu.Status })
            .HasDatabaseName("ix_competition_users_competition_status");

        // Index for user queries: get all registrations by user
        builder.HasIndex(cu => cu.UserId)
            .HasDatabaseName("ix_competition_users_user_id");

        // Relationship to Competition
        builder.HasOne(cu => cu.Competition)
            .WithMany()  // Competition doesn't need navigation to CompetitionUsers in MVP
            .HasForeignKey(cu => cu.CompetitionId)
            .OnDelete(DeleteBehavior.Cascade)  // Delete user registrations if competition is deleted
            .HasConstraintName("fk_competition_users_competition_id");

        // Ignore domain events (not persisted)
        builder.Ignore(cu => cu.DomainEvents);
    }
}
