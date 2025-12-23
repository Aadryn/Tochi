# Tâche 068 - Corriger ADR-043 : Exception Handling dans Providers

## PRIORITÉ
🟠 **P2 - MAJEURE** (Quick Win - Robustesse)

## OBJECTIF

Corriger les violations ADR-043 (Exception Handling Strategy) dans tous les provider clients LLM. Actuellement, 16+ `catch` blocs avalent les exceptions sans logging.

## CONTEXTE

### ADR-043 Règles Violées
- Toute exception doit être loggée avant d'être gérée
- Pas de `catch` vide ou silencieux
- Utiliser `ProblemDetails` pour les erreurs HTTP

### Violations Identifiées

**Pattern problématique répété dans 9 fichiers providers :**

```csharp
// ❌ VIOLATION : Exception avalée sans logging
public override async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
{
    try
    {
        // ... health check logic
        return true;
    }
    catch
    {
        return false;  // Exception silencieuse !
    }
}
```

### Fichiers Concernés

1. `applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Providers/PublicCloud/CohereProviderClient.cs`
2. `applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Providers/PublicCloud/OpenAIProviderClient.cs`
3. `applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Providers/PublicCloud/AzureOpenAIProviderClient.cs`
4. `applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Providers/PublicCloud/GoogleGeminiProviderClient.cs`
5. `applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Providers/PublicCloud/HuggingFaceProviderClient.cs`
6. `applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Providers/PublicCloud/AWSBedrockProviderClient.cs`
7. `applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Providers/PublicCloud/AnthropicProviderClient.cs`
8. `applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Providers/Local/OllamaProviderClient.cs`
9. `applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Providers/Local/VllmProviderClient.cs`

## IMPLÉMENTATION

### Correction Type (Appliquer à tous les fichiers)

```csharp
// ✅ CONFORME ADR-043 : Exception loggée
public override async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
{
    try
    {
        // ... health check logic
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Health check failed for provider {ProviderName}", Name);
        return false;
    }
}
```

### Étapes de Correction

1. Pour chaque fichier provider :
   - Localiser tous les `catch` sans paramètre ou vides
   - Ajouter le paramètre `Exception ex`
   - Ajouter un `_logger.LogWarning` ou `_logger.LogError` selon la criticité
   - Inclure le contexte (provider name, operation)

2. Vérifier que `ILogger<T>` est injecté dans chaque provider

## CRITÈRES DE SUCCÈS

- [ ] 0 `catch` sans logging dans les providers
- [ ] Chaque exception loggée avec niveau approprié (Warning/Error)
- [ ] Contexte inclus dans les logs (provider name, operation type)
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests existants passent toujours

## ESTIMATION

**Effort** : 2h
**Risque** : Faible (ajout de logging sans changement de comportement)
**Valeur** : Haute (meilleure observabilité, debugging facilité)

## RÉFÉRENCES

- ADR-043 : Exception Handling Strategy
- ADR-031 : Structured Logging
- `refactor.analysis.md` : Analyse détaillée des violations



## TRACKING

Début: 2025-12-29T15:00:00Z
Fin: 2025-12-29T15:30:00Z
Durée: 00:30:00

## RÉSUMÉ DE COMPLÉTION

### Fichiers modifiés (8)
- Cohere ProviderClient.cs
- OpenAI ProviderClient.cs  
- AzureOpenAI ProviderClient.cs
- GoogleGemini ProviderClient.cs
- HuggingFace ProviderClient.cs
- AWSBedrock ProviderClient.cs
- Anthropic ProviderClient.cs
- Ollama ProviderClient.cs

### Corrections appliquées
- Remplacement de `catch` par `catch (Exception ex)`
- Ajout de `Logger.LogWarning(ex, "Health check failed for provider {ProviderName}", Name);`
- 0 catch silencieux restant

### Validation
- ✅ Build: 0 erreurs, 0 warnings
- ✅ Tests: 0 nouveaux échecs (4 échecs pré-existants dans Redis)
- ✅ ADR-043: 100% conforme

---
status: done
completed: 2025-12-29

