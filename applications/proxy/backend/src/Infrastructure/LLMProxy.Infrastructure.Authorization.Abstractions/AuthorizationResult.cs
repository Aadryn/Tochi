namespace LLMProxy.Infrastructure.Authorization.Abstractions;

/// <summary>
/// Représente le résultat d'une vérification d'autorisation ReBAC.
/// </summary>
/// <remarks>
/// Cette classe est immuable et fournit des informations détaillées
/// sur le résultat de la vérification d'autorisation.
/// </remarks>
public sealed class AuthorizationResult
{
    /// <summary>
    /// Instance représentant une autorisation accordée.
    /// </summary>
    public static readonly AuthorizationResult Allowed = new(true);

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="AuthorizationResult"/>.
    /// </summary>
    /// <param name="isAllowed">Indique si l'accès est autorisé.</param>
    /// <param name="reason">Raison optionnelle du refus (si applicable).</param>
    private AuthorizationResult(bool isAllowed, string? reason = null)
    {
        IsAllowed = isAllowed;
        Reason = reason;
    }

    /// <summary>
    /// Indique si l'accès est autorisé.
    /// </summary>
    public bool IsAllowed { get; }

    /// <summary>
    /// Indique si l'accès est refusé (inverse de <see cref="IsAllowed"/>).
    /// </summary>
    public bool IsDenied => !IsAllowed;

    /// <summary>
    /// Raison du refus d'accès (null si autorisé).
    /// </summary>
    public string? Reason { get; }

    /// <summary>
    /// Crée un résultat de refus d'autorisation avec une raison.
    /// </summary>
    /// <param name="reason">Raison du refus.</param>
    /// <returns>Instance de <see cref="AuthorizationResult"/> représentant un refus.</returns>
    public static AuthorizationResult Denied(string? reason = null) => new(false, reason ?? "Accès refusé");

    /// <summary>
    /// Crée un résultat de refus indiquant que la relation n'existe pas.
    /// </summary>
    /// <param name="relation">Nom de la relation manquante.</param>
    /// <param name="objectType">Type d'objet concerné.</param>
    /// <param name="objectId">Identifiant de l'objet concerné.</param>
    /// <returns>Instance de <see cref="AuthorizationResult"/> avec raison détaillée.</returns>
    public static AuthorizationResult NoRelation(string relation, string objectType, string objectId)
        => new(false, $"L'utilisateur n'a pas la relation '{relation}' sur {objectType}:{objectId}");

    /// <summary>
    /// Crée un résultat de refus suite à une erreur technique.
    /// </summary>
    /// <param name="errorMessage">Message d'erreur technique.</param>
    /// <returns>Instance de <see cref="AuthorizationResult"/> avec raison technique.</returns>
    public static AuthorizationResult Error(string errorMessage)
        => new(false, $"Erreur d'autorisation: {errorMessage}");

    /// <summary>
    /// Opérateur implicite pour utiliser le résultat comme booléen.
    /// </summary>
    /// <param name="result">Résultat d'autorisation.</param>
    public static implicit operator bool(AuthorizationResult result) => result.IsAllowed;

    /// <inheritdoc />
    public override string ToString()
        => IsAllowed ? "Autorisé" : $"Refusé: {Reason}";
}
