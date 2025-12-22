// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// LLMProxy - Conventions Sémantiques OpenTelemetry pour GenAI
// Basé sur : https://opentelemetry.io/docs/specs/semconv/gen-ai/
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

namespace LLMProxy.Infrastructure.Telemetry.Tracing;

/// <summary>
/// Conventions sémantiques OpenTelemetry pour les attributs GenAI (Generative AI).
/// Conformes aux spécifications officielles OpenTelemetry Semantic Conventions.
/// </summary>
/// <remarks>
/// <para>
/// Ces conventions permettent une interopérabilité entre différents outils d'observabilité
/// (Jaeger, Grafana Tempo, Honeycomb, etc.) en utilisant des noms d'attributs standardisés.
/// </para>
/// <para>
/// Référence : <see href="https://opentelemetry.io/docs/specs/semconv/gen-ai/"/>
/// </para>
/// </remarks>
public static class LLMSemanticConventions
{
    // ═══════════════════════════════════════════════════════════════
    // ATTRIBUTS DE SYSTÈME GenAI
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Identifiant du système GenAI (ex: "openai", "anthropic", "ollama").
    /// </summary>
    /// <example>
    /// <code>
    /// activity.SetTag(LLMSemanticConventions.GenAiSystem, "openai");
    /// </code>
    /// </example>
    public const string GenAiSystem = "gen_ai.system";

    /// <summary>
    /// Type d'opération GenAI (ex: "chat", "completion", "embedding").
    /// </summary>
    public const string GenAiOperationName = "gen_ai.operation.name";

    // ═══════════════════════════════════════════════════════════════
    // ATTRIBUTS DE REQUÊTE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Nom du modèle demandé dans la requête (ex: "gpt-4", "claude-3-opus").
    /// </summary>
    public const string GenAiRequestModel = "gen_ai.request.model";

    /// <summary>
    /// Nombre maximum de tokens à générer demandé.
    /// </summary>
    public const string GenAiRequestMaxTokens = "gen_ai.request.max_tokens";

    /// <summary>
    /// Température demandée pour la génération (0.0 à 2.0).
    /// </summary>
    public const string GenAiRequestTemperature = "gen_ai.request.temperature";

    /// <summary>
    /// Top-P (nucleus sampling) demandé.
    /// </summary>
    public const string GenAiRequestTopP = "gen_ai.request.top_p";

    /// <summary>
    /// Top-K demandé pour la sélection de tokens.
    /// </summary>
    public const string GenAiRequestTopK = "gen_ai.request.top_k";

    /// <summary>
    /// Séquences d'arrêt demandées.
    /// </summary>
    public const string GenAiRequestStopSequences = "gen_ai.request.stop_sequences";

    /// <summary>
    /// Pénalité de présence demandée.
    /// </summary>
    public const string GenAiRequestPresencePenalty = "gen_ai.request.presence_penalty";

    /// <summary>
    /// Pénalité de fréquence demandée.
    /// </summary>
    public const string GenAiRequestFrequencyPenalty = "gen_ai.request.frequency_penalty";

    // ═══════════════════════════════════════════════════════════════
    // ATTRIBUTS DE RÉPONSE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Nom du modèle effectivement utilisé dans la réponse.
    /// Peut différer du modèle demandé (alias, version spécifique).
    /// </summary>
    public const string GenAiResponseModel = "gen_ai.response.model";

    /// <summary>
    /// Identifiant unique de la réponse fourni par le provider.
    /// </summary>
    public const string GenAiResponseId = "gen_ai.response.id";

    /// <summary>
    /// Raisons de fin de génération (ex: "stop", "length", "content_filter").
    /// </summary>
    public const string GenAiResponseFinishReasons = "gen_ai.response.finish_reasons";

    // ═══════════════════════════════════════════════════════════════
    // ATTRIBUTS D'UTILISATION (TOKENS)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Nombre de tokens dans le prompt/entrée.
    /// </summary>
    public const string GenAiUsageInputTokens = "gen_ai.usage.input_tokens";

    /// <summary>
    /// Nombre de tokens générés en sortie.
    /// </summary>
    public const string GenAiUsageOutputTokens = "gen_ai.usage.output_tokens";

    // ═══════════════════════════════════════════════════════════════
    // ATTRIBUTS PERSONNALISÉS LLMProxy
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Identifiant du tenant effectuant la requête.
    /// </summary>
    public const string LlmProxyTenantId = "llmproxy.tenant.id";

    /// <summary>
    /// Identifiant de l'utilisateur effectuant la requête.
    /// </summary>
    public const string LlmProxyUserId = "llmproxy.user.id";

    /// <summary>
    /// Identifiant de la clé API utilisée (masqué).
    /// </summary>
    public const string LlmProxyApiKeyId = "llmproxy.api_key.id";

    /// <summary>
    /// Identifiant du cluster YARP ciblé.
    /// </summary>
    public const string LlmProxyClusterId = "llmproxy.cluster.id";

    /// <summary>
    /// Identifiant de la route YARP utilisée.
    /// </summary>
    public const string LlmProxyRouteId = "llmproxy.route.id";

    /// <summary>
    /// Indique si la requête utilise le streaming SSE.
    /// </summary>
    public const string LlmProxyIsStreaming = "llmproxy.is_streaming";

    /// <summary>
    /// Coût estimé de la requête en centimes (EUR).
    /// </summary>
    public const string LlmProxyCostEstimatedCents = "llmproxy.cost.estimated_cents";

    /// <summary>
    /// Durée de la première réponse (Time To First Token) en millisecondes.
    /// </summary>
    public const string LlmProxyTtftMs = "llmproxy.ttft_ms";

    /// <summary>
    /// Statut de la requête LLM (success, error, timeout, rate_limited).
    /// </summary>
    public const string LlmProxyStatus = "llmproxy.status";

    // ═══════════════════════════════════════════════════════════════
    // VALEURS STANDARDS POUR gen_ai.system
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Valeurs standardisées pour l'attribut <see cref="GenAiSystem"/>.
    /// </summary>
    public static class Systems
    {
        /// <summary>OpenAI (GPT-4, GPT-3.5, etc.).</summary>
        public const string OpenAi = "openai";
        
        /// <summary>Azure OpenAI Service.</summary>
        public const string AzureOpenAi = "azure_openai";
        
        /// <summary>Anthropic (Claude).</summary>
        public const string Anthropic = "anthropic";
        
        /// <summary>AWS Bedrock.</summary>
        public const string AwsBedrock = "aws_bedrock";
        
        /// <summary>Ollama (local).</summary>
        public const string Ollama = "ollama";
        
        /// <summary>vLLM (local).</summary>
        public const string Vllm = "vllm";
        
        /// <summary>Mistral AI.</summary>
        public const string Mistral = "mistral";
        
        /// <summary>OVH Cloud AI.</summary>
        public const string OvhCloud = "ovh_cloud";
        
        /// <summary>Scaleway AI.</summary>
        public const string Scaleway = "scaleway";
    }

    /// <summary>
    /// Valeurs standardisées pour l'attribut <see cref="GenAiOperationName"/>.
    /// </summary>
    public static class Operations
    {
        /// <summary>Génération de chat/conversation.</summary>
        public const string Chat = "chat";
        
        /// <summary>Complétion de texte.</summary>
        public const string Completion = "completion";
        
        /// <summary>Génération d'embeddings.</summary>
        public const string Embedding = "embedding";
        
        /// <summary>Génération d'images.</summary>
        public const string ImageGeneration = "image_generation";
        
        /// <summary>Transcription audio (speech-to-text).</summary>
        public const string Transcription = "transcription";
        
        /// <summary>Synthèse vocale (text-to-speech).</summary>
        public const string TextToSpeech = "text_to_speech";
    }

    /// <summary>
    /// Valeurs standardisées pour <see cref="LlmProxyStatus"/>.
    /// </summary>
    public static class Statuses
    {
        /// <summary>Requête réussie.</summary>
        public const string Success = "success";
        
        /// <summary>Erreur lors de la requête.</summary>
        public const string Error = "error";
        
        /// <summary>Timeout de la requête.</summary>
        public const string Timeout = "timeout";
        
        /// <summary>Rate limit atteint.</summary>
        public const string RateLimited = "rate_limited";
        
        /// <summary>Filtre de contenu déclenché.</summary>
        public const string ContentFiltered = "content_filtered";
    }
}
