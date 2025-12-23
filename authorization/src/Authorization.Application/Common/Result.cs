namespace Authorization.Application.Common;

/// <summary>
/// Résultat d'une opération d'application.
/// </summary>
/// <typeparam name="T">Type de la valeur de succès.</typeparam>
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
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");

    /// <summary>
    /// Erreur du résultat (si échec).
    /// </summary>
    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access error of a successful result.");

    /// <summary>
    /// Crée un résultat de succès.
    /// </summary>
    public static Result<T> Success(T value) => new(value);

    /// <summary>
    /// Crée un résultat d'échec.
    /// </summary>
    public static Result<T> Failure(Error error) => new(error);

    /// <summary>
    /// Crée un résultat d'échec avec code et message.
    /// </summary>
    public static Result<T> Failure(string code, string message) =>
        new(new Error(code, message));

    /// <summary>
    /// Conversion implicite depuis une valeur.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Conversion implicite depuis une erreur.
    /// </summary>
    public static implicit operator Result<T>(Error error) => Failure(error);
}

/// <summary>
/// Résultat sans valeur de retour.
/// </summary>
public class Result
{
    private readonly Error? _error;

    private Result()
    {
        _error = null;
        IsSuccess = true;
    }

    private Result(Error error)
    {
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
    /// Erreur du résultat (si échec).
    /// </summary>
    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access error of a successful result.");

    /// <summary>
    /// Instance de succès.
    /// </summary>
    public static Result Success() => new();

    /// <summary>
    /// Crée un résultat d'échec.
    /// </summary>
    public static Result Failure(Error error) => new(error);

    /// <summary>
    /// Crée un résultat d'échec avec code et message.
    /// </summary>
    public static Result Failure(string code, string message) =>
        new(new Error(code, message));

    /// <summary>
    /// Conversion implicite depuis une erreur.
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Représente une erreur applicative.
/// </summary>
/// <param name="Code">Code d'erreur.</param>
/// <param name="Message">Message d'erreur.</param>
public record Error(string Code, string Message)
{
    /// <summary>
    /// Erreurs de validation.
    /// </summary>
    public static Error Validation(string message) => new("VALIDATION_ERROR", message);

    /// <summary>
    /// Ressource non trouvée.
    /// </summary>
    public static Error NotFound(string resource, string id) =>
        new("NOT_FOUND", $"{resource} with id '{id}' was not found.");

    /// <summary>
    /// Accès non autorisé.
    /// </summary>
    public static Error Unauthorized(string message = "Access denied.") =>
        new("UNAUTHORIZED", message);

    /// <summary>
    /// Opération non autorisée.
    /// </summary>
    public static Error Forbidden(string message) => new("FORBIDDEN", message);

    /// <summary>
    /// Conflit (ressource existe déjà).
    /// </summary>
    public static Error Conflict(string message) => new("CONFLICT", message);

    /// <summary>
    /// Erreur interne.
    /// </summary>
    public static Error Internal(string message) => new("INTERNAL_ERROR", message);
}
