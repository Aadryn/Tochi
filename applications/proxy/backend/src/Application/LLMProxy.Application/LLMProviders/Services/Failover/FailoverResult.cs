using LLMProxy.Domain.Entities;

namespace LLMProxy.Application.LLMProviders.Services.Failover;

/// <summary>
/// Résultat d'une opération avec failover.
/// </summary>
/// <typeparam name="T">Type du résultat.</typeparam>
/// <remarks>
/// Contient le résultat de l'opération ainsi que toutes les tentatives effectuées,
/// permettant une analyse détaillée du processus de failover.
/// </remarks>
public sealed record FailoverResult<T>
{
    /// <summary>
    /// Indique si l'opération a réussi.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Résultat de l'opération (si succès).
    /// </summary>
    /// <remarks>
    /// Null si Success est false.
    /// </remarks>
    public T? Result { get; init; }

    /// <summary>
    /// Provider qui a réussi l'opération.
    /// </summary>
    /// <remarks>
    /// Null si aucun provider n'a réussi.
    /// </remarks>
    public ProviderType? SuccessfulProvider { get; init; }

    /// <summary>
    /// Liste des providers essayés avec leurs erreurs.
    /// </summary>
    /// <remarks>
    /// Contient toutes les tentatives, y compris celle qui a réussi (le cas échéant).
    /// </remarks>
    public required IReadOnlyList<FailoverAttempt> Attempts { get; init; }

    /// <summary>
    /// Durée totale du failover.
    /// </summary>
    /// <remarks>
    /// Temps écoulé depuis la première tentative jusqu'au succès ou à l'épuisement des providers.
    /// </remarks>
    public TimeSpan TotalDuration { get; init; }
}
