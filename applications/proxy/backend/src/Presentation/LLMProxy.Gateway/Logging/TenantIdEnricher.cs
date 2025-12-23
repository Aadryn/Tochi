// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// LLMProxy - Enrichisseur TenantId
// ADR-031 : Structured Logging
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using Serilog.Core;
using Serilog.Events;

namespace LLMProxy.Gateway.Logging;

/// <summary>
/// Enrichisseur Serilog qui ajoute automatiquement l'identifiant du tenant
/// à tous les événements de log dans le contexte d'une requête HTTP.
/// </summary>
/// <remarks>
/// <para>
/// Cet enrichisseur récupère le TenantId depuis le <see cref="IHttpContextAccessor"/>
/// et l'ajoute comme propriété structurée à chaque log. Cela permet de filtrer
/// et analyser les logs par tenant dans les systèmes d'agrégation (Seq, Elasticsearch).
/// </para>
/// <para>
/// Le TenantId est extrait depuis <c>HttpContext.Items["TenantId"]</c> qui doit
/// être peuplé par un middleware d'authentification en amont.
/// </para>
/// </remarks>
/// <example>
/// Configuration dans Serilog :
/// <code>
/// .Enrich.With&lt;TenantIdEnricher&gt;()
/// </code>
/// </example>
public sealed class TenantIdEnricher : ILogEventEnricher
{
    /// <summary>
    /// Nom de la propriété ajoutée aux logs.
    /// </summary>
    public const string PropertyName = "TenantId";

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="TenantIdEnricher"/>.
    /// </summary>
    /// <remarks>
    /// Ce constructeur est utilisé par Serilog lors de la création de l'enrichisseur.
    /// L'<see cref="IHttpContextAccessor"/> est résolu depuis le conteneur DI.
    /// </remarks>
    public TenantIdEnricher()
        : this(new HttpContextAccessor())
    {
    }

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="TenantIdEnricher"/>
    /// avec un <see cref="IHttpContextAccessor"/> spécifique.
    /// </summary>
    /// <param name="httpContextAccessor">L'accesseur au contexte HTTP.</param>
    /// <exception cref="ArgumentNullException">
    /// Levée si <paramref name="httpContextAccessor"/> est null.
    /// </exception>
    public TenantIdEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    /// Enrichit l'événement de log avec le TenantId du contexte HTTP courant.
    /// </summary>
    /// <param name="logEvent">L'événement de log à enrichir.</param>
    /// <param name="propertyFactory">La factory pour créer des propriétés de log.</param>
    /// <remarks>
    /// <para>
    /// Si aucun contexte HTTP n'est disponible ou si le TenantId n'est pas défini,
    /// la propriété est ajoutée avec la valeur "unknown".
    /// </para>
    /// <para>
    /// Le TenantId est cherché dans <c>HttpContext.Items["TenantId"]</c>.
    /// </para>
    /// </remarks>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(propertyFactory);

        var httpContext = _httpContextAccessor.HttpContext;
        
        string tenantIdValue;
        if (httpContext?.Items.TryGetValue("TenantId", out var tenantId) == true && tenantId is Guid tid)
        {
            tenantIdValue = tid.ToString();
        }
        else
        {
            tenantIdValue = "unknown";
        }

        var property = propertyFactory.CreateProperty(PropertyName, tenantIdValue);
        logEvent.AddPropertyIfAbsent(property);
    }
}
