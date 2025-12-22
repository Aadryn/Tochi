// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// LLMProxy - Enrichisseur ApiKey
// ADR-031 : Structured Logging
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using Serilog.Core;
using Serilog.Events;

namespace LLMProxy.Gateway.Logging;

/// <summary>
/// Enrichisseur Serilog qui ajoute l'identifiant de la clé API (masqué)
/// à tous les événements de log dans le contexte d'une requête HTTP.
/// </summary>
/// <remarks>
/// <para>
/// Pour des raisons de sécurité, seuls les 8 premiers caractères de l'identifiant
/// de la clé API sont loggés, suffisants pour l'identification sans exposer
/// la clé complète.
/// </para>
/// <para>
/// L'ApiKeyId est récupéré depuis <c>HttpContext.Items["ApiKeyId"]</c> qui doit
/// être peuplé par le middleware d'authentification par clé API.
/// </para>
/// </remarks>
/// <example>
/// Configuration dans Serilog :
/// <code>
/// .Enrich.With&lt;ApiKeyEnricher&gt;()
/// </code>
/// Log résultant :
/// <code>
/// { "ApiKeyId": "abc12345...", ... }
/// </code>
/// </example>
public sealed class ApiKeyEnricher : ILogEventEnricher
{
    /// <summary>
    /// Nom de la propriété ajoutée aux logs.
    /// </summary>
    public const string PropertyName = "ApiKeyId";

    /// <summary>
    /// Nombre de caractères de l'identifiant à afficher.
    /// </summary>
    private const int VisibleCharacters = 8;

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ApiKeyEnricher"/>.
    /// </summary>
    public ApiKeyEnricher()
        : this(new HttpContextAccessor())
    {
    }

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ApiKeyEnricher"/>
    /// avec un <see cref="IHttpContextAccessor"/> spécifique.
    /// </summary>
    /// <param name="httpContextAccessor">L'accesseur au contexte HTTP.</param>
    /// <exception cref="ArgumentNullException">
    /// Levée si <paramref name="httpContextAccessor"/> est null.
    /// </exception>
    public ApiKeyEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    /// Enrichit l'événement de log avec l'identifiant de clé API masqué.
    /// </summary>
    /// <param name="logEvent">L'événement de log à enrichir.</param>
    /// <param name="propertyFactory">La factory pour créer des propriétés de log.</param>
    /// <remarks>
    /// <para>
    /// Si aucune clé API n'est présente dans le contexte, la propriété n'est pas ajoutée.
    /// </para>
    /// <para>
    /// Le masquage affiche les 8 premiers caractères suivis de "..." pour la traçabilité
    /// sans exposition de données sensibles.
    /// </para>
    /// </remarks>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(propertyFactory);

        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.Items.TryGetValue("ApiKeyId", out var apiKeyId) != true)
        {
            return;
        }

        string maskedValue;
        if (apiKeyId is Guid keyGuid)
        {
            var keyString = keyGuid.ToString();
            maskedValue = MaskApiKey(keyString);
        }
        else if (apiKeyId is string keyString)
        {
            maskedValue = MaskApiKey(keyString);
        }
        else
        {
            return;
        }

        var property = propertyFactory.CreateProperty(PropertyName, maskedValue);
        logEvent.AddPropertyIfAbsent(property);
    }

    /// <summary>
    /// Masque une clé API en ne conservant que les premiers caractères.
    /// </summary>
    /// <param name="apiKey">La clé API à masquer.</param>
    /// <returns>La clé masquée avec format "xxxxxxxx...".</returns>
    private static string MaskApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            return "unknown";
        }

        if (apiKey.Length <= VisibleCharacters)
        {
            return apiKey;
        }

        return $"{apiKey[..VisibleCharacters]}...";
    }
}
