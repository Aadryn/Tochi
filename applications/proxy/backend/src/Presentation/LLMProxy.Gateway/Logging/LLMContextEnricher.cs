// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// LLMProxy - Enrichisseur de contexte LLM
// ADR-031 : Structured Logging
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using Serilog.Core;
using Serilog.Events;

namespace LLMProxy.Gateway.Logging;

/// <summary>
/// Enrichisseur Serilog qui ajoute les informations contextuelles LLM
/// à tous les événements de log dans le contexte d'une requête vers un provider LLM.
/// </summary>
/// <remarks>
/// <para>
/// Cet enrichisseur ajoute des propriétés spécifiques aux requêtes LLM :
/// </para>
/// <list type="bullet">
/// <item><description><b>LLMProvider</b> : Le nom du provider (OpenAI, Ollama, Anthropic, etc.)</description></item>
/// <item><description><b>LLMModel</b> : Le modèle utilisé (gpt-4, claude-3, llama3, etc.)</description></item>
/// <item><description><b>LLMClusterId</b> : L'identifiant du cluster YARP ciblé</description></item>
/// <item><description><b>IsStreaming</b> : Indique si la requête utilise le streaming</description></item>
/// </list>
/// <para>
/// Ces informations permettent d'analyser l'utilisation des différents providers
/// et modèles, et de diagnostiquer les problèmes spécifiques à certaines configurations.
/// </para>
/// </remarks>
/// <example>
/// Configuration dans Serilog :
/// <code>
/// .Enrich.With&lt;LLMContextEnricher&gt;()
/// </code>
/// Log résultant :
/// <code>
/// { "LLMProvider": "OpenAI", "LLMModel": "gpt-4", "IsStreaming": true, ... }
/// </code>
/// </example>
public sealed class LLMContextEnricher : ILogEventEnricher
{
    /// <summary>
    /// Noms des propriétés ajoutées aux logs.
    /// </summary>
    public static class PropertyNames
    {
        /// <summary>Nom du provider LLM (OpenAI, Anthropic, Ollama, etc.).</summary>
        public const string Provider = "LLMProvider";
        
        /// <summary>Nom du modèle LLM utilisé.</summary>
        public const string Model = "LLMModel";
        
        /// <summary>Identifiant du cluster YARP ciblé.</summary>
        public const string ClusterId = "LLMClusterId";
        
        /// <summary>Indique si la requête utilise le streaming SSE.</summary>
        public const string IsStreaming = "LLMIsStreaming";
        
        /// <summary>Nombre de tokens dans la requête (si disponible).</summary>
        public const string InputTokens = "LLMInputTokens";
        
        /// <summary>Nombre de tokens dans la réponse (si disponible).</summary>
        public const string OutputTokens = "LLMOutputTokens";
    }

    /// <summary>
    /// Clés utilisées dans HttpContext.Items pour stocker les informations LLM.
    /// </summary>
    public static class ItemKeys
    {
        /// <summary>Clé pour le nom du provider.</summary>
        public const string Provider = "LLM_Provider";
        
        /// <summary>Clé pour le nom du modèle.</summary>
        public const string Model = "LLM_Model";
        
        /// <summary>Clé pour l'identifiant du cluster.</summary>
        public const string ClusterId = "LLM_ClusterId";
        
        /// <summary>Clé pour l'indicateur de streaming.</summary>
        public const string IsStreaming = "LLM_IsStreaming";
        
        /// <summary>Clé pour le nombre de tokens d'entrée.</summary>
        public const string InputTokens = "LLM_InputTokens";
        
        /// <summary>Clé pour le nombre de tokens de sortie.</summary>
        public const string OutputTokens = "LLM_OutputTokens";
    }

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="LLMContextEnricher"/>.
    /// </summary>
    public LLMContextEnricher()
        : this(new HttpContextAccessor())
    {
    }

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="LLMContextEnricher"/>
    /// avec un <see cref="IHttpContextAccessor"/> spécifique.
    /// </summary>
    /// <param name="httpContextAccessor">L'accesseur au contexte HTTP.</param>
    /// <exception cref="ArgumentNullException">
    /// Levée si <paramref name="httpContextAccessor"/> est null.
    /// </exception>
    public LLMContextEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    /// Enrichit l'événement de log avec les informations de contexte LLM.
    /// </summary>
    /// <param name="logEvent">L'événement de log à enrichir.</param>
    /// <param name="propertyFactory">La factory pour créer des propriétés de log.</param>
    /// <remarks>
    /// Seules les propriétés disponibles dans le contexte HTTP sont ajoutées.
    /// Les propriétés manquantes sont simplement ignorées.
    /// </remarks>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(propertyFactory);

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return;
        }

        // Provider LLM
        if (httpContext.Items.TryGetValue(ItemKeys.Provider, out var provider) && provider is string providerStr)
        {
            var property = propertyFactory.CreateProperty(PropertyNames.Provider, providerStr);
            logEvent.AddPropertyIfAbsent(property);
        }

        // Modèle LLM
        if (httpContext.Items.TryGetValue(ItemKeys.Model, out var model) && model is string modelStr)
        {
            var property = propertyFactory.CreateProperty(PropertyNames.Model, modelStr);
            logEvent.AddPropertyIfAbsent(property);
        }

        // Cluster YARP
        if (httpContext.Items.TryGetValue(ItemKeys.ClusterId, out var clusterId) && clusterId is string clusterStr)
        {
            var property = propertyFactory.CreateProperty(PropertyNames.ClusterId, clusterStr);
            logEvent.AddPropertyIfAbsent(property);
        }

        // Streaming
        if (httpContext.Items.TryGetValue(ItemKeys.IsStreaming, out var isStreaming) && isStreaming is bool streamingBool)
        {
            var property = propertyFactory.CreateProperty(PropertyNames.IsStreaming, streamingBool);
            logEvent.AddPropertyIfAbsent(property);
        }

        // Tokens d'entrée
        if (httpContext.Items.TryGetValue(ItemKeys.InputTokens, out var inputTokens) && inputTokens is int inputInt)
        {
            var property = propertyFactory.CreateProperty(PropertyNames.InputTokens, inputInt);
            logEvent.AddPropertyIfAbsent(property);
        }

        // Tokens de sortie
        if (httpContext.Items.TryGetValue(ItemKeys.OutputTokens, out var outputTokens) && outputTokens is int outputInt)
        {
            var property = propertyFactory.CreateProperty(PropertyNames.OutputTokens, outputInt);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}
