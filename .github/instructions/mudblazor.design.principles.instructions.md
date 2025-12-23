---
description: Design minimaliste MudBlazor - Palette clair/gris/blanc, espacement, typographie, icônes
name: MudBlazor_Design_Principles
applyTo: "**Presentation/**/*.razor,**Presentation/**/*.razor.cs"
---

# MudBlazor - Principes de Design Minimaliste

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** de couleurs hors de la palette définie (#0288d1, #f5f5f5, #ffffff)
- **N'applique jamais** `Elevation` >2 (design plat et minimaliste)
- **Ne mélange jamais** plusieurs typographies différentes sur un même écran
- **N'utilise jamais** d'icônes hors de Material Design (`Icons.Material.*`)
- **Ne surcharge jamais** visuellement les interfaces (moins = mieux)
- **N'utilise jamais** de `Spacing` >4 entre composants standards
- **N'ajoute jamais** de décorations superflues (ombres, bordures excessives)

## ✅ À FAIRE

- **Utilise toujours** la palette clair/gris/blanc définie (Primary=#0288d1, Background=#f5f5f5)
- **Applique toujours** `Elevation="0"` ou `Elevation="1"` maximum
- **Utilise toujours** la hiérarchie typographique (h5/h6 pour titres, body1/body2 pour texte)
- **Utilise toujours** les icônes Material Design uniquement
- **Respecte toujours** l'espacement cohérent (`Spacing="3"` ≈ 24px standard)
- **Préfère toujours** les espaces blancs aux séparateurs visuels
- **Maintiens toujours** un contraste suffisant pour l'accessibilité

## 🎨 Philosophie de Design

### Design Minimaliste Clair/Gris/Blanc

**Palette de couleurs OBLIGATOIRE :**
```csharp
private readonly MudTheme _theme = new()
{
  PaletteLight = new PaletteLight
  {
    Primary = "#0288d1",        / Bleu clair principal
    Secondary = "#78909c",      / Gris-bleu secondaire
    Background = "#f5f5f5",     / Gris très clair
    Surface = "#ffffff",        / Blanc
    AppbarBackground = "#ffffff", / Blanc pour AppBar
    DrawerBackground = "#fafafa", / Gris ultra-clair pour Drawer
    TextPrimary = "#212121",    / Gris très foncé pour texte principal
    TextSecondary = "#757575",  / Gris moyen pour texte secondaire
    Divider = "#e0e0e0",        / Gris clair pour séparateurs
    LinesDefault = "#e0e0e0"    / Gris clair pour bordures
  }
};
```

**Principes du design minimaliste :**
- ✅ Espaces blancs généreux (padding, margin)
- ✅ Bordures subtiles (#e0e0e0) au lieu d'ombres fortes
- ✅ Élévations minimales (Elevation="0" ou "1")
- ✅ Typographie claire et lisible
- ✅ Icônes Material Design uniquement
- ❌ Pas de dégradés colorés
- ❌ Pas d'ombres portées lourdes
- ❌ Pas de couleurs vives multiples

## 📏 Espacement Cohérent

### Conventions d'espacement

**Espacements standards :**
- Padding des containers : `padding: 24px` ou `Spacing="3"`
- Spacing entre éléments : `Spacing="3"` (≈24px)
- Margin entre sections : `margin-bottom: 24px`
- Padding des cards : `padding: 20px`
- Spacing compact : `Spacing="2"` (≈16px)
- Spacing dense : `Spacing="1"` (≈8px)

**Échelle d'espacement MudBlazor :**
```
Spacing="0"  →  0px
Spacing="1"  →  8px
Spacing="2"  →  16px
Spacing="3"  →  24px (RECOMMANDÉ par défaut)
Spacing="4"  →  32px
Spacing="5"  →  40px
```

**Exemples :**
```razor
<!-- Container principal -->
<MudContainer MaxWidth="MaxWidth.False" Style="padding: 24px;">
  
  <!-- Section avec espacement généreux -->
  <MudStack Spacing="3">
    <MudText Typo="Typo.h5">Titre</MudText>
    <MudText Typo="Typo.body1">Contenu</MudText>
  </MudStack>
  
  <!-- Card avec padding interne -->
  <MudPaper Elevation="0" Class="card-stat">
    <MudStack Spacing="2">
      <!-- Contenu espacé -->
    </MudStack>
  </MudPaper>
</MudContainer>
```

## 📝 Typographie

### Hiérarchie typographique

```razor
<!-- Titre de page (h1 équivalent) -->
<MudText Typo="Typo.h5" Class="page-title">
  Titre Principal de Page
</MudText>

<!-- Titre de section (h2 équivalent) -->
<MudText Typo="Typo.h6" Class="section-title">
  Titre de Section
</MudText>

<!-- Corps de texte principal -->
<MudText Typo="Typo.body1" Color="Color.Default">
  Texte principal
</MudText>

<!-- Texte secondaire -->
<MudText Typo="Typo.body2" Color="Color.Secondary">
  Texte secondaire ou description
</MudText>

<!-- Caption / légende -->
<MudText Typo="Typo.caption" Color="Color.Secondary">
  Légende ou note explicative
</MudText>

<!-- Surtitre / overline -->
<MudText Typo="Typo.overline" Color="Color.Secondary">
  CATÉGORIE
</MudText>

<!-- Sous-titre -->
<MudText Typo="Typo.subtitle1">
  Sous-titre important
</MudText>

<MudText Typo="Typo.subtitle2" Color="Color.Secondary">
  Sous-titre secondaire
</MudText>
```

**Correspondance Typo et tailles :**
```
Typo.h1        → 96px  (Rarement utilisé)
Typo.h2        → 60px  (Rarement utilisé)
Typo.h3        → 48px  (Titres de landing pages)
Typo.h4        → 34px  (Titres de modales)
Typo.h5        → 24px  (Titres de pages - RECOMMANDÉ)
Typo.h6        → 20px  (Titres de sections - RECOMMANDÉ)
Typo.subtitle1 → 16px
Typo.subtitle2 → 14px
Typo.body1     → 16px  (Texte principal - RECOMMANDÉ)
Typo.body2     → 14px  (Texte secondaire - RECOMMANDÉ)
Typo.caption   → 12px  (Légendes)
Typo.overline  → 10px  (Surtitres en majuscules)
```

### Classes CSS pour typographie

```css
/* wwwroot/app.css */

.page-title {
  font-weight: 600;
  color: #212121;
  line-height: 1.2;
  margin-bottom: 24px;
}

.section-title {
  font-weight: 600;
  color: #212121;
  line-height: 1.3;
  margin-bottom: 16px;
}

.text-truncate {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.text-truncate-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
```

## 🎨 Icônes

### Utiliser uniquement Material Icons

```razor
@using static MudBlazor.Icons.Material.Filled

<!-- Icônes de navigation -->
<MudIcon Icon="@Dashboard" Color="Color.Primary"/>
<MudIcon Icon="@Person" Color="Color.Default"/>
<MudIcon Icon="@Settings" Color="Color.Default"/>
<MudIcon Icon="@Notifications" Color="Color.Default"/>
<MudIcon Icon="@Menu" Color="Color.Default"/>

<!-- Icônes d'actions -->
<MudIcon Icon="@Add" Color="Color.Primary"/>
<MudIcon Icon="@Edit" Color="Color.Default"/>
<MudIcon Icon="@Delete" Color="Color.Default"/>
<MudIcon Icon="@Save" Color="Color.Primary"/>
<MudIcon Icon="@Cancel" Color="Color.Default"/>
<MudIcon Icon="@Close" Color="Color.Default"/>

<!-- Icônes de recherche et filtrage -->
<MudIcon Icon="@Search" Color="Color.Default"/>
<MudIcon Icon="@FilterList" Color="Color.Default"/>
<MudIcon Icon="@Sort" Color="Color.Default"/>

<!-- Icônes de statut -->
<MudIcon Icon="@Check" Color="Color.Success"/>
<MudIcon Icon="@CheckCircle" Color="Color.Success"/>
<MudIcon Icon="@Error" Color="Color.Error"/>
<MudIcon Icon="@Warning" Color="Color.Warning"/>
<MudIcon Icon="@Info" Color="Color.Info"/>

<!-- Icônes de contenu -->
<MudIcon Icon="@Collections" Color="Color.Default"/>
<MudIcon Icon="@Folder" Color="Color.Default"/>
<MudIcon Icon="@Description" Color="Color.Default"/>
<MudIcon Icon="@Image" Color="Color.Default"/>
```

**Tailles d'icônes :**
```razor
<MudIcon Icon="@Dashboard" Size="Size.Small"/>   <!-- 16px -->
<MudIcon Icon="@Dashboard" Size="Size.Medium"/>  <!-- 24px, défaut - RECOMMANDÉ -->
<MudIcon Icon="@Dashboard" Size="Size.Large"/>   <!-- 32px -->
```

**Icônes avec boutons :**
```razor
<!-- IconButton -->
<MudIconButton Icon="@Icons.Material.Filled.Edit" 
               Size="Size.Small" 
               Color="Color.Default"
               AriaLabel="@Localizer["Common.Edit"]"/>

<!-- Bouton avec icône de démarrage -->
<MudButton Variant="Variant.Filled" 
           Color="Color.Primary" 
           StartIcon="@Icons.Material.Filled.Add">
  @Localizer["Common.Add"]
</MudButton>

<!-- Bouton avec icône de fin -->
<MudButton Variant="Variant.Outlined" 
           Color="Color.Default" 
           EndIcon="@Icons.Material.Filled.ArrowForward">
  @Localizer["Common.Next"]
</MudButton>
```

## 🎨 Couleurs et États

### Codes couleurs du thème

```css
/* Couleurs principales */
--color-primary: #0288d1;        /* Bleu clair */
--color-secondary: #78909c;      /* Gris-bleu */
--color-background: #f5f5f5;     /* Gris très clair */
--color-surface: #ffffff;        /* Blanc */

/* Couleurs de texte */
--text-primary: #212121;         /* Gris très foncé */
--text-secondary: #757575;       /* Gris moyen */
--text-disabled: #bdbdbd;        /* Gris clair */

/* Couleurs de bordure */
--border-color: #e0e0e0;         /* Gris clair */

/* Couleurs sémantiques */
--color-success: #4caf50;        /* Vert */
--color-warning: #ff9800;        /* Orange */
--color-error: #f44336;          /* Rouge */
--color-info: #2196f3;           /* Bleu */
```

### Utilisation des couleurs MudBlazor

```razor
<!-- Couleurs principales -->
<MudButton Color="Color.Primary">Primaire</MudButton>
<MudButton Color="Color.Secondary">Secondaire</MudButton>
<MudButton Color="Color.Default">Défaut</MudButton>

<!-- Couleurs sémantiques -->
<MudButton Color="Color.Success">Succès</MudButton>
<MudButton Color="Color.Warning">Avertissement</MudButton>
<MudButton Color="Color.Error">Erreur</MudButton>
<MudButton Color="Color.Info">Information</MudButton>

<!-- Couleurs de texte -->
<MudText Color="Color.Primary">Texte primaire</MudText>
<MudText Color="Color.Secondary">Texte secondaire</MudText>
<MudText Color="Color.Default">Texte par défaut</MudText>

<!-- Couleurs d'icônes -->
<MudIcon Icon="@Icons.Material.Filled.Check" Color="Color.Success"/>
<MudIcon Icon="@Icons.Material.Filled.Error" Color="Color.Error"/>
<MudIcon Icon="@Icons.Material.Filled.Warning" Color="Color.Warning"/>
```

## 🎯 Élévations et Bordures

### Élévations minimales (Material Design)

```razor
<!-- Élévation 0 (RECOMMANDÉ - Design plat avec bordures) -->
<MudPaper Elevation="0" Class="card-stat">
  <!-- Contenu -->
</MudPaper>

<!-- Élévation 1 (Alternative acceptable) -->
<MudPaper Elevation="1" Class="card-section">
  <!-- Contenu -->
</MudPaper>

<!-- ❌ ÉVITER : Élévations fortes (>2) -->
<MudPaper Elevation="8">
  <!-- Trop d'ombre -->
</MudPaper>
```

### Bordures subtiles

```css
/* wwwroot/app.css */

/* Bordures recommandées */
.border-light {
  border: 1px solid #e0e0e0;
}

.border-bottom {
  border-bottom: 1px solid #e0e0e0;
}

.border-top {
  border-top: 1px solid #e0e0e0;
}

/* Rayons de bordure */
.border-radius-sm {
  border-radius: 4px;
}

.border-radius-md {
  border-radius: 8px;
}
```

## 📋 Checklist Design Minimaliste

### ✅ Palette de Couleurs
- [ ] Palette clair/gris/blanc respectée (#0288d1, #f5f5f5, #ffffff, #e0e0e0)
- [ ] Texte principal en #212121, secondaire en #757575
- [ ] Bordures en #e0e0e0
- [ ] Aucune couleur vive multiple
- [ ] Aucun dégradé coloré

### ✅ Espacement
- [ ] Espacement généreux (Spacing="3" ≈ 24px par défaut)
- [ ] Padding des cards : 20-24px
- [ ] Spacing entre sections : 24px
- [ ] Cohérence dans toute l'application

### ✅ Typographie
- [ ] Typo.h5 pour titres de pages
- [ ] Typo.h6 pour titres de sections
- [ ] Typo.body1 pour texte principal
- [ ] Typo.body2 pour texte secondaire
- [ ] Font-weight: 600 pour titres importants

### ✅ Icônes
- [ ] Material Design uniquement
- [ ] Size.Medium par défaut (24px)
- [ ] Couleurs cohérentes avec le thème
- [ ] AriaLabel sur tous les IconButton

### ✅ Élévations
- [ ] Elevation="0" ou "1" uniquement
- [ ] Bordures grises (#e0e0e0) au lieu d'ombres fortes
- [ ] Aucune ombre portée lourde

## 🔍 Validation Automatique

```powershell
# Vérifier les dégradés colorés
Get-ChildItem -Recurse -Filter "*.{razor,css}" | 
  Select-String -Pattern "gradient" | 
  Select-Object Path, LineNumber

# Vérifier les élévations fortes
Get-ChildItem -Recurse -Filter "*.razor" | 
  Select-String -Pattern 'Elevation="[3-9]|1[0-9]|2[0-4]"' | 
  Select-Object Path, LineNumber

# Vérifier les couleurs non conformes
Get-ChildItem -Recurse -Filter "*.{razor,css}" | 
  Select-String -Pattern '#(?!0288d1|78909c|f5f5f5|ffffff|fafafa|212121|757575|e0e0e0|4caf50|ff9800|f44336|2196f3|bdbdbd)[0-9a-f]{6}' -CaseSensitive:$false | 
  Select-Object Path, LineNumber
```

## 📚 Ressources

### Documentation Officielle
- [MudBlazor Colors](https:/mudblazor.com/features/colors)
- [Material Design Guidelines - Color](https:/material.io/design/color)
- [Material Design Guidelines - Typography](https:/material.io/design/typography)
- [Material Design Icons](https:/fonts.google.com/icons)
