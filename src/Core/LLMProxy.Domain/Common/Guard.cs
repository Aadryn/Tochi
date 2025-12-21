namespace LLMProxy.Domain.Common;

/// <summary>
/// Classe utilitaire fournissant des méthodes de validation (Guard Clauses) pour le domaine.
/// </summary>
/// <remarks>
/// <para>
/// Cette classe centralise les validations courantes pour garantir une détection précoce des erreurs
/// et éviter la duplication de code de validation dans les entités du domaine.
/// </para>
/// <para>
/// Conforme aux ADR suivants :
/// - ADR-009 (Fail Fast) : Détection des erreurs au plus tôt
/// - ADR-018 (Guard Clauses) : Validation en début de méthode
/// - ADR-003 (DRY) : Élimination duplication validations
/// - ADR-006 (Onion Architecture) : Domain ne dépend de rien
/// </para>
/// </remarks>
public static class Guard
{
    /// <summary>
    /// Vérifie qu'une chaîne de caractères n'est ni null, ni vide, ni composée uniquement d'espaces.
    /// </summary>
    /// <param name="value">Chaîne à valider.</param>
    /// <param name="paramName">Nom du paramètre (utilisé dans l'exception).</param>
    /// <param name="message">Message d'erreur personnalisé optionnel.</param>
    /// <exception cref="ArgumentException">Levée si <paramref name="value"/> est null, vide ou whitespace.</exception>
    /// <example>
    /// <code>
    /// public void UpdateName(string name)
    /// {
    ///     Guard.AgainstNullOrWhiteSpace(name, nameof(name), "User name cannot be empty.");
    ///     Name = name;
    /// }
    /// </code>
    /// </example>
    public static void AgainstNullOrWhiteSpace(string? value, string paramName, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                message ?? $"Le paramètre '{paramName}' ne peut pas être null, vide ou composé uniquement d'espaces.",
                paramName);
        }
    }

    /// <summary>
    /// Vérifie qu'un GUID n'est pas vide (Guid.Empty).
    /// </summary>
    /// <param name="value">GUID à valider.</param>
    /// <param name="paramName">Nom du paramètre (utilisé dans l'exception).</param>
    /// <param name="message">Message d'erreur personnalisé optionnel.</param>
    /// <exception cref="ArgumentException">Levée si <paramref name="value"/> est Guid.Empty.</exception>
    /// <remarks>
    /// Cette validation est critique pour éviter des requêtes en base de données avec des identifiants invalides.
    /// </remarks>
    /// <example>
    /// <code>
    /// public static Result&lt;Entity&gt; Create(Guid tenantId, ...)
    /// {
    ///     Guard.AgainstEmptyGuid(tenantId, nameof(tenantId), "Invalid tenant ID.");
    ///     // ...
    /// }
    /// </code>
    /// </example>
    public static void AgainstEmptyGuid(Guid value, string paramName, string? message = null)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                message ?? $"Le paramètre '{paramName}' ne peut pas être un GUID vide (Guid.Empty).",
                paramName);
        }
    }
}
