# TÂCHE 117 : Corriger ADR-043 - Exception Handling (12 fichiers)

**Créée le** : 2025-12-24T00:00:00Z UTC  
**Priorité** : 🟡 Majeure  
**ADR Violée** : ADR-043 - Exception Handling Strategy  
**Auditeur** : GitHub Copilot Agent - Audit Pas à Pas

---

## CONTEXTE

### Violation Détectée

**ADR concernée** : ADR-043 - Exception Handling Strategy

**Règle violée** : 
> Toute exception capturée DOIT être loggée avec contexte. Les catch blocks silencieux sont INTERDITS.

### Fichiers en Violation (12 fichiers)

| # | Fichier | Ligne | Pattern | Impact |
|---|---------|-------|---------|--------|
| 1 | `Permission.cs` | L133 | `catch { return false; }` | Perte observabilité |
| 2 | `Scope.cs` | L133 | `catch { return false; }` | Perte observabilité |
| 3 | `User.cs` | L157 | `catch { return false; }` | Perte observabilité |
| 4 | `OllamaProviderClient.cs` | L119 | `catch { return false; }` | Health check silencieux |
| 5 | `AnthropicProviderClient.cs` | L137 | `catch { return false; }` | Health check silencieux |
| 6 | `AWSBedrockProviderClient.cs` | L183 | `catch { return false; }` | Health check silencieux |
| 7 | `AzureOpenAIProviderClient.cs` | L127 | `catch { return false; }` | Health check silencieux |
| 8 | `CohereProviderClient.cs` | L137 | `catch { return false; }` | Health check silencieux |
| 9 | `GoogleGeminiProviderClient.cs` | L139 | `catch { return false; }` | Health check silencieux |
| 10 | `HuggingFaceProviderClient.cs` | L145 | `catch { return false; }` | Health check silencieux |
| 11 | `OpenFgaStoreProvider.cs` | L227 | `catch { // Store existe }` | Commentaire insuffisant |
| 12 | `OpenAICompatibleProviderClient.cs` | L63 | `catch { // Fallback }` | Commentaire insuffisant |

### Preuve Détaillée - CohereProviderClient.cs:137

```csharp
// ❌ VIOLATION ADR-043 : Exception silencieuse
public override async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
{
    try
    {
        var response = await HttpClient.GetAsync("/health", cancellationToken);
        return response.IsSuccessStatusCode;
    }
    catch  // <-- L137 : Exception avalée sans logging
    {
        return false;
    }
}
```

### Impact

**Criticité** : 🟡 Majeure

**Problèmes identifiés** :
- Perte totale de traçabilité des erreurs réseau/provider
- Impossible de diagnostiquer pourquoi un provider est "unhealthy"
- Métriques d'observabilité incomplètes
- Difficulté à identifier pannes providers

**Conséquences si non corrigé** :
- Temps de diagnostic multiplié en production
- Incidents providers non détectés à temps
- SLA dégradé sans visibilité sur cause racine

---

## OBJECTIF

Ajouter logging structuré dans TOUS les catch blocks silencieux.

---

## ÉTAPES DE CORRECTION

### Étape 1 : Providers Health Checks (7 fichiers)

**Pattern commun** pour tous les providers :

**AVANT** :
```csharp
public override async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
{
    try
    {
        var response = await HttpClient.GetAsync("/health", cancellationToken);
        return response.IsSuccessStatusCode;
    }
    catch  // ❌ Silencieux
    {
        return false;
    }
}
```

**APRÈS** :
```csharp
public override async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
{
    try
    {
        var response = await HttpClient.GetAsync("/health", cancellationToken);
        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)  // ✅ Exception capturée
    {
        _logger.LogWarning(ex, 
            "Health check failed for provider {ProviderName} ({ProviderType})",
            Name, 
            GetType().Name);
        return false;
    }
}
```

**Fichiers à modifier** :
- [ ] `OllamaProviderClient.cs` (L119)
- [ ] `AnthropicProviderClient.cs` (L137)
- [ ] `AWSBedrockProviderClient.cs` (L183)
- [ ] `AzureOpenAIProviderClient.cs` (L127)
- [ ] `CohereProviderClient.cs` (L137)
- [ ] `GoogleGeminiProviderClient.cs` (L139)
- [ ] `HuggingFaceProviderClient.cs` (L145)

**Validation Étape 1** :
- [ ] 7 fichiers modifiés
- [ ] Tous les catch capturent `Exception ex`
- [ ] `_logger.LogWarning(ex, ...)` ajouté partout
- [ ] Logging structuré avec paramètres nommés
- [ ] Build réussi (0 erreurs, 0 warnings)

### Étape 2 : Domain Entities (3 fichiers)

**AVANT** (exemple Permission.cs:133) :
```csharp
public static bool TryParse(string value, out Permission permission)
{
    try
    {
        permission = Parse(value);
        return true;
    }
    catch  // ❌ Silencieux
    {
        permission = default;
        return false;
    }
}
```

**APRÈS** :
```csharp
public static bool TryParse(string value, out Permission permission)
{
    try
    {
        permission = Parse(value);
        return true;
    }
    catch (Exception ex)  // ✅ Exception capturée
    {
        // Log à niveau Debug car c'est un TryParse (comportement attendu)
        // Utiliser ILogger statique via LoggerMessage ou contexte
        permission = default;
        return false;
    }
}
```

**Note** : Pour les Value Objects, le logging peut être omis si c'est un `TryParse` (échec attendu). Mais il faut capturer l'exception pour conformité ADR.

**Fichiers à modifier** :
- [ ] `Permission.cs` (L133)
- [ ] `Scope.cs` (L133)
- [ ] `User.cs` (L157)

**Validation Étape 2** :
- [ ] 3 fichiers modifiés
- [ ] Exception capturée explicitement
- [ ] Decision consciente : logger ou non selon cas d'usage
- [ ] Commentaire expliquant pourquoi pas de log si applicable
- [ ] Build réussi

### Étape 3 : Infrastructure (2 fichiers)

**OpenFgaStoreProvider.cs:227**

**AVANT** :
```csharp
catch
{
    // Store existe déjà
}
```

**APRÈS** :
```csharp
catch (Exception ex)
{
    // Store existe déjà - comportement attendu lors de l'initialisation
    _logger.LogDebug(ex, "Store {StoreId} already exists, skipping creation", storeId);
}
```

**OpenAICompatibleProviderClient.cs:63**

**AVANT** :
```csharp
catch
{
    // Fallback to default format
}
```

**APRÈS** :
```csharp
catch (Exception ex)
{
    _logger.LogDebug(ex, 
        "Failed to detect API format for {BaseUrl}, falling back to OpenAI format",
        HttpClient.BaseAddress);
    // Fallback to default format
}
```

**Validation Étape 3** :
- [ ] 2 fichiers modifiés
- [ ] Logging ajouté avec contexte métier
- [ ] Niveau de log approprié (Debug pour cas attendus)
- [ ] Build réussi

---

## CRITÈRES DE SUCCÈS

### Build et Tests

- [ ] `dotnet build` exitCode = 0
- [ ] `dotnet build` 0 Warning(s)
- [ ] `dotnet test` exitCode = 0
- [ ] `dotnet test` Test Run Successful (100% passed)

### Conformité ADR-043

- [ ] Aucun `catch` sans paramètre `Exception ex`
- [ ] Tous les catch loggent l'exception OU ont commentaire justifiant l'absence de log
- [ ] Logging structuré avec paramètres nommés (pas de string interpolation)
- [ ] Niveau de log approprié (Warning pour erreurs, Debug pour cas attendus)
- [ ] Validation manuelle code review

### Documentation

- [ ] Commit message : "fix(ADR-043): Add logging to all catch blocks for observability"

---

## RÉFÉRENCES

- **ADR** : `docs/adr/043-exception-handling-strategy.adr.md`
- **Instructions** : `.github/instructions/csharp.fundamentals.instructions.md`
- **Analyse** : `refactor.analysis.md` (section ADR-043)

---

## MÉTADONNÉES

- **Effort estimé** : 3h
- **Risque régression** : 2/10 (ajout logging uniquement)
- **Impact métier** : 8/10 (observabilité critique)
- **Dépendances** : Aucune
