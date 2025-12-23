namespace LLMProxy.Domain.Common;

/// <summary>
/// Représente le résultat d'une opération qui peut échouer sans retourner de valeur.
/// </summary>
/// <remarks>
/// <para>
/// Le pattern Result permet de gérer les erreurs de manière explicite et type-safe,
/// en évitant l'utilisation d'exceptions pour les cas métier prévisibles.
/// </para>
/// <para>
/// Utilisez ce type pour les opérations qui :
/// <list type="bullet">
/// <item><description>Peuvent échouer de manière prévisible (validation, règle métier)</description></item>
/// <item><description>Ne retournent pas de valeur en cas de succès</description></item>
/// <item><description>Nécessitent une gestion explicite des erreurs</description></item>
/// </list>
/// </para>
/// <para>
/// Les exceptions doivent être réservées aux cas vraiment exceptionnels :
/// <list type="bullet">
/// <item><description>Bugs de programmation (ArgumentNullException, InvalidOperationException)</description></item>
/// <item><description>Erreurs d'infrastructure (base de données inaccessible, réseau)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public async Task&lt;Result&gt; DeleteUserAsync(Guid id, CancellationToken ct)
/// {
///     var user = await _repository.FindAsync(id, ct);
///     if (user is null)
///         return Error.User.NotFound(id);
///     
///     await _repository.DeleteAsync(user, ct);
///     return Result.Success();
/// }
/// </code>
/// </example>
public sealed class Result
{
    /// <summary>
    /// Indique si l'opération a réussi.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indique si l'opération a échoué.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// L'erreur en cas d'échec. Vaut <see cref="Error.None"/> en cas de succès.
    /// </summary>
    public Error Error { get; }

    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Crée un résultat de succès.
    /// </summary>
    /// <returns>Un résultat indiquant le succès de l'opération.</returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Crée un résultat d'échec avec l'erreur spécifiée.
    /// </summary>
    /// <param name="error">L'erreur à associer au résultat.</param>
    /// <returns>Un résultat indiquant l'échec de l'opération.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Conversion implicite depuis une erreur vers un résultat d'échec.
    /// </summary>
    /// <param name="error">L'erreur à convertir.</param>
    /// <remarks>
    /// Permet d'écrire simplement :
    /// <code>
    /// if (user is null)
    ///     return Error.User.NotFound(id); // Conversion implicite vers Result
    /// </code>
    /// </remarks>
    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>
    /// Exécute une fonction selon le résultat (succès ou échec).
    /// </summary>
    /// <typeparam name="T">Le type de la valeur retournée.</typeparam>
    /// <param name="onSuccess">Fonction à exécuter en cas de succès.</param>
    /// <param name="onFailure">Fonction à exécuter en cas d'échec.</param>
    /// <returns>Le résultat de la fonction exécutée.</returns>
    /// <example>
    /// <code>
    /// var result = await DeleteUserAsync(id, ct);
    /// return result.Match(
    ///     onSuccess: () => NoContent(),
    ///     onFailure: error => NotFound(error));
    /// </code>
    /// </example>
    public T Match<T>(Func<T> onSuccess, Func<Error, T> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Error);
    }

    /// <summary>
    /// Exécute une action selon le résultat (succès ou échec).
    /// </summary>
    /// <param name="onSuccess">Action à exécuter en cas de succès.</param>
    /// <param name="onFailure">Action à exécuter en cas d'échec.</param>
    public void Match(Action onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
            onSuccess();
        else
            onFailure(Error);
    }

    /// <summary>
    /// [OBSOLÈTE - Migration] Crée un résultat d'échec depuis un message d'erreur texte.
    /// </summary>
    /// <param name="message">Le message d'erreur.</param>
    /// <returns>Un résultat d'échec.</returns>
    /// <remarks>
    /// ⚠️ Cette méthode est obsolète et existe uniquement pour la compatibilité 
    /// lors de la migration vers ADR-023. Utilisez <see cref="Failure(Error)"/> avec un Error typé.
    /// </remarks>
    [Obsolete("Utilisez Result.Failure(Error) avec un Error typé. Cette méthode sera supprimée dans une version future.")]
    public static Result Failure(string message) => 
        Failure(new Error("Legacy.Error", message));

    /// <summary>
    /// [OBSOLÈTE - Migration] Crée un résultat de succès avec valeur.
    /// </summary>
    [Obsolete("Utilisez Result<T>.Success(value) ou la conversion implicite. Cette méthode sera supprimée dans une version future.")]
    public static Result<T> Success<T>(T value) => 
        Result<T>.Success(value);

    /// <summary>
    /// [OBSOLÈTE - Migration] Crée un résultat d'échec avec valeur.
    /// </summary>
    [Obsolete("Utilisez Result<T>.Failure(Error) avec un Error typé. Cette méthode sera supprimée dans une version future.")]
    public static Result<T> Failure<T>(string message) => 
        Result<T>.Failure(new Error("Legacy.Error", message));
}
