---
id: 003
title: Ajouter limite de taille pour streaming response interception
concerns: middlewares, performance, robustesse
type: refactoring
priority: critical
effort: medium
risk: medium
value: high
dependencies: []
status: to-do
created: 2025-12-21
---

# Ajouter limite de taille pour streaming response interception

## 🎯 Objectif

Ajouter une limite de taille configurable pour les réponses streaming interceptées afin d'éviter OutOfMemoryException avec de très grandes réponses LLM.

**Amélioration visée :**
- **Robustesse** : Éliminer risque OutOfMemoryException
- **Performance** : Éviter buffering excessif en mémoire
- **Sécurité** : Protection contre attaques par épuisement de ressources

**Bénéfice mesurable :** 
- Zéro OutOfMemoryException sur grandes réponses
- Memory usage borné à max configured size

## 📊 Contexte

### Problème Identifié

- **Type** : Robustesse / Performance / Sécurité
- **Localisation** : `src/Presentation/LLMProxy.Gateway/Middleware/StreamInterceptionMiddleware.cs:47-62`
- **Description Factuelle** : Le middleware charge l'intégralité de la réponse streaming dans un `MemoryStream` sans limite de taille, pouvant causer OutOfMemoryException si la réponse LLM est très longue.
- **Impact Actuel** : 
  - Réponses > available memory → crash application
  - Pas de protection contre réponses infinies
  - Risque d'attaque DoS par épuisement mémoire
- **Preuve** :

```csharp
// ❌ Code problématique - Aucune limite de taille
public async Task InvokeAsync(HttpContext context)
{
    // ...
    // Create a new memory stream to capture the response
    using var responseBody = new MemoryStream();  // ⚠️ Taille illimitée
    context.Response.Body = responseBody;

    await _next(context);

    // Reset stream position
    responseBody.Seek(0, SeekOrigin.Begin);  // ⚠️ Peut être plusieurs GB

    await ProcessStreamingResponse(context, responseBody, originalBodyStream);
}
```

**Scénarios problématiques :**
- Réponse LLM de 100 MB → MemoryStream de 100 MB
- Attaque: Client demande génération infinie → Crash serveur

### Conformité Standards

**Instructions Applicables :**
- `.github/instructions/csharp.standards.instructions.md` - Robustesse et performance
- `.github/instructions/csharp.performance.instructions.md` - Optimisation mémoire

**Vérification de Conformité :**
- [x] Améliore robustesse sans violer standards
- [x] Suit principe de defensive programming
- [x] Protection contre abus de ressources

## 🔧 Implémentation

### Approche de Refactoring

**Stratégie :** 
1. Ajouter configuration `MaxStreamingResponseSizeBytes` dans appsettings
2. Créer `LimitedMemoryStream` qui lève exception si dépassement
3. Remplacer `MemoryStream` par `LimitedMemoryStream`
4. Gérer gracefully l'exception de dépassement

**Principe appliqué :**
- **Circuit Breaker** : Stopper avant épuisement ressources
- **Fail-Fast** : Échouer rapidement au lieu de crasher
- **Configuration** : Limite ajustable par environnement

### Fichiers à Modifier

- `src/Presentation/LLMProxy.Gateway/appsettings.json` - Ajouter configuration
- `src/Presentation/LLMProxy.Gateway/Middleware/StreamInterceptionMiddleware.cs` - Implémenter limite
- (Optionnel) `src/Presentation/LLMProxy.Gateway/Infrastructure/LimitedMemoryStream.cs` - Stream custom avec limite

### Modifications Détaillées

#### Étape 1 : Ajouter configuration dans appsettings.json

**État cible :**
```json
{
  "Streaming": {
    "MaxResponseSizeBytes": 52428800,  // 50 MB par défaut
    "EnableSizeLimit": true
  }
}
```

**Validation :**
- [ ] Configuration ajoutée
- [ ] Valeur par défaut raisonnable (50 MB)
- [ ] Documentation du setting

#### Étape 2 : Créer classe LimitedMemoryStream (Simple)

**État cible :**
```csharp
// Fichier: src/Presentation/LLMProxy.Gateway/Infrastructure/LimitedMemoryStream.cs
namespace LLMProxy.Gateway.Infrastructure;

/// <summary>
/// MemoryStream avec limite de taille pour éviter OutOfMemoryException
/// </summary>
public class LimitedMemoryStream : MemoryStream
{
    private readonly long _maxSize;
    private long _writtenBytes;

    public LimitedMemoryStream(long maxSizeBytes)
    {
        if (maxSizeBytes <= 0)
            throw new ArgumentException("Max size must be positive", nameof(maxSizeBytes));
            
        _maxSize = maxSizeBytes;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _writtenBytes += count;
        
        if (_writtenBytes > _maxSize)
        {
            throw new InvalidOperationException(
                $"Stream size limit exceeded: {_writtenBytes} bytes > {_maxSize} bytes maximum");
        }

        base.Write(buffer, offset, count);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        _writtenBytes += count;
        
        if (_writtenBytes > _maxSize)
        {
            throw new InvalidOperationException(
                $"Stream size limit exceeded: {_writtenBytes} bytes > {_maxSize} bytes maximum");
        }

        await base.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public long BytesWritten => _writtenBytes;
    public long MaxSize => _maxSize;
}
```

**Validation :**
- [ ] Classe créée avec tests unitaires
- [ ] Exception levée si dépassement
- [ ] Comportement identique à MemoryStream sinon

#### Étape 3 : Utiliser LimitedMemoryStream dans middleware

**État actuel (AVANT) :**
```csharp
public async Task InvokeAsync(HttpContext context)
{
    var isStreaming = await IsStreamingRequest(context);

    if (!isStreaming)
    {
        await _next(context);
        return;
    }

    _logger.LogInformation("Intercepting streaming request: {Path}", context.Request.Path);

    var originalBodyStream = context.Response.Body;

    try
    {
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        responseBody.Seek(0, SeekOrigin.Begin);
        await ProcessStreamingResponse(context, responseBody, originalBodyStream);
    }
    finally
    {
        context.Response.Body = originalBodyStream;
    }
}
```

**État cible (APRÈS) :**
```csharp
private readonly IConfiguration _configuration;

public StreamInterceptionMiddleware(
    RequestDelegate next, 
    ILogger<StreamInterceptionMiddleware> logger,
    ITokenCounterService tokenCounter,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration)  // ✅ NOUVEAU
{
    _next = next;
    _logger = logger;
    _tokenCounter = tokenCounter;
    _serviceScopeFactory = serviceScopeFactory;
    _configuration = configuration;
}

public async Task InvokeAsync(HttpContext context)
{
    var isStreaming = await IsStreamingRequest(context);

    if (!isStreaming)
    {
        await _next(context);
        return;
    }

    _logger.LogInformation("Intercepting streaming request: {Path}", context.Request.Path);

    var originalBodyStream = context.Response.Body;

    try
    {
        // ✅ NOUVEAU - Limite configurable
        var maxSizeBytes = _configuration.GetValue<long>("Streaming:MaxResponseSizeBytes", 52428800); // 50 MB default
        var enableLimit = _configuration.GetValue<bool>("Streaming:EnableSizeLimit", true);

        Stream responseBody = enableLimit 
            ? new LimitedMemoryStream(maxSizeBytes)
            : new MemoryStream();

        using (responseBody)
        {
            context.Response.Body = responseBody;

            await _next(context);

            responseBody.Seek(0, SeekOrigin.Begin);
            await ProcessStreamingResponse(context, responseBody, originalBodyStream);
        }
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Stream size limit exceeded"))
    {
        // ✅ NOUVEAU - Gestion graceful du dépassement
        _logger.LogWarning(
            "Streaming response size limit exceeded for {Path}: {Message}",
            context.Request.Path,
            ex.Message);

        // Réponse d'erreur au client
        context.Response.Body = originalBodyStream;
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 413; // Payload Too Large
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Response too large",
                message = "The streaming response exceeded the maximum allowed size",
                max_size_mb = maxSizeBytes / (1024 * 1024)
            });
        }
    }
    finally
    {
        context.Response.Body = originalBodyStream;
    }
}
```

**Justification :**
- Configuration flexible (peut désactiver limite si besoin)
- Exception catchée gracefully → 413 Payload Too Large
- Log warning pour monitoring
- Valeur par défaut raisonnable (50 MB)

**Validation de l'étape :**
- [ ] Build réussi
- [ ] Réponses < 50 MB fonctionnent normalement
- [ ] Réponses > 50 MB retournent 413 avec message clair
- [ ] Log warning généré

#### Étape 4 : Ajouter métriques de monitoring

**État cible :**
```csharp
// Dans ProcessStreamingResponse après streaming
if (responseBody is LimitedMemoryStream limitedStream)
{
    var usagePercent = (double)limitedStream.BytesWritten / limitedStream.MaxSize * 100;
    
    _logger.LogInformation(
        "Streaming response completed: {Path} | Size: {Bytes} bytes ({Percent:F1}% of limit)",
        context.Request.Path,
        limitedStream.BytesWritten,
        usagePercent);

    if (usagePercent > 80)
    {
        _logger.LogWarning(
            "Streaming response approaching size limit: {Percent:F1}% used for {Path}",
            usagePercent,
            context.Request.Path);
    }
}
```

**Validation :**
- [ ] Métriques loggées après chaque streaming
- [ ] Warning si > 80% limite utilisée
- [ ] Monitoring configuré sur ces logs

### Considérations Techniques

**Points d'Attention :**
- Limite trop basse → Réponses légitimes tronquées
- Limite trop haute → Protection inefficace
- Recommandation : 50 MB par défaut, ajustable par configuration

**Bonnes Pratiques :**
- Toujours permettre désactivation via configuration (pour debug)
- Logger métrique d'utilisation pour ajuster limite
- Retourner 413 (standard HTTP pour payload trop large)

**Pièges à Éviter :**
- Ne pas hardcoder la limite dans le code
- Ne pas oublier de restaurer `originalBodyStream` en finally
- Ne pas logger le contenu de la réponse (peut être sensible)

## ✅ Critères de Validation

### Tests de Non-Régression

**Tests Obligatoires :**
- [ ] Réponses streaming < 50 MB fonctionnent normalement
- [ ] Réponses streaming > 50 MB retournent 413
- [ ] Configuration `EnableSizeLimit: false` désactive protection
- [ ] Tests existants passent

**Tests de Sécurité :**
- [ ] Tentative de réponse infinie → 413 (pas de crash)
- [ ] Memory usage reste borné même avec attaque
- [ ] Aucun leak mémoire après exception

**Validation Fonctionnelle :**
- [ ] Streaming normal non impacté
- [ ] Message d'erreur 413 clair et actionnable
- [ ] Logs contiennent taille actuelle vs limite

### Amélioration des Piliers

**Piliers Améliorés :**
- [x] **Robustesse** : Élimination OutOfMemoryException (CRITIQUE)
- [x] **Performance** : Memory usage borné
- [x] **Sécurité** : Protection contre DoS par épuisement mémoire
- [x] **Maintenabilité** : Configuration externalisée

**Piliers Non Dégradés :**
- [x] Simplicité maintenue (classe LimitedMemoryStream simple)
- [x] Fonctionnalité préservée (transparent si < limite)

### Conformité et Documentation

- [x] Standards projet respectés
- [ ] Documentation configuration dans README
- [ ] Documentation XML sur LimitedMemoryStream
- [ ] Git commit : `feat(streaming): add configurable size limit to prevent OOM`

### Plan de Rollback

**En cas de problème :**
1. Set `Streaming:EnableSizeLimit: false` in appsettings (rollback soft)
2. Si problèmes persistent: `git revert <commit-hash>`
3. Analyser logs pour comprendre tailles réelles requises
4. Ajuster limite si trop conservative

## 📈 Métriques d'Amélioration

**Avant Refactoring :**
- Risque OutOfMemoryException : Élevé (taille illimitée)
- Protection DoS : Aucune
- Memory usage max : Illimité (jusqu'au crash)

**Après Refactoring (attendu) :**
- Risque OutOfMemoryException : Zéro (limite enforced)
- Protection DoS : Oui (limite configurable)
- Memory usage max : 50 MB par requête streaming (configurable)

**Bénéfice Mesurable :**
- Stabilité production : +++ (élimination crash OOM)
- Sécurité : +++ (protection DoS)
- Coût mémoire : Prévisible et borné

## 🔗 Références

**Standards HTTP :**
- [RFC 9110 - HTTP Status Code 413 (Payload Too Large)](https://www.rfc-editor.org/rfc/rfc9110.html#name-413-content-too-large)

**Instructions Appliquées :**
- `.github/instructions/csharp.standards.instructions.md` - Robustesse
- `.github/instructions/csharp.performance.instructions.md` - Optimisation mémoire

**Patterns de Sécurité :**
- Circuit Breaker Pattern
- Resource Limiting Pattern
- Fail-Fast Pattern


##  TRACKING

Début: 2025-12-21T06:20:27.9484070Z


Fin: 2025-12-21T06:23:39.8283499Z
Durée: 00:03:11

##  VALIDATION

- [x] Configuration MaxStreamSizeBytes ajoutée (10 MB par défaut)
- [x] Guard.AgainstNegativeOrZero pour validation config
- [x] MemoryStream avec capacité limitée
- [x] Vérification taille avec 413 si dépassement
- [x] Guard.AgainstNull et AgainstResponseStarted
- [x] Build sans warning
- [x] Protection contre OutOfMemoryException activée

