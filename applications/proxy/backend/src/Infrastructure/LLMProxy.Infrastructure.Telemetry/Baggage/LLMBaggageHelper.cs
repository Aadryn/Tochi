// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// LLMProxy - Propagateur de Baggage pour le contexte LLM
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace LLMProxy.Infrastructure.Telemetry.Baggage;

/// <summary>
/// Propagateur de baggage OpenTelemetry pour les informations contextuelles LLM.
/// Permet de transmettre TenantId, UserId et autres métadonnées à travers les services.
/// </summary>
/// <remarks>
/// <para>
/// Le baggage est propagé via les headers HTTP standards W3C :
/// </para>
/// <list type="bullet">
/// <item><description><c>baggage</c> : Header W3C Baggage contenant les paires clé-valeur</description></item>
/// </list>
/// <para>
/// Ce propagateur ajoute automatiquement les informations métier LLMProxy
/// au baggage standard, permettant leur récupération dans les services en aval.
/// </para>
/// </remarks>
/// <example>
/// Injection du contexte :
/// <code>
/// LLMBaggageHelper.SetTenantId(tenantGuid);
/// LLMBaggageHelper.SetUserId(userGuid);
/// </code>
/// 
/// Extraction dans un service en aval :
/// <code>
/// var tenantId = LLMBaggageHelper.GetTenantId();
/// </code>
/// </example>
public static class LLMBaggageHelper
{
    /// <summary>
    /// Clé de baggage pour l'identifiant du tenant.
    /// </summary>
    public const string TenantIdKey = "llmproxy.tenant_id";

    /// <summary>
    /// Clé de baggage pour l'identifiant de l'utilisateur.
    /// </summary>
    public const string UserIdKey = "llmproxy.user_id";

    /// <summary>
    /// Clé de baggage pour l'identifiant de la clé API (masqué).
    /// </summary>
    public const string ApiKeyIdKey = "llmproxy.api_key_id";

    /// <summary>
    /// Clé de baggage pour l'identifiant de corrélation de requête.
    /// </summary>
    public const string CorrelationIdKey = "llmproxy.correlation_id";

    /// <summary>
    /// Clé de baggage pour le provider LLM ciblé.
    /// </summary>
    public const string ProviderKey = "llmproxy.provider";

    /// <summary>
    /// Clé de baggage pour le modèle LLM demandé.
    /// </summary>
    public const string ModelKey = "llmproxy.model";

    // ═══════════════════════════════════════════════════════════════
    // SETTERS - Ajout de valeurs au baggage
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Définit l'identifiant du tenant dans le baggage courant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    public static void SetTenantId(Guid tenantId)
    {
        OpenTelemetry.Baggage.SetBaggage(TenantIdKey, tenantId.ToString());
    }

    /// <summary>
    /// Définit l'identifiant de l'utilisateur dans le baggage courant.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    public static void SetUserId(Guid userId)
    {
        OpenTelemetry.Baggage.SetBaggage(UserIdKey, userId.ToString());
    }

    /// <summary>
    /// Définit l'identifiant de la clé API dans le baggage courant.
    /// </summary>
    /// <param name="apiKeyId">Identifiant de la clé API.</param>
    /// <remarks>
    /// L'identifiant est automatiquement tronqué pour des raisons de sécurité.
    /// </remarks>
    public static void SetApiKeyId(Guid apiKeyId)
    {
        // Masquer partiellement l'ID pour la sécurité
        var masked = apiKeyId.ToString()[..8] + "...";
        OpenTelemetry.Baggage.SetBaggage(ApiKeyIdKey, masked);
    }

    /// <summary>
    /// Définit l'identifiant de corrélation dans le baggage courant.
    /// </summary>
    /// <param name="correlationId">Identifiant de corrélation.</param>
    public static void SetCorrelationId(string correlationId)
    {
        OpenTelemetry.Baggage.SetBaggage(CorrelationIdKey, correlationId);
    }

    /// <summary>
    /// Définit le provider LLM dans le baggage courant.
    /// </summary>
    /// <param name="provider">Nom du provider (openai, anthropic, etc.).</param>
    public static void SetProvider(string provider)
    {
        OpenTelemetry.Baggage.SetBaggage(ProviderKey, provider);
    }

    /// <summary>
    /// Définit le modèle LLM dans le baggage courant.
    /// </summary>
    /// <param name="model">Nom du modèle.</param>
    public static void SetModel(string model)
    {
        OpenTelemetry.Baggage.SetBaggage(ModelKey, model);
    }

    /// <summary>
    /// Définit plusieurs valeurs de baggage en une seule opération.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="userId">Identifiant de l'utilisateur (optionnel).</param>
    /// <param name="apiKeyId">Identifiant de la clé API (optionnel).</param>
    /// <param name="correlationId">Identifiant de corrélation (optionnel).</param>
    public static void SetContext(
        Guid tenantId,
        Guid? userId = null,
        Guid? apiKeyId = null,
        string? correlationId = null)
    {
        SetTenantId(tenantId);

        if (userId.HasValue)
        {
            SetUserId(userId.Value);
        }

        if (apiKeyId.HasValue)
        {
            SetApiKeyId(apiKeyId.Value);
        }

        if (!string.IsNullOrEmpty(correlationId))
        {
            SetCorrelationId(correlationId);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // GETTERS - Récupération de valeurs depuis le baggage
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Récupère l'identifiant du tenant depuis le baggage courant.
    /// </summary>
    /// <returns>L'identifiant du tenant, ou null s'il n'est pas défini.</returns>
    public static Guid? GetTenantId()
    {
        var value = OpenTelemetry.Baggage.GetBaggage(TenantIdKey);
        return Guid.TryParse(value, out var guid) ? guid : null;
    }

    /// <summary>
    /// Récupère l'identifiant de l'utilisateur depuis le baggage courant.
    /// </summary>
    /// <returns>L'identifiant de l'utilisateur, ou null s'il n'est pas défini.</returns>
    public static Guid? GetUserId()
    {
        var value = OpenTelemetry.Baggage.GetBaggage(UserIdKey);
        return Guid.TryParse(value, out var guid) ? guid : null;
    }

    /// <summary>
    /// Récupère l'identifiant de la clé API depuis le baggage courant.
    /// </summary>
    /// <returns>L'identifiant masqué de la clé API, ou null s'il n'est pas défini.</returns>
    public static string? GetApiKeyId()
    {
        return OpenTelemetry.Baggage.GetBaggage(ApiKeyIdKey);
    }

    /// <summary>
    /// Récupère l'identifiant de corrélation depuis le baggage courant.
    /// </summary>
    /// <returns>L'identifiant de corrélation, ou null s'il n'est pas défini.</returns>
    public static string? GetCorrelationId()
    {
        return OpenTelemetry.Baggage.GetBaggage(CorrelationIdKey);
    }

    /// <summary>
    /// Récupère le provider LLM depuis le baggage courant.
    /// </summary>
    /// <returns>Le nom du provider, ou null s'il n'est pas défini.</returns>
    public static string? GetProvider()
    {
        return OpenTelemetry.Baggage.GetBaggage(ProviderKey);
    }

    /// <summary>
    /// Récupère le modèle LLM depuis le baggage courant.
    /// </summary>
    /// <returns>Le nom du modèle, ou null s'il n'est pas défini.</returns>
    public static string? GetModel()
    {
        return OpenTelemetry.Baggage.GetBaggage(ModelKey);
    }

    // ═══════════════════════════════════════════════════════════════
    // PROPAGATION VERS ACTIVITY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Enrichit une activité avec les valeurs du baggage courant.
    /// </summary>
    /// <param name="activity">L'activité à enrichir.</param>
    /// <remarks>
    /// Cette méthode copie les valeurs du baggage vers les tags de l'activité,
    /// permettant leur visualisation dans les outils de tracing.
    /// </remarks>
    public static void EnrichActivityFromBaggage(Activity? activity)
    {
        if (activity is null)
        {
            return;
        }

        var tenantId = GetTenantId();
        if (tenantId.HasValue)
        {
            activity.SetTag(Tracing.LLMSemanticConventions.LlmProxyTenantId, tenantId.Value.ToString());
        }

        var userId = GetUserId();
        if (userId.HasValue)
        {
            activity.SetTag(Tracing.LLMSemanticConventions.LlmProxyUserId, userId.Value.ToString());
        }

        var apiKeyId = GetApiKeyId();
        if (!string.IsNullOrEmpty(apiKeyId))
        {
            activity.SetTag(Tracing.LLMSemanticConventions.LlmProxyApiKeyId, apiKeyId);
        }

        var provider = GetProvider();
        if (!string.IsNullOrEmpty(provider))
        {
            activity.SetTag(Tracing.LLMSemanticConventions.GenAiSystem, provider);
        }

        var model = GetModel();
        if (!string.IsNullOrEmpty(model))
        {
            activity.SetTag(Tracing.LLMSemanticConventions.GenAiRequestModel, model);
        }
    }

    /// <summary>
    /// Efface toutes les valeurs LLMProxy du baggage courant.
    /// </summary>
    public static void Clear()
    {
        OpenTelemetry.Baggage.RemoveBaggage(TenantIdKey);
        OpenTelemetry.Baggage.RemoveBaggage(UserIdKey);
        OpenTelemetry.Baggage.RemoveBaggage(ApiKeyIdKey);
        OpenTelemetry.Baggage.RemoveBaggage(CorrelationIdKey);
        OpenTelemetry.Baggage.RemoveBaggage(ProviderKey);
        OpenTelemetry.Baggage.RemoveBaggage(ModelKey);
    }
}
