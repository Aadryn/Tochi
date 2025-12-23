namespace Authorization.Domain.Exceptions;

/// <summary>
/// Exception de base pour les erreurs du domaine d'autorisation.
/// </summary>
public abstract class AuthorizationDomainException : Exception
{
    /// <summary>
    /// Code d'erreur unique pour l'identification de l'exception.
    /// </summary>
    public abstract string ErrorCode { get; }

    /// <summary>
    /// Constructeur avec message.
    /// </summary>
    /// <param name="message">Message d'erreur.</param>
    protected AuthorizationDomainException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Constructeur avec message et exception interne.
    /// </summary>
    /// <param name="message">Message d'erreur.</param>
    /// <param name="innerException">Exception interne.</param>
    protected AuthorizationDomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
