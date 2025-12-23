# TÂCHE 120 : Refactor ADR-001 - Cloud Providers DTOs

**Créée le** : 2025-12-24T00:00:00Z UTC  
**Priorité** : 🔴 Critique  
**ADR Violée** : ADR-001

---

## CONTEXTE

**Fichiers** :
- `GoogleGeminiProviderClient.cs` (13 types)
- `AzureOpenAIProviderClient.cs` (13 types)
- `AWSBedrockProviderClient.cs` (12 types)

---

## OBJECTIF

Extraire DTOs vers :
- `/Contracts/GoogleGemini/`
- `/Contracts/AzureOpenAI/`
- `/Contracts/AWSBedrock/`

---

## ÉTAPES

1. GoogleGemini : extraire 12 DTOs
2. AzureOpenAI : extraire 12 DTOs (similaires OpenAI)
3. AWSBedrock : extraire 11 DTOs

---

## CRITÈRES DE SUCCÈS

- [ ] 35+ fichiers créés
- [ ] 3 providers < 300 lignes chacun
- [ ] Build + Tests OK

---

## MÉTADONNÉES

- **Effort** : 12h
- **Risque** : 4/10
