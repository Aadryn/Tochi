using System.Text.Json;

namespace LLMProxy.Infrastructure.Redis.Common;

/// <summary>
/// Factory pour créer des options JSON standardisées dans l'application (ADR-003 DRY).
/// </summary>
/// <remarks>
/// Centralise la configuration de <see cref="JsonSerializerOptions"/> pour garantir
/// la cohérence de la sérialisation JSON dans toute l'application (camelCase, DefaultIgnoreCondition, etc.).
/// </remarks>
public static class JsonSerializerOptionsFactory
{
    /// <summary>
    /// Crée une instance de <see cref="JsonSerializerOptions"/> avec la configuration par défaut.
    /// </summary>
    /// <returns>Options JSON configurées (camelCase).</returns>
    /// <remarks>
    /// Configuration appliquée :
    /// - PropertyNamingPolicy : camelCase
    /// </remarks>
    public static JsonSerializerOptions CreateDefault() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
