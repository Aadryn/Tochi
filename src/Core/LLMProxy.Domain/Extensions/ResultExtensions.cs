namespace LLMProxy.Domain.Extensions;

using LLMProxy.Domain.Common;

/// <summary>
/// Extensions pour le chaînage de <see cref="Result"/> et <see cref="Result{T}"/> (Railway-Oriented Programming).
/// </summary>
/// <remarks>
/// <para>
/// Ces extensions permettent de composer des opérations qui peuvent échouer
/// de manière fluide et lisible, en évitant les pyramides de if/else.
/// </para>
/// <para>
/// Le pattern "Railway-Oriented" visualise le flux comme deux rails :
/// <list type="bullet">
/// <item><description><strong>Rail de succès</strong> : Continue l'exécution si tout va bien</description></item>
/// <item><description><strong>Rail d'échec</strong> : Court-circuite et propage l'erreur</description></item>
/// </list>
/// </para>
/// </remarks>
public static class ResultExtensions
{
    /// <summary>
    /// Exécute une fonction qui retourne un <see cref="Result{TOut}"/> si le résultat initial est un succès.
    /// En cas d'échec, propage l'erreur sans exécuter la fonction.
    /// </summary>
    /// <typeparam name="TIn">Le type de la valeur d'entrée.</typeparam>
    /// <typeparam name="TOut">Le type de la valeur de sortie.</typeparam>
    /// <param name="resultTask">Le résultat initial (asynchrone).</param>
    /// <param name="func">La fonction à exécuter sur la valeur si succès.</param>
    /// <returns>Le résultat de la fonction ou l'erreur initiale.</returns>
    /// <remarks>
    /// <para>
    /// Bind permet de chaîner des opérations qui retournent un Result.
    /// C'est l'équivalent de "flatMap" ou "andThen" dans d'autres langages.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// return await GetUserAsync(id, ct)
    ///     .BindAsync(user => ValidateUserAsync(user, ct))
    ///     .BindAsync(user => SendEmailAsync(user, ct));
    /// </code>
    /// </example>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> func)
    {
        var result = await resultTask;

        if (result.IsFailure)
            return Result<TOut>.Failure(result.Error);

        return await func(result.Value);
    }

    /// <summary>
    /// Version synchrone de <see cref="BindAsync{TIn,TOut}(Task{Result{TIn}},Func{TIn,Task{Result{TOut}}})"/>.
    /// </summary>
    public static async Task<Result<TOut>> Bind<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Result<TOut>> func)
    {
        var result = await resultTask;

        if (result.IsFailure)
            return Result<TOut>.Failure(result.Error);

        return func(result.Value);
    }

    /// <summary>
    /// Transforme la valeur d'un résultat en appliquant une fonction si succès.
    /// En cas d'échec, propage l'erreur sans exécuter la fonction.
    /// </summary>
    /// <typeparam name="TIn">Le type de la valeur d'entrée.</typeparam>
    /// <typeparam name="TOut">Le type de la valeur de sortie.</typeparam>
    /// <param name="resultTask">Le résultat initial (asynchrone).</param>
    /// <param name="mapper">La fonction de transformation.</param>
    /// <returns>Le résultat avec la valeur transformée ou l'erreur initiale.</returns>
    /// <remarks>
    /// <para>
    /// Map permet de transformer la valeur sans changer le contexte Result.
    /// C'est l'équivalent de "map" ou "select" dans d'autres langages.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// return await GetUserAsync(id, ct)
    ///     .MapAsync(user => user.ToDto());
    /// </code>
    /// </example>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> mapper)
    {
        var result = await resultTask;

        if (result.IsFailure)
            return Result<TOut>.Failure(result.Error);

        return Result<TOut>.Success(mapper(result.Value));
    }

    /// <summary>
    /// Version asynchrone de Map où la transformation est asynchrone.
    /// </summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> mapper)
    {
        var result = await resultTask;

        if (result.IsFailure)
            return Result<TOut>.Failure(result.Error);

        var value = await mapper(result.Value);
        return Result<TOut>.Success(value);
    }

    /// <summary>
    /// Exécute une action sur la valeur si succès, puis retourne le résultat inchangé.
    /// En cas d'échec, ne fait rien et propage l'erreur.
    /// </summary>
    /// <typeparam name="T">Le type de la valeur.</typeparam>
    /// <param name="resultTask">Le résultat initial (asynchrone).</param>
    /// <param name="action">L'action à exécuter sur la valeur (effet de bord).</param>
    /// <returns>Le résultat initial inchangé.</returns>
    /// <remarks>
    /// <para>
    /// Tap permet d'exécuter des effets de bord (logging, notifications)
    /// sans modifier le résultat. Utile pour le chaînage.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// return await CreateUserAsync(command, ct)
    ///     .TapAsync(user => _logger.UserCreated(user.Id, user.Email, user.TenantId))
    ///     .TapAsync(user => SendWelcomeEmailAsync(user, ct));
    /// </code>
    /// </example>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task> action)
    {
        var result = await resultTask;

        if (result.IsSuccess)
            await action(result.Value);

        return result;
    }

    /// <summary>
    /// Version synchrone de <see cref="TapAsync{T}(Task{Result{T}},Func{T,Task})"/>.
    /// </summary>
    public static async Task<Result<T>> Tap<T>(
        this Task<Result<T>> resultTask,
        Action<T> action)
    {
        var result = await resultTask;

        if (result.IsSuccess)
            action(result.Value);

        return result;
    }

    /// <summary>
    /// Exécute une action sur l'erreur si échec, puis retourne le résultat inchangé.
    /// En cas de succès, ne fait rien.
    /// </summary>
    /// <typeparam name="T">Le type de la valeur.</typeparam>
    /// <param name="resultTask">Le résultat initial (asynchrone).</param>
    /// <param name="action">L'action à exécuter sur l'erreur.</param>
    /// <returns>Le résultat initial inchangé.</returns>
    /// <remarks>
    /// <para>
    /// TapError permet de logger ou traiter les erreurs sans modifier le résultat.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// return await GetUserAsync(id, ct)
    ///     .TapErrorAsync(error => _logger.LogWarning("User not found: {Error}", error.Message));
    /// </code>
    /// </example>
    public static async Task<Result<T>> TapErrorAsync<T>(
        this Task<Result<T>> resultTask,
        Func<Error, Task> action)
    {
        var result = await resultTask;

        if (result.IsFailure)
            await action(result.Error);

        return result;
    }

    /// <summary>
    /// Version synchrone de <see cref="TapErrorAsync{T}(Task{Result{T}},Func{Error,Task})"/>.
    /// </summary>
    public static async Task<Result<T>> TapError<T>(
        this Task<Result<T>> resultTask,
        Action<Error> action)
    {
        var result = await resultTask;

        if (result.IsFailure)
            action(result.Error);

        return result;
    }

    /// <summary>
    /// Combine plusieurs résultats en un seul.
    /// Si tous réussissent, retourne succès. Si un échoue, retourne la première erreur.
    /// </summary>
    /// <param name="results">Les résultats à combiner.</param>
    /// <returns>Succès si tous réussissent, sinon la première erreur.</returns>
    /// <example>
    /// <code>
    /// var results = new[]
    /// {
    ///     await ValidateEmailAsync(email, ct),
    ///     await ValidatePasswordAsync(password, ct),
    ///     await ValidateNameAsync(name, ct)
    /// };
    /// 
    /// var combined = Result.Combine(results);
    /// if (combined.IsFailure)
    ///     return combined.Error;
    /// </code>
    /// </example>
    public static Result Combine(params Result[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }

        return Result.Success();
    }

    /// <summary>
    /// Combine plusieurs résultats avec valeur en un tableau.
    /// Si tous réussissent, retourne un tableau des valeurs. Si un échoue, retourne la première erreur.
    /// </summary>
    /// <typeparam name="T">Le type des valeurs.</typeparam>
    /// <param name="results">Les résultats à combiner.</param>
    /// <returns>Succès avec tableau de valeurs si tous réussissent, sinon la première erreur.</returns>
    public static Result<T[]> Combine<T>(params Result<T>[] results)
    {
        var values = new List<T>(results.Length);

        foreach (var result in results)
        {
            if (result.IsFailure)
                return Result<T[]>.Failure(result.Error);

            values.Add(result.Value);
        }

        return Result<T[]>.Success(values.ToArray());
    }

    /// <summary>
    /// Convertit un <see cref="Result{T}"/> en <see cref="Result"/> (sans valeur).
    /// </summary>
    /// <typeparam name="T">Le type de la valeur à ignorer.</typeparam>
    /// <param name="resultTask">Le résultat initial avec valeur.</param>
    /// <returns>Le résultat sans valeur.</returns>
    /// <remarks>
    /// Utile quand on veut ignorer la valeur retournée et ne garder que le statut succès/échec.
    /// </remarks>
    public static async Task<Result> ToResult<T>(this Task<Result<T>> resultTask)
    {
        var result = await resultTask;

        return result.IsSuccess
            ? Result.Success()
            : Result.Failure(result.Error);
    }
}
