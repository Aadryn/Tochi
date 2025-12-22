// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// LLMProxy - Source d'activité pour le tracing LLM
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System.Diagnostics;

namespace LLMProxy.Infrastructure.Telemetry.Tracing;

/// <summary>
/// Source d'activité (ActivitySource) dédiée aux opérations LLM.
/// Fournit des méthodes pour créer des spans tracés avec les attributs sémantiques GenAI.
/// </summary>
/// <remarks>
/// <para>
/// Cette classe centralise la création des spans pour toutes les opérations LLM,
/// garantissant une instrumentation cohérente à travers l'application.
/// </para>
/// <para>
/// Les spans créés incluent automatiquement les attributs sémantiques OpenTelemetry GenAI,
/// permettant une analyse détaillée dans les outils d'observabilité.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// using var activity = LLMActivitySource.StartLLMRequest(
///     system: "openai",
///     model: "gpt-4",
///     operation: "chat");
/// 
/// // ... exécution de la requête ...
/// 
/// LLMActivitySource.SetResponseAttributes(activity, 
///     inputTokens: 150, 
///     outputTokens: 500);
/// </code>
/// </example>
public static class LLMActivitySource
{
    /// <summary>
    /// Nom de la source d'activité pour les opérations LLM.
    /// </summary>
    public const string SourceName = "LLMProxy.LLM";

    /// <summary>
    /// Version de la source d'activité.
    /// </summary>
    public const string SourceVersion = "2.0.0";

    /// <summary>
    /// Source d'activité singleton pour les opérations LLM.
    /// </summary>
    private static readonly ActivitySource ActivitySource = new(SourceName, SourceVersion);

    /// <summary>
    /// Obtient la source d'activité pour l'enregistrement auprès d'OpenTelemetry.
    /// </summary>
    public static ActivitySource Source => ActivitySource;

    /// <summary>
    /// Démarre une nouvelle activité pour une requête LLM.
    /// </summary>
    /// <param name="system">Système GenAI (openai, anthropic, ollama, etc.).</param>
    /// <param name="model">Nom du modèle demandé.</param>
    /// <param name="operation">Type d'opération (chat, completion, embedding).</param>
    /// <param name="tenantId">Identifiant du tenant (optionnel).</param>
    /// <param name="userId">Identifiant de l'utilisateur (optionnel).</param>
    /// <returns>L'activité créée, ou null si le tracing n'est pas activé.</returns>
    /// <remarks>
    /// L'activité retournée doit être disposée à la fin de l'opération
    /// pour enregistrer correctement la durée du span.
    /// </remarks>
    public static Activity? StartLLMRequest(
        string system,
        string model,
        string operation = LLMSemanticConventions.Operations.Chat,
        Guid? tenantId = null,
        Guid? userId = null)
    {
        var activity = ActivitySource.StartActivity(
            name: $"{system}.{operation}",
            kind: ActivityKind.Client);

        if (activity is null)
        {
            return null;
        }

        // Attributs GenAI de base
        activity.SetTag(LLMSemanticConventions.GenAiSystem, system);
        activity.SetTag(LLMSemanticConventions.GenAiRequestModel, model);
        activity.SetTag(LLMSemanticConventions.GenAiOperationName, operation);

        // Attributs LLMProxy contextuels
        if (tenantId.HasValue)
        {
            activity.SetTag(LLMSemanticConventions.LlmProxyTenantId, tenantId.Value.ToString());
        }

        if (userId.HasValue)
        {
            activity.SetTag(LLMSemanticConventions.LlmProxyUserId, userId.Value.ToString());
        }

        return activity;
    }

    /// <summary>
    /// Démarre une nouvelle activité pour une requête de streaming LLM.
    /// </summary>
    /// <param name="system">Système GenAI.</param>
    /// <param name="model">Nom du modèle demandé.</param>
    /// <param name="tenantId">Identifiant du tenant (optionnel).</param>
    /// <returns>L'activité créée, ou null si le tracing n'est pas activé.</returns>
    public static Activity? StartStreamingRequest(
        string system,
        string model,
        Guid? tenantId = null)
    {
        var activity = StartLLMRequest(system, model, LLMSemanticConventions.Operations.Chat, tenantId);
        activity?.SetTag(LLMSemanticConventions.LlmProxyIsStreaming, true);
        return activity;
    }

    /// <summary>
    /// Ajoute les attributs de requête optionnels à une activité.
    /// </summary>
    /// <param name="activity">L'activité à enrichir.</param>
    /// <param name="maxTokens">Nombre maximum de tokens demandé.</param>
    /// <param name="temperature">Température de génération.</param>
    /// <param name="topP">Top-P (nucleus sampling).</param>
    public static void SetRequestAttributes(
        Activity? activity,
        int? maxTokens = null,
        double? temperature = null,
        double? topP = null)
    {
        if (activity is null)
        {
            return;
        }

        if (maxTokens.HasValue)
        {
            activity.SetTag(LLMSemanticConventions.GenAiRequestMaxTokens, maxTokens.Value);
        }

        if (temperature.HasValue)
        {
            activity.SetTag(LLMSemanticConventions.GenAiRequestTemperature, temperature.Value);
        }

        if (topP.HasValue)
        {
            activity.SetTag(LLMSemanticConventions.GenAiRequestTopP, topP.Value);
        }
    }

    /// <summary>
    /// Ajoute les attributs de réponse à une activité.
    /// </summary>
    /// <param name="activity">L'activité à enrichir.</param>
    /// <param name="inputTokens">Nombre de tokens en entrée.</param>
    /// <param name="outputTokens">Nombre de tokens en sortie.</param>
    /// <param name="responseModel">Modèle effectivement utilisé.</param>
    /// <param name="responseId">Identifiant de la réponse.</param>
    /// <param name="finishReason">Raison de fin de génération.</param>
    public static void SetResponseAttributes(
        Activity? activity,
        int inputTokens,
        int outputTokens,
        string? responseModel = null,
        string? responseId = null,
        string? finishReason = null)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetTag(LLMSemanticConventions.GenAiUsageInputTokens, inputTokens);
        activity.SetTag(LLMSemanticConventions.GenAiUsageOutputTokens, outputTokens);

        if (!string.IsNullOrEmpty(responseModel))
        {
            activity.SetTag(LLMSemanticConventions.GenAiResponseModel, responseModel);
        }

        if (!string.IsNullOrEmpty(responseId))
        {
            activity.SetTag(LLMSemanticConventions.GenAiResponseId, responseId);
        }

        if (!string.IsNullOrEmpty(finishReason))
        {
            activity.SetTag(LLMSemanticConventions.GenAiResponseFinishReasons, finishReason);
        }
    }

    /// <summary>
    /// Ajoute les attributs de coût et performance à une activité.
    /// </summary>
    /// <param name="activity">L'activité à enrichir.</param>
    /// <param name="estimatedCostCents">Coût estimé en centimes.</param>
    /// <param name="ttftMs">Time To First Token en millisecondes.</param>
    /// <param name="clusterId">Identifiant du cluster YARP.</param>
    /// <param name="routeId">Identifiant de la route YARP.</param>
    public static void SetProxyAttributes(
        Activity? activity,
        decimal? estimatedCostCents = null,
        double? ttftMs = null,
        string? clusterId = null,
        string? routeId = null)
    {
        if (activity is null)
        {
            return;
        }

        if (estimatedCostCents.HasValue)
        {
            activity.SetTag(LLMSemanticConventions.LlmProxyCostEstimatedCents, (double)estimatedCostCents.Value);
        }

        if (ttftMs.HasValue)
        {
            activity.SetTag(LLMSemanticConventions.LlmProxyTtftMs, ttftMs.Value);
        }

        if (!string.IsNullOrEmpty(clusterId))
        {
            activity.SetTag(LLMSemanticConventions.LlmProxyClusterId, clusterId);
        }

        if (!string.IsNullOrEmpty(routeId))
        {
            activity.SetTag(LLMSemanticConventions.LlmProxyRouteId, routeId);
        }
    }

    /// <summary>
    /// Marque une activité comme réussie.
    /// </summary>
    /// <param name="activity">L'activité à marquer.</param>
    public static void SetSuccess(Activity? activity)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetStatus(ActivityStatusCode.Ok);
        activity.SetTag(LLMSemanticConventions.LlmProxyStatus, LLMSemanticConventions.Statuses.Success);
    }

    /// <summary>
    /// Marque une activité comme échouée avec une erreur.
    /// </summary>
    /// <param name="activity">L'activité à marquer.</param>
    /// <param name="exception">L'exception survenue.</param>
    /// <param name="status">Statut d'erreur spécifique.</param>
    public static void SetError(
        Activity? activity,
        Exception? exception = null,
        string status = LLMSemanticConventions.Statuses.Error)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetStatus(ActivityStatusCode.Error, exception?.Message);
        activity.SetTag(LLMSemanticConventions.LlmProxyStatus, status);

        if (exception is not null)
        {
            // Enregistrer l'exception comme événement avec les attributs sémantiques standard
            var exceptionTags = new ActivityTagsCollection
            {
                { "exception.type", exception.GetType().FullName },
                { "exception.message", exception.Message },
                { "exception.stacktrace", exception.StackTrace }
            };
            activity.AddEvent(new ActivityEvent("exception", tags: exceptionTags));
        }
    }

    /// <summary>
    /// Enregistre un événement dans l'activité courante.
    /// </summary>
    /// <param name="activity">L'activité où enregistrer l'événement.</param>
    /// <param name="name">Nom de l'événement.</param>
    /// <param name="attributes">Attributs additionnels de l'événement.</param>
    public static void AddEvent(
        Activity? activity,
        string name,
        IDictionary<string, object?>? attributes = null)
    {
        if (activity is null)
        {
            return;
        }

        var tags = new ActivityTagsCollection();
        if (attributes is not null)
        {
            foreach (var (key, value) in attributes)
            {
                if (value is not null)
                {
                    tags.Add(key, value);
                }
            }
        }

        activity.AddEvent(new ActivityEvent(name, tags: tags));
    }
}
