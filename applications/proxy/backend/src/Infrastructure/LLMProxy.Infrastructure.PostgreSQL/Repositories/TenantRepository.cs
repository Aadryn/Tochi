using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Extensions;
using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository pour l'agrégat Tenant (Adaptateur dans l'architecture hexagonale).
/// </summary>
/// <remarks>
/// <para>
/// Repository spécialisé pour les tenants avec support du Result Pattern (ADR-023).
/// Hérite de <see cref="RepositoryBase{Tenant}"/> pour les opérations CRUD standard
/// et ajoute des méthodes métier spécifiques (recherche par slug, etc.).
/// </para>
/// <para>
/// <strong>Null Object Pattern (ADR-026) :</strong>
/// Conserve les méthodes GetByXOrDefaultAsync qui retournent <see cref="NullTenant.Instance"/>
/// au lieu de null pour éliminer les null checks.
/// </para>
/// </remarks>
public class TenantRepository : RepositoryBase<Tenant>, ITenantRepository
{
    /// <summary>
    /// Initialise une nouvelle instance du repository Tenant.
    /// </summary>
    /// <param name="context">Contexte de base de données EF Core.</param>
    /// <param name="logger">Logger pour enregistrer les opérations et erreurs.</param>
    public TenantRepository(LLMProxyDbContext context, ILogger<TenantRepository> logger) 
        : base(context, logger)
    {
    }

    /// <summary>
    /// Récupère un tenant par son slug unique.
    /// </summary>
    /// <param name="slug">Slug du tenant (identifiant URL-safe).</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result{T}"/> contenant le tenant si trouvé.
    /// Retourne <c>Error.Tenant.NotFound</c> si aucun tenant ne correspond.
    /// </returns>
    /// <remarks>
    /// Le slug est normalisé avant la recherche (lowercase, caractères spéciaux supprimés).
    /// </remarks>
    public async Task<Result<Tenant>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedSlug = slug.NormalizeSlug();
            var tenant = await DbSet.FirstOrDefaultAsync(t => t.Slug == normalizedSlug, cancellationToken);

            if (tenant is null)
            {
                Logger.LogWarning("Tenant avec slug '{Slug}' non trouvé", slug);
                return Error.Tenant.NotFound(Guid.Empty);
            }

            return Result<Tenant>.Success(tenant);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Opération GetBySlugAsync annulée pour slug '{Slug}'", slug);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur lors de la récupération du tenant par slug '{Slug}'", slug);
            return Error.Database.AccessError("GetBySlugAsync", ex.Message);
        }
    }

    /// <summary>
    /// Récupère un tenant par son identifiant ou retourne le tenant par défaut.
    /// Conforme à ADR-026 (Null Object Pattern).
    /// </summary>
    /// <param name="id">Identifiant unique du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Le tenant trouvé ou <see cref="NullTenant.Instance"/>.</returns>
    /// <remarks>
    /// <para>
    /// Élimine les null checks dans le code appelant.
    /// Au lieu de vérifier <c>if (tenant == null)</c>, vérifier <c>if (!tenant.IsActive)</c>.
    /// </para>
    /// <para>
    /// <strong>Note :</strong> Cette méthode conserve l'ancien comportement pour compatibilité.
    /// Pour la nouvelle API Result Pattern, utiliser <see cref="GetByIdAsync"/>.
    /// </para>
    /// </remarks>
    public async Task<Tenant> GetByIdOrDefaultAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await DbSet.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        return tenant ?? NullTenant.Instance;
    }

    /// <summary>
    /// Récupère un tenant par son slug ou retourne le tenant par défaut.
    /// Conforme à ADR-026 (Null Object Pattern).
    /// </summary>
    /// <param name="slug">Slug unique du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Le tenant trouvé ou <see cref="NullTenant.Instance"/>.</returns>
    /// <remarks>
    /// <para>
    /// Élimine les null checks dans le code appelant.
    /// Au lieu de vérifier <c>if (tenant == null)</c>, vérifier <c>if (!tenant.IsActive)</c>.
    /// </para>
    /// <para>
    /// <strong>Note :</strong> Cette méthode conserve l'ancien comportement pour compatibilité.
    /// Pour la nouvelle API Result Pattern, utiliser <see cref="GetBySlugAsync"/>.
    /// </para>
    /// </remarks>
    public async Task<Tenant> GetBySlugOrDefaultAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.NormalizeSlug();
        var tenant = await DbSet.FirstOrDefaultAsync(t => t.Slug == normalizedSlug, cancellationToken);
        return tenant ?? NullTenant.Instance;
    }

    /// <summary>
    /// Récupère tous les tenants avec filtrage optionnel par statut actif/inactif.
    /// </summary>
    /// <param name="includeInactive">Si <c>true</c>, inclut les tenants inactifs. Par défaut <c>false</c>.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result{T}"/> contenant la collection de tenants filtrés.
    /// </returns>
    public override async Task<Result<IReadOnlyList<Tenant>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetAllAsync(includeInactive: false, cancellationToken);
    }

    /// <summary>
    /// Récupère tous les tenants avec filtrage optionnel par statut actif/inactif.
    /// </summary>
    /// <param name="includeInactive">Si <c>true</c>, inclut les tenants inactifs. Par défaut <c>false</c>.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result{T}"/> contenant la collection de tenants filtrés.
    /// </returns>
    public async Task<Result<IReadOnlyList<Tenant>>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = DbSet.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive);
            }

            var tenants = await query.ToListAsync(cancellationToken);

            Logger.LogDebug("Récupéré {Count} tenants (includeInactive: {IncludeInactive})", 
                tenants.Count, includeInactive);

            return Result<IReadOnlyList<Tenant>>.Success(tenants.AsReadOnly());
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Opération GetAllAsync annulée");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur lors de la récupération de tous les tenants");
            return Error.Database.AccessError("GetAllAsync", ex.Message);
        }
    }

    /// <summary>
    /// Vérifie si un slug est déjà utilisé par un tenant existant.
    /// </summary>
    /// <param name="slug">Slug à vérifier.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération asynchrone.</param>
    /// <returns>
    /// <see cref="Result{T}"/> contenant <c>true</c> si le slug existe, <c>false</c> sinon.
    /// </returns>
    /// <remarks>
    /// Le slug est normalisé avant la vérification pour garantir la cohérence.
    /// </remarks>
    public async Task<Result<bool>> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedSlug = slug.NormalizeSlug();
            var exists = await DbSet.AnyAsync(t => t.Slug == normalizedSlug, cancellationToken);

            Logger.LogDebug("Slug '{Slug}' existe: {Exists}", slug, exists);

            return Result<bool>.Success(exists);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Opération SlugExistsAsync annulée pour slug '{Slug}'", slug);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur lors de la vérification d'existence du slug '{Slug}'", slug);
            return Error.Database.AccessError("SlugExistsAsync", ex.Message);
        }
    }
}
