namespace LLMProxy.Application.Authorization;

/// <summary>
/// Exception levée lorsqu'un utilisateur n'a pas les permissions requises.
/// </summary>
[Serializable]
public class UnauthorizedException : Exception
{
    /// <summary>
    /// Initialise une nouvelle instance de <see cref="UnauthorizedException"/>.
    /// </summary>
    public UnauthorizedException()
        : base("Accès non autorisé")
    {
    }

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="UnauthorizedException"/> avec un message.
    /// </summary>
    /// <param name="message">Message d'erreur.</param>
    public UnauthorizedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="UnauthorizedException"/> avec un message et une exception interne.
    /// </summary>
    /// <param name="message">Message d'erreur.</param>
    /// <param name="innerException">Exception interne.</param>
    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Crée une exception d'autorisation avec les détails de la vérification échouée.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="relation">Relation requise.</param>
    /// <param name="objectType">Type d'objet.</param>
    /// <param name="objectId">Identifiant de l'objet.</param>
    /// <returns>Instance configurée de <see cref="UnauthorizedException"/>.</returns>
    public static UnauthorizedException ForPermission(
        string userId,
        string relation,
        string objectType,
        string objectId)
    {
        return new UnauthorizedException(
            $"L'utilisateur '{userId}' n'a pas la permission '{relation}' sur {objectType}:{objectId}");
    }
}
