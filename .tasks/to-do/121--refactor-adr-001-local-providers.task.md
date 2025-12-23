# TÂCHE 121 : Refactor ADR-001 - Local Providers DTOs

**Créée le** : 2025-12-24T00:00:00Z UTC  
**Priorité** : 🟡 Majeure  
**ADR Violée** : ADR-001

---

## CONTEXTE

**Fichiers** :
- `OllamaProviderClient.cs` (11 types)
- `HuggingFaceProviderClient.cs` (12 types)
- `AnthropicProviderClient.cs` (10 types)

---

## OBJECTIF

Extraire DTOs vers :
- `/Contracts/Ollama/`
- `/Contracts/HuggingFace/`
- `/Contracts/Anthropic/`

---

## ÉTAPES

1. Ollama : extraire 10 DTOs
2. HuggingFace : extraire 11 DTOs
3. Anthropic : extraire 9 DTOs

---

## CRITÈRES DE SUCCÈS

- [ ] 30+ fichiers créés
- [ ] 3 providers < 250 lignes chacun
- [ ] Build + Tests OK

---

## MÉTADONNÉES

- **Effort** : 6h
- **Risque** : 3/10
