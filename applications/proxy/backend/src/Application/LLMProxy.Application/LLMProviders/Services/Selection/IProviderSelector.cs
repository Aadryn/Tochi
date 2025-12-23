using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.LLMProviders.Services.Selection;

/// <summary>
/// Service de sélection de providers LLM.
/// </summary>
/// <remarks>
/// Ce service permet de sélectionner le ou les meilleurs providers en fonction de critères spécifiques,
/// tels que les préférences utilisateur, les capacités requises, et les métriques de performance.
/// </remarks>
public interface IProviderSelector
{
    /// <summary>
    /// Sélectionne une liste ordonnée de types de providers selon des critères donnés.
    /// </summary>
    /// <param name="criteria">Critères de sélection (optionnel).</param>
    /// <returns>Liste ordonnée de types de providers, du plus approprié au moins approprié.</returns>
    /// <remarks>
    /// Si aucun critère n'est fourni, retourne tous les providers disponibles dans un ordre par défaut
    /// basé sur la fiabilité générale.
    /// </remarks>
    IReadOnlyList<ProviderType> Select(SelectionCriteria? criteria = null);

    /// <summary>
    /// Sélectionne le meilleur provider client parmi une liste de providers disponibles.
    /// </summary>
    /// <param name="request">Requête LLM pour laquelle sélectionner un provider.</param>
    /// <param name="availableProviders">Liste des providers clients disponibles.</param>
    /// <param name="criteria">Critères de sélection (optionnel).</param>
    /// <returns>Le meilleur provider client, ou null si aucun n'est disponible.</returns>
    /// <remarks>
    /// Utilise un système de scoring pour déterminer le meilleur provider en fonction des critères
    /// et des métriques de performance (si disponibles).
    /// </remarks>
    ILLMProviderClient? SelectBestProvider(
        LLMRequest request,
        IReadOnlyList<ILLMProviderClient> availableProviders,
        SelectionCriteria? criteria = null);

    /// <summary>
    /// Sélectionne et ordonne tous les providers clients selon leur score.
    /// </summary>
    /// <param name="request">Requête LLM pour laquelle ordonner les providers.</param>
    /// <param name="availableProviders">Liste des providers clients disponibles.</param>
    /// <param name="criteria">Critères de sélection (optionnel).</param>
    /// <returns>Liste ordonnée de providers clients, du meilleur au moins bon.</returns>
    /// <remarks>
    /// Cette méthode permet d'obtenir une liste complète de providers ordonnés par score,
    /// utile pour implémenter un mécanisme de failover.
    /// </remarks>
    IReadOnlyList<ILLMProviderClient> SelectProvidersOrdered(
        LLMRequest request,
        IReadOnlyList<ILLMProviderClient> availableProviders,
        SelectionCriteria? criteria = null);
}
