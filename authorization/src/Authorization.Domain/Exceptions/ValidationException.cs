namespace Authorization.Domain.Exceptions;

/// <summary>
/// Exception levée lors d'une erreur de validation de données.
/// </summary>
public sealed class ValidationException : AuthorizationDomainException
{
    /// <inheritdoc />
    public override string ErrorCode => "VALIDATION_ERROR";

    /// <summary>
    /// Nom de la propriété invalide.
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>
    /// Valeur invalide.
    /// </summary>
    public object? AttemptedValue { get; }

    /// <summary>
    /// Constructeur avec message.
    /// </summary>
    /// <param name="message">Message d'erreur.</param>
    public ValidationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Constructeur avec propriété.
    /// </summary>
    /// <param name="propertyName">Nom de la propriété.</param>
    /// <param name="message">Message d'erreur.</param>
    public ValidationException(string propertyName, string message)
        : base($"{propertyName}: {message}")
    {
        PropertyName = propertyName;
    }

    /// <summary>
    /// Constructeur complet.
    /// </summary>
    /// <param name="propertyName">Nom de la propriété.</param>
    /// <param name="attemptedValue">Valeur tentée.</param>
    /// <param name="message">Message d'erreur.</param>
    public ValidationException(string propertyName, object? attemptedValue, string message)
        : base($"{propertyName}: {message}")
    {
        PropertyName = propertyName;
        AttemptedValue = attemptedValue;
    }
}
