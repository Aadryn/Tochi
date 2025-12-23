# TÂCHE 118 : Refactor ADR-001 - CohereProviderClient (18 DTOs)

**Créée le** : 2025-12-24T00:00:00Z UTC  
**Priorité** : 🔴 Critique  
**ADR Violée** : ADR-001 - Un seul type par fichier C#  
**Auditeur** : GitHub Copilot Agent - Audit Pas à Pas

---

## CONTEXTE

### Violation Détectée

**Fichier** : `applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Providers/PublicCloud/CohereProviderClient.cs`

**ADR concernée** : ADR-001 - Un seul type par fichier C#

**Règle violée** : 
> Chaque fichier C# DOIT contenir un seul type de premier niveau (class, interface, enum, struct, record).

### Preuve Factuelle

**Types déclarés (19)** :
```
L16:   public sealed class CohereProviderClient
L350:  private sealed record CohereChatRequest
L365:  private sealed record CohereMessage
L371:  private sealed record CohereChatResponse
L379:  private sealed record CohereMessageContent
L384:  private sealed record CohereContentBlock
L390:  private sealed record CohereUsage
L395:  private sealed record CohereTokens
L401:  private sealed record CohereStreamEvent
L408:  private sealed record CohereDelta
L413:  private sealed record CohereMessageDelta
L418:  private sealed record CohereContentDelta
L423:  private sealed record CohereStreamResponse
L430:  private sealed record CohereMeta
L435:  private sealed record CohereEmbedRequest
L443:  private sealed record CohereEmbedResponse
L449:  private sealed record CohereEmbeddingResult
L454:  private sealed record CohereEmbedMeta
L459:  private sealed record CohereBilledUnits
```

**Fichier** : 466 lignes, **19 types** (1 classe + 18 DTOs privés)

### Impact

**Criticité** : 🔴 Critique

**Problèmes** :
- Violation flagrante ADR-001 (19 types dans un seul fichier)
- Maintenabilité réduite (fichier de 466 lignes)
- Impossibilité de réutiliser les DTOs ailleurs
- Navigation difficile dans le fichier
- Tests unitaires compliqués (impossibilité de mocker DTOs)

---

## OBJECTIF

Extraire les 18 DTOs privés vers des fichiers séparés dans `/Contracts/Cohere/`.

---

## ÉTAPES DE CORRECTION

### Étape 1 : Créer répertoire Contracts/Cohere

```bash
mkdir -p applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Contracts/Cohere
```

**Validation** :
- [ ] Répertoire créé

### Étape 2 : Extraire DTOs Chat (9 fichiers)

**Extraire chaque DTO** vers fichier séparé :

1. `CohereChatRequest.cs`
2. `CohereMessage.cs`
3. `CohereChatResponse.cs`
4. `CohereMessageContent.cs`
5. `CohereContentBlock.cs`
6. `CohereUsage.cs`
7. `CohereTokens.cs`
8. `CohereStreamEvent.cs`
9. `CohereDelta.cs`
10. `CohereMessageDelta.cs`
11. `CohereContentDelta.cs`
12. `CohereStreamResponse.cs`
13. `CohereMeta.cs`

**Template de fichier** :
```csharp
namespace LLMProxy.Infrastructure.LLMProviders.Contracts.Cohere;

/// <summary>
/// [Description du DTO].
/// </summary>
internal sealed record CohereChatRequest
{
    // Propriétés...
}
```

**Validation** :
- [ ] 13 fichiers créés dans `/Contracts/Cohere/`
- [ ] Namespace `LLMProxy.Infrastructure.LLMProviders.Contracts.Cohere`
- [ ] Visibilité `internal` (pas private)
- [ ] Documentation XML complète

### Étape 3 : Extraire DTOs Embeddings (5 fichiers)

14. `CohereEmbedRequest.cs`
15. `CohereEmbedResponse.cs`
16. `CohereEmbeddingResult.cs`
17. `CohereEmbedMeta.cs`
18. `CohereBilledUnits.cs`

**Validation** :
- [ ] 5 fichiers créés
- [ ] Total 18 DTOs extraits

### Étape 4 : Mettre à jour CohereProviderClient.cs

**AVANT** (lignes 350-459) :
```csharp
private sealed record CohereChatRequest { ... }
private sealed record CohereMessage { ... }
// ... 16 autres DTOs
```

**APRÈS** :
```csharp
// Supprimer TOUS les DTOs privés
// Ajouter using si nécessaire
using LLMProxy.Infrastructure.LLMProviders.Contracts.Cohere;
```

**Validation** :
- [ ] Tous les DTOs supprimés de CohereProviderClient.cs
- [ ] Using ajouté
- [ ] Fichier réduit à ~300 lignes
- [ ] Build réussi

---

## CRITÈRES DE SUCCÈS

- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests : 100% passing
- [ ] 18 fichiers créés dans `/Contracts/Cohere/`
- [ ] CohereProviderClient.cs < 350 lignes
- [ ] ADR-001 respectée (1 type par fichier)

---

## MÉTADONNÉES

- **Effort estimé** : 6h
- **Risque** : 3/10
- **Impact** : 9/10
