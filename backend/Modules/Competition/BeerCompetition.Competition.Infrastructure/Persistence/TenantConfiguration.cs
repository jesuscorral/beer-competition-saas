using BeerCompetition.Competition.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeerCompetition.Competition.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core configuration for Tenant entity.
/// Defines database schema, constraints, and relationships using Fluent API.
/// </summary>
internal class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        // Table mapping with schema
        builder.ToTable("tenants", "Competition");

        // Primary key
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();  // Generated in domain

        // Multi-tenancy: TenantId is same as Id for Tenant entity (self-reference)
        builder.Property(t => t.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        // Properties
        builder.Property(t => t.OrganizationName)
            .HasColumnName("organization_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()  // Store enum as string
            .HasMaxLength(50)
            .IsRequired();

        // Audit fields
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(t => t.Email)
            .IsUnique()
            .HasDatabaseName("ix_tenants_email");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("ix_tenants_status");

        // Relationships
        // One Tenant -> Many Competitions
        builder.HasMany(t => t.Competitions)
            .WithOne(c => c.Tenant)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade delete

        // Ignore domain events (not persisted to database)
        builder.Ignore(t => t.DomainEvents);
    }
}
