namespace LLMProxy.Application.Common.Interfaces;

/// <summary>
/// Marker interface pour indiquer qu'une Command doit invalider le cache après exécution.
/// Conforme à ADR-042 (Distributed Cache Strategy).
/// </summary>
/// <remarks>
/// Les Commands implémentant cette interface doivent définir quelles clés de cache invalider
/// pour maintenir la cohérence entre le cache et la base de données.
/// 
/// UTILISATION TYPIQUE :
/// - CreateTenantCommand → Invalider liste tenants
/// - UpdateTenantCommand → Invalider tenant spécifique + liste
/// - DeleteTenantCommand → Invalider tenant spécifique + liste
/// </remarks>
public interface ICacheInvalidator
{
    /// <summary>
    /// Retourne les clés de cache à invalider après exécution de la Command.
    /// </summary>
    /// <remarks>
    /// Patterns supportés :
    /// - Clé exacte : "GetTenantByIdQuery:abc123"
    /// - Pattern wildcard : "GetTenantByIdQuery:*" (invalide toutes les queries GetTenantById)
    /// - Pattern global : "GetAllTenantsQuery:*" (invalide toutes les listes de tenants)
    /// 
    /// ATTENTION : Les wildcards (*) peuvent impacter les performances si trop de clés.
    /// Préférer l'invalidation ciblée quand possible.
    /// </remarks>
    /// <returns>Liste de clés ou patterns à invalider</returns>
    IEnumerable<string> GetCacheKeysToInvalidate();
}
