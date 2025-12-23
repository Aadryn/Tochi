using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.LLMProviders.Services.Selection;

/// <summary>
/// Critères de sélection de providers.
/// </summary>
/// <remarks>
/// Ces critères permettent de filtrer et ordonner les providers LLM disponibles
/// selon les besoins spécifiques de chaque requête.
/// </remarks>
public sealed record SelectionCriteria
{
    /// <summary>
    /// Providers préférés (ordonnés par préférence).
    /// </summary>
    /// <remarks>
    /// Si spécifié, seuls ces providers seront considérés (sauf s'ils sont dans ExcludedProviders).
    /// L'ordre dans la liste définit la préférence, le premier étant le plus préféré.
    /// </remarks>
    public IReadOnlyList<ProviderType>? PreferredProviders { get; init; }

    /// <summary>
    /// Capacités requises du modèle.
    /// </summary>
    /// <remarks>
    /// Filtre les providers en fonction des capacités minimales requises (streaming, function calling, etc.).
    /// </remarks>
    public ModelCapabilities? RequiredCapabilities { get; init; }

    /// <summary>
    /// Modèle requis (si un modèle spécifique est demandé).
    /// </summary>
    /// <remarks>
    /// Si spécifié, seuls les providers supportant ce modèle seront sélectionnés.
    /// </remarks>
    public string? RequiredModel { get; init; }

    /// <summary>
    /// Préférer le provider le moins cher.
    /// </summary>
    public bool PreferCheapest { get; init; }

    /// <summary>
    /// Préférer le provider le plus rapide.
    /// </summary>
    public bool PreferFastest { get; init; }

    /// <summary>
    /// Providers à exclure de la sélection.
    /// </summary>
    public IReadOnlyList<ProviderType>? ExcludedProviders { get; init; }

    /// <summary>
    /// Longueur minimale du contexte requis (nombre de tokens).
    /// </summary>
    /// <remarks>
    /// Filtre les providers en fonction de la fenêtre de contexte minimale requise.
    /// </remarks>
    public int? MinContextLength { get; init; }
}
