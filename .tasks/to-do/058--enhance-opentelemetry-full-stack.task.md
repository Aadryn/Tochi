# Tâche 058 - Améliorer OpenTelemetry Full Stack

## PRIORITÉ
🔴 **P1 - HAUTE** (Priorité 3/8 de la refonte)

## OBJECTIF

Étendre l'intégration OpenTelemetry existante vers une observabilité complète : traces distribuées enrichies, métriques métier LLM, logs corrélés, et export vers collecteur OTLP.

## CONTEXTE

### État Actuel
- OpenTelemetry 1.9.0 intégré (traces + métriques de base)
- Instrumentation ASP.NET Core et HttpClient
- Export Console + OTLP basique

### État Cible
- Traces enrichies avec contexte LLM (model, provider, tokens)
- Métriques métier (latence par provider, tokens/sec, coût estimé)
- Corrélation logs-traces-métriques via TraceId/SpanId
- Baggage propagation pour contexte multi-service
- Dashboards prêts pour Grafana/Jaeger

## IMPLÉMENTATION

### Phase 1 : Custom Instrumentation LLM
```
src/Infrastructure/LLMProxy.Infrastructure.Telemetry/
├── Tracing/
│   ├── LLMActivitySource.cs       # ActivitySource dédié LLM
│   ├── LLMSpanProcessor.cs        # Enrichissement spans
│   └── LLMSamplingStrategy.cs     # Sampling adaptatif
├── Metrics/
│   ├── LLMMetrics.cs              # Meters custom
│   ├── ProviderLatencyHistogram.cs
│   ├── TokenCounterGauge.cs
│   └── CostEstimationCounter.cs
├── Baggage/
│   └── LLMBaggagePropagator.cs    # Propagation TenantId, etc.
```

### Phase 2 : Enrichissement Automatique
```csharp
// Attributs sémantiques LLM (conventions OpenTelemetry GenAI)
public static class LLMSemanticConventions
{
    public const string GEN_AI_SYSTEM = "gen_ai.system";           // "openai", "anthropic"
    public const string GEN_AI_REQUEST_MODEL = "gen_ai.request.model";
    public const string GEN_AI_RESPONSE_MODEL = "gen_ai.response.model";
    public const string GEN_AI_REQUEST_MAX_TOKENS = "gen_ai.request.max_tokens";
    public const string GEN_AI_USAGE_INPUT_TOKENS = "gen_ai.usage.input_tokens";
    public const string GEN_AI_USAGE_OUTPUT_TOKENS = "gen_ai.usage.output_tokens";
    public const string GEN_AI_RESPONSE_FINISH_REASONS = "gen_ai.response.finish_reasons";
}
```

### Phase 3 : Métriques Métier
```csharp
// Métriques à implémenter
- llmproxy.request.duration (Histogram, par provider/model)
- llmproxy.request.count (Counter, par tenant/provider/status)
- llmproxy.tokens.input (Counter, par tenant/model)
- llmproxy.tokens.output (Counter, par tenant/model)
- llmproxy.cost.estimated (Counter, par tenant, en centimes)
- llmproxy.provider.availability (Gauge, 0/1 par provider)
- llmproxy.ratelimit.remaining (Gauge, par tenant)
```

### Phase 4 : Configuration Avancée
```json
{
  "OpenTelemetry": {
    "ServiceName": "LLMProxy.Gateway",
    "ServiceVersion": "2.0.0",
    "Tracing": {
      "Sampler": "ParentBased",
      "SamplingRatio": 0.1,
      "AlwaysSampleErrors": true
    },
    "Metrics": {
      "ExportIntervalMilliseconds": 15000,
      "HistogramBuckets": [0.01, 0.05, 0.1, 0.5, 1, 2, 5, 10, 30, 60]
    },
    "Exporter": {
      "Type": "OTLP",
      "Endpoint": "http://otel-collector:4317",
      "Protocol": "grpc"
    }
  }
}
```

## CRITÈRES DE SUCCÈS

- [ ] ActivitySource dédié `LLMProxy.LLM` créé
- [ ] Métriques métier LLM implémentées (7 métriques)
- [ ] Attributs sémantiques GenAI sur tous les spans LLM
- [ ] Corrélation TraceId dans logs Serilog
- [ ] Baggage TenantId/RequestId propagé
- [ ] Configuration OTLP externalisée
- [ ] Tests unitaires pour métriques
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Documentation XML complète (français)

## DÉPENDANCES

- Tâche 057 (Serilog) pour corrélation logs

## ESTIMATION

**Effort** : 6h
**Complexité** : Moyenne

## RÉFÉRENCES

- [OpenTelemetry Semantic Conventions GenAI](https://opentelemetry.io/docs/specs/semconv/gen-ai/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
- ADR existants OpenTelemetry
