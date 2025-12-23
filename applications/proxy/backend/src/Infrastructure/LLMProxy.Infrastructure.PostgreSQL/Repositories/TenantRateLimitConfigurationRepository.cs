using LLMProxy.Application.Configuration.RateLimiting;
using LLMProxy.Application.Interfaces;
using LLMProxy.Infrastructure.PostgreSQL.Entities;
using Microsoft.EntityFrameworkCore;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Repository pour la gestion des configurations de rate limiting.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-017 Repository Pattern et ADR-041 Rate Limiting.
/// </para>
/// <para>
/// Ce repository gère la persistance des configurations de rate limiting par tenant
/// dans PostgreSQL. Les configurations incluent les limites globales, les limites
/// par API Key et les limites spécifiques par endpoint.
/// </para>
/// </remarks>
public sealed class TenantRateLimitConfigurationRepository : ITenantRateLimitConfigurationRepository
{
    private readonly LLMProxyDbContext _context;

    /// <summary>
    /// Initialise une nouvelle instance du repository.
    /// </summary>
    /// <param name="context">Contexte de base de données EF Core.</param>
    public TenantRateLimitConfigurationRepository(LLMProxyDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<TenantRateLimitConfiguration?> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken ct = default)
    {
        var entity = await _context.TenantRateLimitConfigurations
            .Include(e => e.EndpointLimits)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.TenantId == tenantId, ct);

        return entity == null ? null : MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<TenantRateLimitConfiguration> UpsertAsync(
        TenantRateLimitConfiguration config,
        CancellationToken ct = default)
    {
        var existing = await _context.TenantRateLimitConfigurations
            .Include(e => e.EndpointLimits)
            .FirstOrDefaultAsync(e => e.TenantId == config.TenantId, ct);

        if (existing == null)
        {
            // INSERT - Créer nouvelle entité
            var entity = MapToEntity(config);
            _context.TenantRateLimitConfigurations.Add(entity);
            await _context.SaveChangesAsync(ct);
            return MapToDomain(entity);
        }

        // UPDATE - Modifier entité existante
        existing.Update(
            config.GlobalLimit.RequestsPerMinute,
            config.GlobalLimit.RequestsPerDay,
            config.GlobalLimit.TokensPerMinute,
            config.GlobalLimit.TokensPerDay,
            config.ApiKeyLimit.RequestsPerMinute,
            config.ApiKeyLimit.TokensPerMinute);

        // Supprimer anciennes limites et recréer
        _context.EndpointLimits.RemoveRange(existing.EndpointLimits);

        foreach (var (path, limit) in config.EndpointLimits)
        {
            var endpointLimit = EndpointLimitEntity.Create(
                existing.Id,
                path,
                limit.RequestsPerMinute,
                limit.TokensPerMinute,
                limit.BurstCapacity);
            existing.EndpointLimits.Add(endpointLimit);
        }

        await _context.SaveChangesAsync(ct);
        return MapToDomain(existing);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid tenantId, CancellationToken ct = default)
    {
        var entity = await _context.TenantRateLimitConfigurations
            .FirstOrDefaultAsync(e => e.TenantId == tenantId, ct);

        if (entity != null)
        {
            _context.TenantRateLimitConfigurations.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _context.TenantRateLimitConfigurations
            .AnyAsync(e => e.TenantId == tenantId, ct);
    }

    /// <summary>
    /// Mappe une entité EF Core vers le modèle de domaine.
    /// </summary>
    /// <param name="entity">Entité source.</param>
    /// <returns>Configuration de domaine.</returns>
    private static TenantRateLimitConfiguration MapToDomain(TenantRateLimitConfigurationEntity entity)
    {
        var endpointLimits = entity.EndpointLimits
            .ToDictionary(
                el => el.EndpointPath,
                el => new EndpointLimit
                {
                    RequestsPerMinute = el.RequestsPerMinute,
                    TokensPerMinute = el.TokensPerMinute,
                    BurstCapacity = el.BurstCapacity
                });

        return new TenantRateLimitConfiguration
        {
            TenantId = entity.TenantId,
            GlobalLimit = new GlobalLimit
            {
                RequestsPerMinute = entity.GlobalRequestsPerMinute,
                RequestsPerDay = entity.GlobalRequestsPerDay,
                TokensPerMinute = entity.GlobalTokensPerMinute,
                TokensPerDay = entity.GlobalTokensPerDay
            },
            ApiKeyLimit = new ApiKeyLimit
            {
                RequestsPerMinute = entity.ApiKeyRequestsPerMinute,
                TokensPerMinute = entity.ApiKeyTokensPerMinute
            },
            EndpointLimits = endpointLimits
        };
    }

    /// <summary>
    /// Mappe un modèle de domaine vers une entité EF Core.
    /// </summary>
    /// <param name="config">Configuration de domaine.</param>
    /// <returns>Entité EF Core.</returns>
    private static TenantRateLimitConfigurationEntity MapToEntity(TenantRateLimitConfiguration config)
    {
        var entity = TenantRateLimitConfigurationEntity.Create(
            config.TenantId,
            config.GlobalLimit.RequestsPerMinute,
            config.GlobalLimit.RequestsPerDay,
            config.GlobalLimit.TokensPerMinute,
            config.GlobalLimit.TokensPerDay,
            config.ApiKeyLimit.RequestsPerMinute,
            config.ApiKeyLimit.TokensPerMinute);

        foreach (var (path, limit) in config.EndpointLimits)
        {
            var endpointLimit = EndpointLimitEntity.Create(
                entity.Id,
                path,
                limit.RequestsPerMinute,
                limit.TokensPerMinute,
                limit.BurstCapacity);
            entity.EndpointLimits.Add(endpointLimit);
        }

        return entity;
    }
}
