using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Service de comptage des tokens LLM (Port en Architecture Hexagonale)
/// Permet d'estimer et de calculer la consommation de tokens avant/après les requêtes
/// </summary>
public interface ITokenCounterService
{
    /// <summary>
    /// Estime le nombre de tokens pour un texte donné avec un tokenizer local
    /// Utilisé pour la pré-validation et l'estimation des coûts
    /// </summary>
    int EstimateTokens(string text, string model);
    
    /// <summary>
    /// Parse le nombre réel de tokens depuis la réponse du fournisseur LLM
    /// Retourne (tokens d'entrée, tokens de sortie) pour la facturation précise
    /// </summary>
    (long inputTokens, long outputTokens) ParseTokensFromResponse(string responseBody, ProviderType providerType);
}
