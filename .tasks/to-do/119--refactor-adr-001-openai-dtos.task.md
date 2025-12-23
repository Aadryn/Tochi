# TÂCHE 119 : Refactor ADR-001 - OpenAIProviderClient (14 DTOs)

**Créée le** : 2025-12-24T00:00:00Z UTC  
**Priorité** : 🔴 Critique  
**ADR Violée** : ADR-001 - Un seul type par fichier C#

---

## CONTEXTE

**Fichier** : `OpenAIProviderClient.cs`  
**Types** : 15 (1 classe + 14 DTOs privés)  
**Lignes** : ~400

### DTOs à Extraire

1. OpenAIChatRequest
2. OpenAIMessage
3. OpenAIChatResponse
4. OpenAIChoice
5. OpenAIMessageContent
6. OpenAIUsage
7. OpenAIStreamChunk
8. OpenAIStreamDelta
9. OpenAIEmbedRequest
10. OpenAIEmbedResponse
11. OpenAIEmbeddingData
12. OpenAIEmbedUsage
13. OpenAIStreamChoice
14. OpenAIFinishReason

---

## OBJECTIF

Extraire vers `/Contracts/OpenAI/`.

---

## ÉTAPES

1. Créer `/Contracts/OpenAI/`
2. Extraire 14 DTOs (1 fichier par DTO)
3. Mettre à jour OpenAIProviderClient.cs
4. Build + Tests

---

## CRITÈRES DE SUCCÈS

- [ ] 14 fichiers créés
- [ ] OpenAIProviderClient.cs < 300 lignes
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests : 100% passing

---

## MÉTADONNÉES

- **Effort** : 5h
- **Risque** : 3/10
