# TÂCHE 116 : Corriger ADR-044 - TokenCounterService Deadlock Potentiel

**Créée le** : 2025-12-24T00:00:00Z UTC  
**Priorité** : 🔴 Critique  
**ADR Violée** : ADR-044 - Async/Await Best Practices  
**Auditeur** : GitHub Copilot Agent - Audit Pas à Pas

---

## CONTEXTE

### Violation Détectée

**Fichier** : `applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/TokenCounterService.cs`

**ADR concernée** : ADR-044 - Async/Await Best Practices

**Règle violée** : 
> NEVER block on async (.Result, .Wait(), .GetAwaiter().GetResult())  
> ALWAYS use WaitAsync() sur SemaphoreSlim

### Preuve Factuelle

**Lignes** : 259-271

```csharp
private GptEncoding GetEncodingForModel(string modelName)
{
    var encodingName = GetEncodingNameForModel(modelName);

    _cacheLock.Wait();  // <-- L259 : ❌ VIOLATION - DEADLOCK POTENTIEL
    try
    {
        if (!_encodingCache.TryGetValue(encodingName, out var encoding))
        {
            encoding = GptEncoding.GetEncoding(encodingName);
            _encodingCache[encodingName] = encoding;
        }

        return encoding;
    }
    finally
    {
        _cacheLock.Release();
    }
}
```

### Impact

**Criticité** : 🔴 Critique

**Problèmes identifiés** :
- Utilisation de `_cacheLock.Wait()` (synchrone) au lieu de `WaitAsync()` (asynchrone)
- Méthode synchrone `GetEncodingForModel()` appelée depuis code potentiellement async
- **Risque de deadlock** si appelée depuis contexte avec SynchronizationContext (ASP.NET Core)
- Thread pool starvation potentielle

**Conséquences si non corrigé** :
- **Deadlock** : L'application peut se bloquer complètement sous charge
- **Performance dégradée** : Threads bloqués inutilement
- **Instabilité** : Comportement non déterministe selon contexte d'appel
- **Violation ADR-044** : Code non conforme aux best practices async

---

## OBJECTIF

Refactoriser `GetEncodingForModel()` pour être async et utiliser `await _cacheLock.WaitAsync()`.

---

## ÉTAPES DE CORRECTION (Baby Steps)

### Étape 1 : Convertir méthode en async

**AVANT** :
```csharp
private GptEncoding GetEncodingForModel(string modelName)
{
    var encodingName = GetEncodingNameForModel(modelName);

    _cacheLock.Wait();  // ❌ Synchrone
    try
    {
        if (!_encodingCache.TryGetValue(encodingName, out var encoding))
        {
            encoding = GptEncoding.GetEncoding(encodingName);
            _encodingCache[encodingName] = encoding;
        }

        return encoding;
    }
    finally
    {
        _cacheLock.Release();
    }
}
```

**APRÈS** :
```csharp
/// <summary>
/// Obtient l'encodage GPT pour un modèle spécifique de manière asynchrone.
/// </summary>
/// <param name="modelName">Nom du modèle LLM.</param>
/// <param name="cancellationToken">Token d'annulation.</param>
/// <returns>Encodage GPT correspondant au modèle.</returns>
/// <remarks>
/// Utilise un cache thread-safe avec SemaphoreSlim.WaitAsync() pour éviter deadlocks.
/// </remarks>
private async Task<GptEncoding> GetEncodingForModelAsync(
    string modelName,
    CancellationToken cancellationToken = default)
{
    var encodingName = GetEncodingNameForModel(modelName);

    await _cacheLock.WaitAsync(cancellationToken);  // ✅ Asynchrone
    try
    {
        if (!_encodingCache.TryGetValue(encodingName, out var encoding))
        {
            encoding = GptEncoding.GetEncoding(encodingName);
            _encodingCache[encodingName] = encoding;
        }

        return encoding;
    }
    finally
    {
        _cacheLock.Release();
    }
}
```

**Validation Étape 1** :
- [ ] Méthode renommée avec suffixe `Async`
- [ ] Signature retourne `Task<GptEncoding>`
- [ ] Paramètre `CancellationToken` ajouté
- [ ] `_cacheLock.Wait()` remplacé par `await _cacheLock.WaitAsync(cancellationToken)`
- [ ] Documentation XML complète ajoutée
- [ ] Code compile sans erreurs

### Étape 2 : Mettre à jour les appels (CountTokens)

**IDENTIFIER** tous les appels à `GetEncodingForModel()` :

```bash
grep -rn "GetEncodingForModel" applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/TokenCounterService.cs
```

**Résultat attendu** : Ligne 76, 105, 144 (méthodes `CountTokens`, `CountTokensFromMessages`, `CountTokensFromStreamChunk`)

**AVANT** (exemple ligne 76) :
```csharp
public int CountTokens(string modelName, string text)
{
    var encoding = GetEncodingForModel(modelName);  // ❌ Synchrone
    return encoding.Encode(text).Count;
}
```

**APRÈS** :
```csharp
/// <summary>
/// Compte le nombre de tokens dans un texte pour un modèle spécifique de manière asynchrone.
/// </summary>
/// <param name="modelName">Nom du modèle LLM.</param>
/// <param name="text">Texte à analyser.</param>
/// <param name="cancellationToken">Token d'annulation.</param>
/// <returns>Nombre de tokens.</returns>
public async Task<int> CountTokensAsync(
    string modelName,
    string text,
    CancellationToken cancellationToken = default)
{
    var encoding = await GetEncodingForModelAsync(modelName, cancellationToken);  // ✅ Async
    return encoding.Encode(text).Count;
}
```

**Répéter pour** :
- `CountTokensFromMessages()` → `CountTokensFromMessagesAsync()`
- `CountTokensFromStreamChunk()` → `CountTokensFromStreamChunkAsync()`

**Validation Étape 2** :
- [ ] Méthode `CountTokensAsync()` créée
- [ ] Méthode `CountTokensFromMessagesAsync()` créée
- [ ] Méthode `CountTokensFromStreamChunkAsync()` créée
- [ ] Toutes les méthodes appellent `await GetEncodingForModelAsync()`
- [ ] Documentation XML complète sur chaque méthode
- [ ] Code compile sans erreurs

### Étape 3 : Mettre à jour l'interface ITokenCounterService

**AVANT** :
```csharp
public interface ITokenCounterService
{
    int CountTokens(string modelName, string text);
    int CountTokensFromMessages(string modelName, IEnumerable<object> messages);
    int CountTokensFromStreamChunk(string modelName, string chunk);
}
```

**APRÈS** :
```csharp
/// <summary>
/// Service de comptage de tokens pour les modèles LLM.
/// </summary>
public interface ITokenCounterService
{
    /// <summary>
    /// Compte le nombre de tokens dans un texte de manière asynchrone.
    /// </summary>
    /// <param name="modelName">Nom du modèle LLM.</param>
    /// <param name="text">Texte à analyser.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Nombre de tokens.</returns>
    Task<int> CountTokensAsync(string modelName, string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compte le nombre de tokens à partir de messages de conversation de manière asynchrone.
    /// </summary>
    /// <param name="modelName">Nom du modèle LLM.</param>
    /// <param name="messages">Messages de conversation.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Nombre de tokens.</returns>
    Task<int> CountTokensFromMessagesAsync(string modelName, IEnumerable<object> messages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compte le nombre de tokens dans un chunk de streaming de manière asynchrone.
    /// </summary>
    /// <param name="modelName">Nom du modèle LLM.</param>
    /// <param name="chunk">Chunk de données streaming.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Nombre de tokens.</returns>
    Task<int> CountTokensFromStreamChunkAsync(string modelName, string chunk, CancellationToken cancellationToken = default);
}
```

**Validation Étape 3** :
- [ ] Interface `ITokenCounterService` mise à jour
- [ ] Toutes les signatures retournent `Task<int>`
- [ ] Paramètres `CancellationToken` ajoutés
- [ ] Documentation XML complète
- [ ] Code compile sans erreurs

### Étape 4 : Mettre à jour les appelants (StreamInterceptionMiddleware, etc.)

**CHERCHER** tous les usages de `ITokenCounterService` :

```bash
grep -rn "CountTokens\|CountTokensFromMessages\|CountTokensFromStreamChunk" applications/proxy/backend/src --include="*.cs"
```

**Pour chaque appelant** :

**AVANT** :
```csharp
var tokenCount = _tokenCounterService.CountTokens(modelName, text);
```

**APRÈS** :
```csharp
var tokenCount = await _tokenCounterService.CountTokensAsync(modelName, text, cancellationToken);
```

**Fichiers probables à modifier** :
- `StreamInterceptionMiddleware.cs`
- `ProviderOrchestrator.cs`
- `FailoverManager.cs`
- Autres services d'application

**Validation Étape 4** :
- [ ] Tous les appels synchrones remplacés par `await XxxAsync()`
- [ ] `CancellationToken` propagé dans tous les appels
- [ ] Méthodes appelantes sont async (signature `async Task`)
- [ ] Aucun appel synchrone restant (`.Result`, `.Wait()`)
- [ ] Code compile sans erreurs

### Étape 5 : Mettre à jour les tests unitaires

**FICHIER** : `tests/LLMProxy.Infrastructure.LLMProviders.Tests/TokenCounterServiceTests.cs` (si existe)

**AVANT** :
```csharp
[Fact]
public void CountTokens_ShouldReturnCorrectCount()
{
    // Arrange
    var service = new TokenCounterService();
    
    // Act
    var count = service.CountTokens("gpt-4", "Hello world");
    
    // Assert
    count.Should().BeGreaterThan(0);
}
```

**APRÈS** :
```csharp
[Fact]
public async Task CountTokensAsync_ShouldReturnCorrectCount()
{
    // Arrange
    var service = new TokenCounterService();
    
    // Act
    var count = await service.CountTokensAsync("gpt-4", "Hello world");
    
    // Assert
    count.Should().BeGreaterThan(0);
}
```

**Validation Étape 5** :
- [ ] Tests unitaires convertis en `async Task`
- [ ] Appels deviennent `await XxxAsync()`
- [ ] Tests passent (100% réussite)
- [ ] Aucun test flaky détecté

---

## CRITÈRES DE SUCCÈS

### Build et Tests

- [ ] `dotnet build` exitCode = 0
- [ ] `dotnet build` 0 Warning(s)
- [ ] `dotnet test` exitCode = 0
- [ ] `dotnet test` Test Run Successful (100% passed)

### Conformité ADR-044

- [ ] Aucun `_cacheLock.Wait()` synchrone restant
- [ ] Tous les appels utilisent `await _cacheLock.WaitAsync(cancellationToken)`
- [ ] Toutes les méthodes async propagent `CancellationToken`
- [ ] Aucune méthode bloquante sur Task (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`)
- [ ] Validation manuelle code review

### Documentation

- [ ] Commentaires XML à jour sur toutes méthodes modifiées
- [ ] Documentation explique le changement async
- [ ] Commit message explicite : "fix(ADR-044): Convert TokenCounterService to async to prevent deadlocks"

---

## RÉFÉRENCES

- **ADR** : `docs/adr/044-async-await-best-practices.adr.md`
- **Instructions** : `.github/instructions/csharp.async.instructions.md`
- **Analyse complète** : `refactor.analysis.md` (section ADR-044)
- **Microsoft Docs** : [Async/Await Best Practices](https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)

---

## MÉTADONNÉES

- **Effort estimé** : 1-2h
- **Risque régression** : 3/10 (changement signature interface)
- **Impact métier** : 9/10 (prévention deadlock critique)
- **Dépendances** : Aucune
- **Fichiers impactés** : 
  - `TokenCounterService.cs` (refactoring principal)
  - `ITokenCounterService.cs` (interface)
  - `StreamInterceptionMiddleware.cs` (appelant)
  - `ProviderOrchestrator.cs` (appelant potentiel)
  - Tests unitaires associés

---

**Début** : [À définir]  
**Fin** : [À définir]  
**Durée** : [À définir]
## TRACKING
Début: 2025-12-24T00:37:11Z
