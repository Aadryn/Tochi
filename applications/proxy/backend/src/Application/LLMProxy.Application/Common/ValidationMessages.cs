namespace LLMProxy.Application.Common;

/// <summary>
/// Messages de validation standardisés pour FluentValidation.
/// </summary>
/// <remarks>
/// <para>
/// Cette classe centralise tous les messages de validation pour garantir la cohérence
/// et faciliter la maintenance et l'internationalisation.
/// </para>
/// <para>
/// Conforme aux ADR suivants :
/// - ADR-003 (DRY) : Élimination duplication messages
/// - ADR-019 (Convention over Configuration) : Messages standardisés
/// </para>
/// </remarks>
public static class ValidationMessages
{
    /// <summary>
    /// Message pour un champ requis.
    /// </summary>
    /// <param name="fieldName">Nom du champ.</param>
    /// <returns>Message formaté : "{fieldName} is required"</returns>
    public static string Required(string fieldName) => $"{fieldName} is required";

    /// <summary>
    /// Message pour une longueur maximale dépassée.
    /// </summary>
    /// <param name="fieldName">Nom du champ.</param>
    /// <param name="maxLength">Longueur maximale autorisée.</param>
    /// <returns>Message formaté : "{fieldName} must not exceed {maxLength} characters"</returns>
    public static string MaxLength(string fieldName, int maxLength) =>
        $"{fieldName} must not exceed {maxLength} characters";

    /// <summary>
    /// Message pour un format invalide.
    /// </summary>
    /// <param name="fieldName">Nom du champ.</param>
    /// <returns>Message formaté : "Invalid {fieldName} format"</returns>
    public static string InvalidFormat(string fieldName) => $"Invalid {fieldName} format";

    /// <summary>
    /// Message pour une valeur qui doit être positive.
    /// </summary>
    /// <param name="fieldName">Nom du champ.</param>
    /// <returns>Message formaté : "{fieldName} must be positive"</returns>
    public static string MustBePositive(string fieldName) => $"{fieldName} must be positive";

    /// <summary>
    /// Message pour une date qui doit être dans le futur.
    /// </summary>
    /// <param name="fieldName">Nom du champ.</param>
    /// <returns>Message formaté : "{fieldName} must be in the future"</returns>
    public static string MustBeFuture(string fieldName) => $"{fieldName} must be in the future";

    /// <summary>
    /// Message pour une valeur invalide parmi un ensemble.
    /// </summary>
    /// <param name="fieldName">Nom du champ.</param>
    /// <param name="validValues">Valeurs valides (optionnel).</param>
    /// <returns>Message formaté</returns>
    public static string InvalidValue(string fieldName, string? validValues = null) =>
        validValues != null
            ? $"Invalid {fieldName}. Valid values: {validValues}"
            : $"Invalid {fieldName}";
}
