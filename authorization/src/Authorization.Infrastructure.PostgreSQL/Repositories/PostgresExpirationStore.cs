// <copyright file="PostgresExpirationStore.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Authorization.Domain.Entities;
using Authorization.Domain.Interfaces;
using Authorization.Infrastructure.PostgreSQL.Data;
using Authorization.Infrastructure.PostgreSQL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation PostgreSQL du stockage des expirations d'assignation.
/// </summary>
/// <remarks>
/// <para>
/// Cette implémentation utilise Entity Framework Core pour interagir
/// avec la table assignment_expirations dans PostgreSQL.
/// </para>
/// <para>
/// Les requêtes sont optimisées via un index sur la colonne expires_at
/// pour permettre des lookups rapides des assignations expirées.
/// </para>
/// </remarks>
public sealed class PostgresExpirationStore : IExpirationStore
{
    private readonly AuthorizationAuditDbContext _dbContext;
    private readonly ILogger<PostgresExpirationStore> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="PostgresExpirationStore"/>.
    /// </summary>
    /// <param name="dbContext">Contexte de base de données.</param>
    /// <param name="logger">Logger.</param>
    public PostgresExpirationStore(
        AuthorizationAuditDbContext dbContext,
        ILogger<PostgresExpirationStore> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AssignmentExpiration> CreateAsync(
        CreateAssignmentExpirationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = new AssignmentExpirationEntity
        {
            TenantId = request.TenantId,
            PrincipalId = request.PrincipalId,
            PrincipalType = request.PrincipalType,
            Role = request.Role,
            Scope = request.Scope,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.AssignmentExpirations.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Expiration créée: {Role} sur {Scope} pour {PrincipalType}:{PrincipalId} expire le {ExpiresAt}",
            request.Role,
            request.Scope,
            request.PrincipalType,
            request.PrincipalId,
            request.ExpiresAt);

        return MapToModel(entity);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AssignmentExpiration>> GetExpiredAsync(
        DateTimeOffset asOf,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.AssignmentExpirations
            .Where(e => e.ExpiresAt <= asOf)
            .OrderBy(e => e.ExpiresAt)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        _logger.LogDebug(
            "Trouvé {Count} assignations expirées avant {AsOf}",
            entities.Count,
            asOf);

        return entities.Select(MapToModel).ToList();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _dbContext.AssignmentExpirations
            .Where(e => e.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
        {
            _logger.LogDebug("Expiration {Id} supprimée", id);
        }

        return deleted > 0;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteByAssignmentAsync(
        string tenantId,
        Guid principalId,
        string role,
        string scope,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _dbContext.AssignmentExpirations
            .Where(e =>
                e.TenantId == tenantId &&
                e.PrincipalId == principalId &&
                e.Role == role &&
                e.Scope == scope)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
        {
            _logger.LogDebug(
                "Expiration supprimée pour {Role} sur {Scope} pour {PrincipalId}",
                role,
                scope,
                principalId);
        }

        return deleted > 0;
    }

    /// <inheritdoc />
    public async Task<AssignmentExpiration?> GetByAssignmentAsync(
        string tenantId,
        Guid principalId,
        string role,
        string scope,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.AssignmentExpirations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e =>
                    e.TenantId == tenantId &&
                    e.PrincipalId == principalId &&
                    e.Role == role &&
                    e.Scope == scope,
                cancellationToken);

        return entity is null ? null : MapToModel(entity);
    }

    /// <summary>
    /// Mappe une entité vers le modèle du domaine.
    /// </summary>
    private static AssignmentExpiration MapToModel(AssignmentExpirationEntity entity)
    {
        return new AssignmentExpiration(
            entity.Id,
            entity.TenantId,
            entity.PrincipalId,
            entity.PrincipalType,
            entity.Role,
            entity.Scope,
            entity.ExpiresAt,
            entity.CreatedAt);
    }
}
