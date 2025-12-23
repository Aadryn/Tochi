using LLMProxy.Application.Common;
using LLMProxy.Application.Common.Interfaces;

namespace LLMProxy.Application.Tenants.Commands;

/// <summary>
/// Commande pour créer un nouveau tenant.
/// </summary>
/// <remarks>
/// Après création, invalide le cache de la liste de tous les tenants (ADR-042).
/// </remarks>
public record CreateTenantCommand : ICommand<TenantDto>, ICacheInvalidator
{
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public TenantSettingsDto? Settings { get; init; }

    /// <summary>
    /// Invalide la liste de tous les tenants car un nouveau a été ajouté.
    /// </summary>
    public IEnumerable<string> GetCacheKeysToInvalidate()
    {
        // Invalider toutes les queries GetAllTenantsQuery
        yield return "GetAllTenantsQuery:*";
    }
}
