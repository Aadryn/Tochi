// <copyright file="AssignmentExpirationConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Authorization.Infrastructure.PostgreSQL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.Infrastructure.PostgreSQL.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité AssignmentExpiration.
/// </summary>
public sealed class AssignmentExpirationConfiguration : IEntityTypeConfiguration<AssignmentExpirationEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AssignmentExpirationEntity> builder)
    {
        builder.ToTable("assignment_expirations", "authorization");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .UseIdentityColumn();

        builder.Property(e => e.TenantId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.PrincipalId)
            .IsRequired();

        builder.Property(e => e.PrincipalType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Role)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Scope)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.ExpiresAt)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Index sur la date d'expiration pour les requêtes de cleanup
        builder.HasIndex(e => e.ExpiresAt)
            .HasDatabaseName("idx_assignment_expirations_expires_at");

        // Index sur le tenant pour les requêtes filtrées
        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("idx_assignment_expirations_tenant");

        // Contrainte d'unicité sur la combinaison tenant/principal/role/scope
        builder.HasIndex(e => new { e.TenantId, e.PrincipalId, e.Role, e.Scope })
            .IsUnique()
            .HasDatabaseName("uq_assignment_expirations_assignment");
    }
}
