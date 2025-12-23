---
description: MudBlazor Spacing Convention - Règles de spacing pour MudGrid, MudStack et layouts
name: MudBlazor_Spacing_Convention
applyTo: "**/backend/Presentation/**/*.razor"
---

# MudBlazor Spacing Convention - Harmonization Guidelines

## 📏 Convention Spacing Standard

**Audit Date:** 2025-12-04  
**Total Analyzed:** 282 occurrences dans fichiers .razor  
**Default Standard:** `Spacing="2"` (53% des cas - convention établie)

### 1. Valeurs Standard et Contextes

| Spacing | Usage | Contexte | % Actuel |
|---------|-------|----------|----------|
| `"0"` | **Ultra-Compact** | Texte empilé sans gap (logo titre/sous-titre, cellules DataGrid multi-lignes) | 3% (9 occurrences) |
| `"1"` | **Dense** | Listes denses, tables, toolbars compacts | 22% (62 occurrences) |
| `"2"` | **Default** ⭐ | Formulaires, cards, sections générales (convention standard) | **53% (149 occurrences)** |
| `"3"` | **Content** | Sections de contenu, blocs texte avec aération | 14% (40 occurrences) |
| `"4"` | **Layout** | Layout de page, grilles principales, espacement large | 6% (17 occurrences) |

### 2. Valeurs Interdites

❌ **INTERDICTION ABSOLUE** des valeurs suivantes (non-standard, corrigées lors audit 2025-12-04):
- `Spacing="5"` - Remplacé par `"4"`
- `Spacing="6"` - Remplacé par `"4"`
- Toute valeur > `"4"` sauf justification explicite avec commentaire

---

## 🎯 Règles d'Application

### Règle 1: Default Spacing="2"

**OBLIGATOIRE:** Utiliser `Spacing="2"` par défaut pour tous les nouveaux composants sauf contexte spécifique.

```razor
<!-- ✅ CORRECT - Default spacing -->
<MudStack Spacing="2">
  <MudTextField Label="Titre"/>
  <MudTextField Label="Description"/>
  <MudButton>Sauvegarder</MudButton>
</MudStack>
```

### Règle 2: Spacing="0" pour Texte Empilé

**CAS D'USAGE:** Empiler du texte sans gap (titre + sous-titre, lignes multiples dans cellules).

```razor
<!-- ✅ CORRECT - Logo avec titre/sous-titre -->
<MudStack Row="true" AlignItems="AlignItems.Center" Spacing="2">
  <MudAvatar Size="Size.Medium">
    <MudIcon Icon="@Icons.Material.Filled.FlightTakeoff"/>
  </MudAvatar>
  <MudStack Spacing="0">
    <MudText Typo="Typo.body1">Paris Aéroport</MudText>
    <MudText Typo="Typo.caption">GenAI Management</MudText>
  </MudStack>
</MudStack>

<!-- ✅ CORRECT - Cellule DataGrid multi-lignes -->
<PropertyColumn>
  <CellTemplate>
    <MudStack Spacing="0">
      <MudText Typo="Typo.body1">@context.Item.Title</MudText>
      <MudText Typo="Typo.caption">@context.Item.Description</MudText>
    </MudStack>
  </CellTemplate>
</PropertyColumn>
```

### Règle 3: Spacing="1" pour Interfaces Denses

**CAS D'USAGE:** Toolbars, filtres, listes compactes, search bars.

```razor
<!-- ✅ CORRECT - Toolbar dense -->
<MudStack Row="true" AlignItems="AlignItems.Center" Spacing="1">
  <MudIconButton Icon="@Icons.Material.Filled.Search"/>
  <MudTextField Placeholder="Rechercher..."/>
  <MudIconButton Icon="@Icons.Material.Filled.FilterList"/>
</MudStack>
```

### Règle 4: Spacing="3" pour Sections de Contenu

**CAS D'USAGE:** Blocs de contenu nécessitant aération (sections de texte, groupes de cards).

```razor
<!-- ✅ CORRECT - Sections de contenu -->
<MudStack Spacing="3">
  <MudText Typo="Typo.h6">Titre Section</MudText>
  <MudText Typo="Typo.body1">Description longue...</MudText>
  <MudDivider/>
  <MudText Typo="Typo.body2">Autre contenu...</MudText>
</MudStack>
```

### Règle 5: Spacing="4" pour Layout de Page

**CAS D'USAGE:** Layout principal de pages, grilles principales, sections de page.

```razor
<!-- ✅ CORRECT - Layout page avec sections multiples -->
<MudStack Spacing="4" Class="mb-5" Id="home-page">
  <HomeSection/>
  <SearchSection/>
  <UserFavoritePromptsSection/>
  <UserFavoriteCollectionsSection/>
  <FeaturedCollectionsSection/>
</MudStack>

<!-- ✅ CORRECT - Grid layout principal -->
<MudGrid Spacing="4">
  <MudItem xs="12" md="6">
    <MudCard>...</MudCard>
  </MudItem>
  <MudItem xs="12" md="6">
    <MudCard>...</MudCard>
  </MudItem>
</MudGrid>
```

---

## ⚠️ Anti-Patterns à Éviter

### ❌ Anti-Pattern 1: Spacing Incohérent

```razor
<!-- ❌ INCORRECT - Spacing incohérent dans même contexte -->
<MudStack Spacing="3">
  <MudTextField/>
  <MudStack Spacing="1"> <!-- Devrait être "2" -->
    <MudButton/>
    <MudButton/>
  </MudStack>
</MudStack>

<!-- ✅ CORRECT -->
<MudStack Spacing="2">
  <MudTextField/>
  <MudStack Spacing="2">
    <MudButton/>
    <MudButton/>
  </MudStack>
</MudStack>
```

### ❌ Anti-Pattern 2: Spacing > "4" Sans Justification

```razor
<!-- ❌ INCORRECT - Spacing="6" non-standard -->
<MudStack Spacing="6">
  <MudText>Contenu</MudText>
</MudStack>

<!-- ✅ CORRECT -->
<MudStack Spacing="4">
  <MudText>Contenu</MudText>
</MudStack>
```

### ❌ Anti-Pattern 3: Spacing="0" Par Défaut

```razor
<!-- ❌ INCORRECT - Spacing="0" sans justification -->
<MudStack Spacing="0">
  <MudTextField/>
  <MudButton/> <!-- Collé au textfield -->
</MudStack>

<!-- ✅ CORRECT -->
<MudStack Spacing="2">
  <MudTextField/>
  <MudButton/>
</MudStack>
```

---

## 🔍 Validation et Audit

### Commande Audit PowerShell

```powershell
# Lister toutes les occurrences Spacing dans .razor
Get-ChildItem -Path . -Filter *.razor -Recurse | 
  Select-String 'Spacing="' | 
  Export-Csv -Path ".tasks\spacing-audit.csv" -NoTypeInformation -Encoding UTF8

# Analyser distribution des valeurs
Import-Csv ".tasks\spacing-audit.csv" | 
  ForEach-Object { if ($_.Line -match 'Spacing="(\d+)"') { $matches[1] } } | 
  Group-Object | 
  Select-Object Count, Name | 
  Sort-Object Name
```

### Critères Conformité

✅ **Conforme** si:
- `Spacing="2"` utilisé pour 50%+ des cas (default)
- `Spacing="0"` uniquement pour texte empilé (< 5%)
- `Spacing="1"` pour interfaces denses (< 25%)
- `Spacing="3"` pour sections contenu (< 20%)
- `Spacing="4"` pour layouts page (< 10%)
- **AUCUNE** valeur > `"4"` sans commentaire justificatif

❌ **Non-Conforme** si:
- Valeurs `"5"`, `"6"` ou supérieures présentes
- `Spacing="0"` utilisé comme default général
- Incohérence dans même contexte (ex: formulaires avec `"1"`, `"2"`, `"3"` mélangés)

---

## 📊 Métriques Cibles

**Objectif Harmonisation:**
- **>95% conformité** : Tous fichiers respectent convention
- **<1% exceptions** : Valeurs > `"4"` uniquement avec justification
- **0 valeurs interdites** : Aucun `Spacing="5"` ou `"6"`

**Audit Historique:**
- **2025-12-04:** 282 occurrences analysées
  - Spacing="2" (53%) - ✅ Default confirmé
  - Spacing="1" (22%) - ✅ Conforme (dense)
  - Spacing="3" (14%) - ✅ Conforme (content)
  - Spacing="4" (6%) - ✅ Conforme (layout)
  - Spacing="0" (3%) - ✅ Conforme (texte empilé)
  - **3 exceptions corrigées** (Spacing="5"/"6" → "4")

---

## 🎓 Exemples Complets

### Exemple 1: Formulaire Standard

```razor
<!-- Page Create.razor - Convention complète -->
<MudStack Spacing="2">
  <MudText Typo="Typo.h5">Créer un Prompt</MudText>
  
  <MudTextField @bind-Value="Model.Title" Label="Titre" Required/>
  <MudTextField @bind-Value="Model.Description" Label="Description" Lines="3"/>
  <MudTextField @bind-Value="Model.Content" Label="Contenu" Lines="5" Required/>
  
  <MudStack Row="true" Spacing="2">
    <MudButton Color="Color.Primary" Variant="Variant.Filled">Créer</MudButton>
    <MudButton Color="Color.Default" Variant="Variant.Text" Href="/prompts">Annuler</MudButton>
  </MudStack>
</MudStack>
```

### Exemple 2: Layout Page Complexe

```razor
<!-- Home.razor - Layout page avec sections -->
<MudStack Spacing="4" Class="mb-5" Id="home-page">
  <!-- Section Hero -->
  <MudStack Spacing="2">
    <MudText Typo="Typo.h4">Bienvenue</MudText>
    <MudText Typo="Typo.body1">Description de la page</MudText>
  </MudStack>
  
  <!-- Section Search -->
  <SearchSection/>
  
  <!-- Section Favorites -->
  <UserFavoritePromptsSection/>
  
  <!-- Section Featured -->
  <FeaturedCollectionsSection/>
</MudStack>
```

### Exemple 3: DataGrid avec Cellules Multi-Lignes

```razor
<!-- List.razor - DataGrid avec cellules complexes -->
<MudDataGrid>
  <Columns>
    <PropertyColumn>
      <CellTemplate>
        <MudStack Spacing="0">
          <MudText Typo="Typo.body1" Class="cell-truncate">
            @context.Item.Title
          </MudText>
          @if (!string.IsNullOrEmpty(context.Item.Description))
          {
            <MudText Typo="Typo.caption" Color="Color.Secondary">
              @context.Item.Description
            </MudText>
          }
        </MudStack>
      </CellTemplate>
    </PropertyColumn>
  </Columns>
</MudDataGrid>
```

---

## 🔗 Références

- **Audit complet:** `.tasks/100-spacing-audit.csv`
- **Analyse distribution:** `.tasks/100-spacing-analysis.md`
- **MudBlazor Spacing Documentation:** [MudBlazor Spacing API](https://mudblazor.com/components/stack#spacing)
- **Instruction connexe:** `mudblazor.design.principles.instructions.md`

---

**Dernière mise à jour:** 2025-12-04  
**Auteur:** GitHub Copilot (Audit automatisé)  
**Statut:** ✅ Actif - Convention établie et validée
