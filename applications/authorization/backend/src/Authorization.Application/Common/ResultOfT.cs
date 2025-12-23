namespace Authorization.Application.Common;

/// <summary>
/// Résultat d'une opération d'application avec valeur de retour.
/// </summary>
/// <typeparam name="T">Type de la valeur de succès.</typeparam>
/// <remarks>
/// Représente soit un succès avec une valeur, soit un échec avec une erreur.
/// Permet une gestion des erreurs explicite sans exceptions.
/// </remarks>
public class Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    private Result(T value)
    {
        _value = value;
        _error = null;
        IsSuccess = true;
    }

    private Result(Error error)
    {
        _value = default;
        _error = error;
        IsSuccess = false;
    }

    /// <summary>
    /// Indique si l'opération a réussi.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indique si l'opération a échoué.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Valeur du résultat (si succès).
    /// </summary>
    /// <exception cref="InvalidOperationException">Si le résultat est un échec.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");

    /// <summary>
    /// Erreur du résultat (si échec).
    /// </summary>
    /// <exception cref="InvalidOperationException">Si le résultat est un succès.</exception>
    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access error of a successful result.");

    /// <summary>
    /// Crée un résultat de succès.
    /// </summary>
    /// <param name="value">Valeur du succès.</param>
    /// <returns>Résultat de succès contenant la valeur.</returns>
    public static Result<T> Success(T value) => new(value);

    /// <summary>
    /// Crée un résultat d'échec.
    /// </summary>
    /// <param name="error">Erreur du résultat.</param>
    /// <returns>Résultat d'échec contenant l'erreur.</returns>
    public static Result<T> Failure(Error error) => new(error);

    /// <summary>
    /// Crée un résultat d'échec avec code et message.
    /// </summary>
    /// <param name="code">Code d'erreur.</param>
    /// <param name="message">Message d'erreur.</param>
    /// <returns>Résultat d'échec.</returns>
    public static Result<T> Failure(string code, string message) =>
        new(new Error(code, message));

    /// <summary>
    /// Conversion implicite depuis une valeur.
    /// </summary>
    /// <param name="value">Valeur à convertir en résultat de succès.</param>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Conversion implicite depuis une erreur.
    /// </summary>
    /// <param name="error">Erreur à convertir en résultat d'échec.</param>
    public static implicit operator Result<T>(Error error) => Failure(error);
}
