using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Service de comptage des tokens LLM (Port en Architecture Hexagonale).
/// Permet d'estimer et de calculer la consommation de tokens avant/après les requêtes.
/// </summary>
public interface ITokenCounterService
{
    /// <summary>
    /// Estime le nombre de tokens pour un texte donné avec un tokenizer local de manière asynchrone.
    /// Utilisé pour la pré-validation et l'estimation des coûts.
    /// </summary>
    /// <param name="text">Texte à analyser.</param>
    /// <param name="model">Nom du modèle pour déterminer l'encodage.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Nombre estimé de tokens.</returns>
    Task<int> EstimateTokensAsync(string text, string model, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Parse le nombre réel de tokens depuis la réponse du fournisseur LLM.
    /// Retourne (tokens d'entrée, tokens de sortie) pour la facturation précise.
    /// </summary>
    /// <param name="responseBody">Corps de la réponse JSON.</param>
    /// <param name="providerType">Type de provider pour adapter le parsing.</param>
    /// <returns>Tuple (inputTokens, outputTokens).</returns>
    (long inputTokens, long outputTokens) ParseTokensFromResponse(string responseBody, ProviderType providerType);
}
