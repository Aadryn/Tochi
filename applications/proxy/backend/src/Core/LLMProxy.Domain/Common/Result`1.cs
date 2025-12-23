namespace LLMProxy.Domain.Common;

/// <summary>
/// Représente le résultat d'une opération qui peut échouer en retournant une valeur de type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">Le type de la valeur retournée en cas de succès.</typeparam>
/// <remarks>
/// <para>
/// Ce type est utilisé pour les opérations qui retournent une valeur en cas de succès
/// et une erreur en cas d'échec, sans utiliser d'exceptions pour le contrôle de flux.
/// </para>
/// <para>
/// Avantages du Result Pattern :
/// <list type="bullet">
/// <item><description><strong>Performance</strong> : Évite le coût des exceptions (~85% plus rapide)</description></item>
/// <item><description><strong>Explicite</strong> : Le type indique que l'opération peut échouer</description></item>
/// <item><description><strong>Type-safe</strong> : Le compilateur force à gérer les erreurs</description></item>
/// <item><description><strong>Composition</strong> : Chaînage facile avec Bind/Map/Tap</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public async Task&lt;Result&lt;User&gt;&gt; GetByIdAsync(Guid id, CancellationToken ct)
/// {
///     var user = await _repository.FindAsync(id, ct);
///     if (user is null)
///         return Error.User.NotFound(id);
///     
///     return user; // Conversion implicite depuis User vers Result&lt;User&gt;
/// }
/// </code>
/// </example>
public sealed class Result<T>
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
    /// La valeur retournée en cas de succès. Vaut <c>default!</c> en cas d'échec.
    /// </summary>
    /// <remarks>
    /// ⚠️ Accéder à cette propriété quand <see cref="IsFailure"/> est <c>true</c>
    /// peut retourner une valeur par défaut. Utilisez toujours <see cref="Match{TOut}"/> 
    /// ou vérifiez <see cref="IsSuccess"/> avant d'accéder à <see cref="Value"/>.
    /// </remarks>
    public T Value { get; }

    /// <summary>
    /// L'erreur en cas d'échec. Vaut <see cref="Error.None"/> en cas de succès.
    /// </summary>
    public Error Error { get; }

    private Result(bool isSuccess, T value, Error error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>
    /// Crée un résultat de succès avec la valeur spécifiée.
    /// </summary>
    /// <param name="value">La valeur à retourner.</param>
    /// <returns>Un résultat indiquant le succès avec la valeur.</returns>
    public static Result<T> Success(T value) => new(true, value, Error.None);

    /// <summary>
    /// Crée un résultat d'échec avec l'erreur spécifiée.
    /// </summary>
    /// <param name="error">L'erreur à associer au résultat.</param>
    /// <returns>Un résultat indiquant l'échec avec l'erreur.</returns>
    public static Result<T> Failure(Error error) => new(false, default!, error);

    /// <summary>
    /// Conversion implicite depuis une valeur vers un résultat de succès.
    /// </summary>
    /// <param name="value">La valeur à convertir.</param>
    /// <remarks>
    /// Permet d'écrire simplement :
    /// <code>
    /// return user; // Au lieu de Result&lt;User&gt;.Success(user)
    /// </code>
    /// </remarks>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Conversion implicite depuis une erreur vers un résultat d'échec.
    /// </summary>
    /// <param name="error">L'erreur à convertir.</param>
    /// <remarks>
    /// Permet d'écrire simplement :
    /// <code>
    /// return Error.User.NotFound(id); // Au lieu de Result&lt;User&gt;.Failure(error)
    /// </code>
    /// </remarks>
    public static implicit operator Result<T>(Error error) => Failure(error);

    /// <summary>
    /// Exécute une fonction selon le résultat (succès ou échec).
    /// </summary>
    /// <typeparam name="TOut">Le type de la valeur retournée.</typeparam>
    /// <param name="onSuccess">Fonction à exécuter en cas de succès.</param>
    /// <param name="onFailure">Fonction à exécuter en cas d'échec.</param>
    /// <returns>Le résultat de la fonction exécutée.</returns>
    /// <example>
    /// <code>
    /// var result = await GetUserAsync(id, ct);
    /// return result.Match(
    ///     onSuccess: user => Ok(user.ToDto()),
    ///     onFailure: error => error.Code switch
    ///     {
    ///         "User.NotFound" => NotFound(error),
    ///         _ => BadRequest(error)
    ///     });
    /// </code>
    /// </example>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }

    /// <summary>
    /// Exécute une action selon le résultat (succès ou échec).
    /// </summary>
    /// <param name="onSuccess">Action à exécuter en cas de succès.</param>
    /// <param name="onFailure">Action à exécuter en cas d'échec.</param>
    public void Match(Action<T> onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
            onSuccess(Value);
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
    [Obsolete("Utilisez Result<T>.Failure(Error) avec un Error typé. Cette méthode sera supprimée dans une version future.")]
    public static Result<T> Failure(string message) => 
        Failure(new Error("Legacy.Error", message));

    /// <summary>
    /// Accès legacy à l'erreur sous forme de string (pour compatibilité).
    /// </summary>
    [Obsolete("Utilisez la propriété Error (de type Error) directement.")]
    public string? ErrorMessage => IsFailure ? Error.Message : null;
}
