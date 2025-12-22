namespace LLMProxy.Infrastructure.Redis.Idempotency;

/// <summary>
/// Représente une réponse HTTP cachée pour l'idempotence.
/// Permet de rejouer exactement la même réponse HTTP si une requête est exécutée plusieurs fois.
/// </summary>
/// <param name="StatusCode">Code de statut HTTP de la réponse (ex: 200, 201, 400).</param>
/// <param name="ContentType">Type de contenu de la réponse (ex: "application/json").</param>
/// <param name="Body">Corps de la réponse HTTP sous forme de chaîne de caractères.</param>
/// <param name="CreatedAt">Date et heure de création de la réponse cachée (UTC).</param>
public sealed record CachedResponse(
    int StatusCode,
    string ContentType,
    string Body,
    DateTime CreatedAt
);
