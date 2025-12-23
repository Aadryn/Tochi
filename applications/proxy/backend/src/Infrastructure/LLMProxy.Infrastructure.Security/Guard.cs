using Microsoft.AspNetCore.Http;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Classe utilitaire fournissant des méthodes de validation (Guard Clauses) pour implémenter le principe Fail Fast.
/// </summary>
/// <remarks>
/// <para>
/// Cette classe centralise toutes les validations courantes pour garantir une détection précoce des erreurs
/// et éviter la duplication de code de validation dans l'application.
/// </para>
/// <para>
/// Conforme aux ADR suivants :
/// - ADR-009 (Fail Fast) : Détection des erreurs au plus tôt
/// - ADR-018 (Guard Clauses) : Validation en début de méthode
/// - ADR-003 (DRY) : Élimination duplication validations
/// </para>
/// </remarks>
public static class Guard
{
    /// <summary>
    /// Vérifie qu'un paramètre n'est pas null.
    /// </summary>
    /// <typeparam name="T">Type du paramètre à valider.</typeparam>
    /// <param name="value">Valeur à valider.</param>
    /// <param name="paramName">Nom du paramètre (utilisé dans l'exception).</param>
    /// <param name="message">Message d'erreur personnalisé optionnel.</param>
    /// <exception cref="ArgumentNullException">Levée si <paramref name="value"/> est null.</exception>
    /// <example>
    /// <code>
    /// public void Process(ILogger logger)
    /// {
    ///     Guard.AgainstNull(logger, nameof(logger));
    ///     // logger est garanti non-null ici
    /// }
    /// </code>
    /// </example>
    public static void AgainstNull<T>(T? value, string paramName, string? message = null)
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName, message ?? $"Le paramètre '{paramName}' ne peut pas être null.");
        }
    }

    /// <summary>
    /// Vérifie qu'une chaîne de caractères n'est ni null, ni vide, ni composée uniquement d'espaces.
    /// </summary>
    /// <param name="value">Chaîne à valider.</param>
    /// <param name="paramName">Nom du paramètre (utilisé dans l'exception).</param>
    /// <param name="message">Message d'erreur personnalisé optionnel.</param>
    /// <exception cref="ArgumentException">Levée si <paramref name="value"/> est null, vide ou whitespace.</exception>
    /// <example>
    /// <code>
    /// public void CreateUser(string username)
    /// {
    ///     Guard.AgainstNullOrWhiteSpace(username, nameof(username));
    ///     // username est garanti non-vide ici
    /// }
    /// </code>
    /// </example>
    public static void AgainstNullOrWhiteSpace(string? value, string paramName, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                message ?? $"Le paramètre '{paramName}' ne peut pas être null, vide ou composé uniquement d'espaces.",
                paramName);
        }
    }

    /// <summary>
    /// Vérifie qu'un GUID n'est pas vide (Guid.Empty).
    /// </summary>
    /// <param name="value">GUID à valider.</param>
    /// <param name="paramName">Nom du paramètre (utilisé dans l'exception).</param>
    /// <param name="message">Message d'erreur personnalisé optionnel.</param>
    /// <exception cref="ArgumentException">Levée si <paramref name="value"/> est Guid.Empty.</exception>
    /// <remarks>
    /// Cette validation est critique pour éviter des requêtes en base de données avec des identifiants invalides,
    /// ce qui pourrait retourner des résultats incorrects ou échouer silencieusement.
    /// </remarks>
    /// <example>
    /// <code>
    /// public async Task&lt;User&gt; GetByIdAsync(Guid userId)
    /// {
    ///     Guard.AgainstEmptyGuid(userId, nameof(userId));
    ///     return await _repository.GetByIdAsync(userId);
    /// }
    /// </code>
    /// </example>
    public static void AgainstEmptyGuid(Guid value, string paramName, string? message = null)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                message ?? $"Le paramètre '{paramName}' ne peut pas être un GUID vide (Guid.Empty).",
                paramName);
        }
    }

    /// <summary>
    /// Vérifie qu'une réponse HTTP n'a pas déjà démarré avant de permettre sa modification.
    /// </summary>
    /// <param name="response">Réponse HTTP à vérifier.</param>
    /// <param name="message">Message d'erreur personnalisé optionnel.</param>
    /// <exception cref="InvalidOperationException">Levée si les headers de la réponse ont déjà été envoyés.</exception>
    /// <remarks>
    /// <para>
    /// Une fois que la réponse HTTP a commencé à être envoyée (premiers bytes écrits dans le stream),
    /// il est impossible de modifier les headers ou le status code. Toute tentative provoquera une exception.
    /// </para>
    /// <para>
    /// Cette guard clause est essentielle dans les middlewares pour éviter les crashs lors de la gestion
    /// d'erreurs après le début de l'envoi de la réponse.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public async Task InvokeAsync(HttpContext context)
    /// {
    ///     try
    ///     {
    ///         await _next(context);
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         Guard.AgainstResponseStarted(context.Response);
    ///         context.Response.StatusCode = 500;
    ///         await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    ///     }
    /// }
    /// </code>
    /// </example>
    public static void AgainstResponseStarted(HttpResponse response, string? message = null)
    {
        AgainstNull(response, nameof(response));

        if (response.HasStarted)
        {
            throw new InvalidOperationException(
                message ?? "Impossible de modifier la réponse HTTP : les headers ont déjà été envoyés au client.");
        }
    }

    /// <summary>
    /// Vérifie qu'une collection n'est pas null et contient au moins un élément.
    /// </summary>
    /// <typeparam name="T">Type des éléments de la collection.</typeparam>
    /// <param name="collection">Collection à valider.</param>
    /// <param name="paramName">Nom du paramètre (utilisé dans l'exception).</param>
    /// <param name="message">Message d'erreur personnalisé optionnel.</param>
    /// <exception cref="ArgumentException">Levée si la collection est null ou vide.</exception>
    /// <example>
    /// <code>
    /// public void ProcessBatch(IEnumerable&lt;int&gt; items)
    /// {
    ///     Guard.AgainstEmptyCollection(items, nameof(items));
    ///     foreach (var item in items)
    ///     {
    ///         // Traitement garanti sur au moins un élément
    ///     }
    /// }
    /// </code>
    /// </example>
    public static void AgainstEmptyCollection<T>(IEnumerable<T>? collection, string paramName, string? message = null)
    {
        if (collection is null || !collection.Any())
        {
            throw new ArgumentException(
                message ?? $"Le paramètre '{paramName}' ne peut pas être null ou vide.",
                paramName);
        }
    }

    /// <summary>
    /// Vérifie qu'un nombre est strictement positif (supérieur à zéro).
    /// </summary>
    /// <param name="value">Valeur numérique à valider.</param>
    /// <param name="paramName">Nom du paramètre (utilisé dans l'exception).</param>
    /// <param name="message">Message d'erreur personnalisé optionnel.</param>
    /// <exception cref="ArgumentException">Levée si <paramref name="value"/> est inférieur ou égal à zéro.</exception>
    /// <example>
    /// <code>
    /// public void SetQuota(int maxRequests)
    /// {
    ///     Guard.AgainstNegativeOrZero(maxRequests, nameof(maxRequests));
    ///     _maxRequests = maxRequests;
    /// }
    /// </code>
    /// </example>
    public static void AgainstNegativeOrZero(int value, string paramName, string? message = null)
    {
        if (value <= 0)
        {
            throw new ArgumentException(
                message ?? $"Le paramètre '{paramName}' doit être strictement positif (valeur actuelle : {value}).",
                paramName);
        }
    }

    /// <summary>
    /// Vérifie qu'une valeur se trouve dans un intervalle spécifié (bornes incluses).
    /// </summary>
    /// <typeparam name="T">Type de la valeur (doit être comparable).</typeparam>
    /// <param name="value">Valeur à valider.</param>
    /// <param name="min">Borne inférieure de l'intervalle (incluse).</param>
    /// <param name="max">Borne supérieure de l'intervalle (incluse).</param>
    /// <param name="paramName">Nom du paramètre (utilisé dans l'exception).</param>
    /// <param name="message">Message d'erreur personnalisé optionnel.</param>
    /// <exception cref="ArgumentOutOfRangeException">Levée si <paramref name="value"/> est hors de l'intervalle [min, max].</exception>
    /// <example>
    /// <code>
    /// public void SetPort(int port)
    /// {
    ///     Guard.AgainstOutOfRange(port, 1, 65535, nameof(port));
    ///     _port = port;
    /// }
    /// </code>
    /// </example>
    public static void AgainstOutOfRange<T>(T value, T min, T max, string paramName, string? message = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                message ?? $"Le paramètre '{paramName}' doit être compris entre {min} et {max} (valeur actuelle : {value}).");
        }
    }
}
