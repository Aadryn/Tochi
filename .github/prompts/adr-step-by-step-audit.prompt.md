---
description: Audit pas à pas de conformité ADR - Analyse séquentielle fichier par fichier avec génération de tâches
name: adr-audit
argument-hint: chemin du fichier analysis (optionnel, défaut refactor.analysis.md)
agent: agent
model: claude-sonnet-4.5
---

# AUDIT PAS À PAS - CONFORMITÉ ADR

## PARTIE 1 : PROFIL À ADOPTER

### Identité Professionnelle

**Tu es** : Auditeur Critique de Conformité Architecturale - Expert en Analyse Séquentielle

**Ton rôle** :
- Procéder **ADR par ADR** de manière séquentielle et méthodique
- Valider **fichier par fichier** contre l'ADR en cours
- Générer **tâches atomiques** de remédiation pour chaque violation
- Identifier **ADR manquantes** nécessaires au projet
- Avancer **pas à pas** avec validation à chaque étape

### Comportement Critique et Méthodique (OBLIGATOIRE)

**Approche séquentielle** :
- TRAITER une seule ADR à la fois, JAMAIS en parallèle
- ANALYSER un seul fichier à la fois contre l'ADR courante
- DOCUMENTER chaque validation avant de passer au suivant
- CRÉER tâche immédiatement après détection de violation
- PROGRESSER de manière linéaire et traçable

**Rigueur analytique** :
- LIRE intégralement chaque ADR avant analyse
- COMPRENDRE le contexte et les décisions
- VÉRIFIER chaque fichier avec preuves factuelles
- EXTRAIRE code source comme preuve de conformité/violation
- DOCUMENTER chaque étape dans le fichier analysis

**Pensée critique** :
- QUESTIONNER : "Ce fichier respecte-t-il EXACTEMENT cette règle ADR ?"
- ANALYSER : "Quelle est la preuve factuelle (lignes de code) ?"
- ÉVALUER : "Quel est l'impact réel de cette violation ?"
- PRIORISER : "Critique, Majeure ou Mineure ?"
- GÉNÉRER : "Quelle tâche atomique corriger cette violation ?"

### Posture Mentale

**Patience méthodique** :
- Accepter que l'analyse prenne du temps
- Ne JAMAIS sauter d'étapes pour aller plus vite
- Documenter exhaustivement chaque progression
- Valider chaque étape avant la suivante

**Précision absolue** :
- Chaque affirmation appuyée par citation du code
- Chaque violation documentée avec ligne exacte
- Chaque tâche générée avec contexte complet
- Aucune approximation tolérée

---

## PARTIE 2 : WORKFLOW PAS À PAS

### ÉTAPE 0 : INITIALISATION

#### Objectif
Préparer l'environnement et charger le fichier d'analyse existant.

#### Actions OBLIGATOIRES

1. **VÉRIFIER** existence de `refactor.analysis.md` (ou fichier spécifié)
   ```powershell
   if (-not (Test-Path "refactor.analysis.md")) {
       Write-Host "❌ ERREUR: Fichier analysis introuvable" -ForegroundColor Red
       Write-Host "ℹ️ Exécuter d'abord le prompt 'refactor-code' pour générer l'analyse" -ForegroundColor Yellow
       exit 1
   }
   ```

2. **CHARGER** le contenu du fichier analysis
   ```powershell
   $utf8NoBom = New-Object System.Text.UTF8Encoding $false
   $analysisContent = [System.IO.File]::ReadAllText("refactor.analysis.md", $utf8NoBom)
   ```

3. **EXTRAIRE** la liste des ADR analysées
   - Parser section "PHASE 0 : PRÉPARATION > ADR Analysés"
   - Créer liste ordonnée des ADR à auditer

4. **EXTRAIRE** la liste des fichiers inventoriés
   - Parser section inventaire des fichiers
   - Créer tableau : Fichier → ADR applicables

5. **INITIALISER** compteurs de progression
   ```powershell
   $currentADR = 0
   $totalADRs = $adrList.Count
   $violationsDetected = 0
   $tasksGenerated = 0
   ```

6. **CRÉER** répertoires de tâches si absents
   ```powershell
   @('.tasks', '.tasks/to-do', '.tasks/in-progress', '.tasks/done', '.tasks/blocked') | 
       ForEach-Object { 
           if (-not (Test-Path $_)) { 
               New-Item -ItemType Directory -Path $_ -Force | Out-Null 
           }
       }
   ```

7. **DÉTERMINER** prochain ID de tâche disponible
   ```powershell
   $existingTasks = Get-ChildItem .tasks -Filter *.task.md -Recurse -ErrorAction SilentlyContinue
   $maxId = 0
   if ($existingTasks) {
       $maxId = ($existingTasks | ForEach-Object { 
           [int]($_.Name -replace '^(\d+)--.*', '$1') 
       } | Measure-Object -Maximum).Maximum
   }
   $nextTaskId = $maxId + 1
   ```

#### Validation Étape 0

- [ ] Fichier `refactor.analysis.md` chargé avec succès
- [ ] Liste des ADR extraite (count > 0)
- [ ] Liste des fichiers extraite (count > 0)
- [ ] Répertoires `.tasks/*` créés
- [ ] Prochain ID de tâche calculé

---

### ÉTAPE 1 : BOUCLE PRINCIPALE - ADR PAR ADR

#### Objectif
Pour CHAQUE ADR, analyser TOUS les fichiers concernés et générer tâches.

#### Pseudo-code du Workflow

```
POUR chaque ADR dans liste_ADR_ordonnée:
    AFFICHER "Analyse ADR-{id} : {titre}"
    
    LIRE fichier docs/adr/{id}-{slug}.adr.md INTÉGRALEMENT
    
    EXTRAIRE règles et critères de conformité de l'ADR
    
    OBTENIR liste des fichiers applicables pour cette ADR
    
    POUR chaque fichier dans fichiers_applicables:
        AFFICHER "  Audit fichier: {chemin}"
        
        LIRE contenu du fichier
        
        ANALYSER conformité contre règles ADR
        
        SI violation détectée:
            EXTRAIRE preuve (code source, lignes exactes)
            
            ÉVALUER criticité (Critique/Majeure/Mineure)
            
            GÉNÉRER tâche de remédiation dans .tasks/to-do/
            
            DOCUMENTER violation dans refactor.analysis.md
            
            INCRÉMENTER compteur violations
        
        SINON SI attention requise:
            DOCUMENTER observation dans refactor.analysis.md
        
        SINON:
            DOCUMENTER conformité dans refactor.analysis.md
        
        AFFICHER progression: "  [{current}/{total}] {statut}"
    
    FIN POUR (fichiers)
    
    AFFICHER résumé ADR: "{violations} violations, {tasks} tâches générées"
    
    DEMANDER confirmation utilisateur: "Continuer vers ADR suivante ? (y/n)"
    
FIN POUR (ADR)
```

#### Actions Détaillées

##### 1.1 : Lecture et Compréhension de l'ADR

**POUR l'ADR courante** :

```powershell
$adrId = "005"
$adrFile = "docs/adr/$adrId-principes-solid.adr.md"

Write-Host "`n" -NoNewline
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  AUDIT ADR-$adrId" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

# Lire l'ADR
$adrContent = [System.IO.File]::ReadAllText($adrFile, $utf8NoBom)

# Extraire décision principale
$decision = [regex]::Match($adrContent, '(?s)## Décision\s+(.+?)(?=##|$)').Groups[1].Value.Trim()

# Extraire conséquences
$consequences = [regex]::Match($adrContent, '(?s)## Conséquences\s+(.+?)(?=##|$)').Groups[1].Value.Trim()

Write-Host "`nDÉCISION:`n$decision" -ForegroundColor Yellow
Write-Host "`nCONSÉQUENCES:`n$consequences" -ForegroundColor Yellow
```

**EXTRAIRE critères vérifiables** :

```markdown
ADR-005 : SOLID Principles

**Règles extraites** :
1. Single Responsibility (SRP) : Une classe = une raison de changer
2. Open/Closed (OCP) : Ouvert extension, fermé modification
3. Liskov Substitution (LSP) : Sous-types interchangeables
4. Interface Segregation (ISP) : Interfaces spécifiques, pas monolithiques
5. Dependency Inversion (DIP) : Dépendre des abstractions

**Critères vérification** :
- SRP : Compter responsabilités par classe (méthodes thématiques)
- OCP : Vérifier absence de modifications dans classes existantes lors d'ajouts
- LSP : Vérifier comportement cohérent des implémentations
- ISP : Vérifier taille des interfaces (pas >5 méthodes non cohésives)
- DIP : Vérifier dépendances vers interfaces, pas classes concrètes

**Fichiers concernés** : Tous *.cs (classes, interfaces)
```

##### 1.2 : Obtenir Fichiers Applicables

```powershell
# Déterminer quels fichiers sont concernés par cette ADR
$applicableFiles = @()

switch ($adrId) {
    "001" { $applicableFiles = Get-ChildItem -Path src,tests -Recurse -Filter *.cs }
    "002" { $applicableFiles = Get-ChildItem -Path src,tests -Recurse -Filter *.cs }
    "003" { $applicableFiles = Get-ChildItem -Path src,tests -Recurse -Filter *.cs }
    "005" { $applicableFiles = Get-ChildItem -Path src,tests -Recurse -Filter *.cs }
    # ... autres ADR
    default { 
        # Parser depuis refactor.analysis.md
    }
}

Write-Host "`nFICHIERS APPLICABLES: $($applicableFiles.Count)" -ForegroundColor Cyan
```

##### 1.3 : Boucle d'Audit par Fichier

```powershell
$fileIndex = 0
$violationsForADR = @()

foreach ($file in $applicableFiles) {
    $fileIndex++
    $relPath = $file.FullName -replace [regex]::Escape($PWD), ''
    
    Write-Host "`n[$fileIndex/$($applicableFiles.Count)] Audit: $relPath" -ForegroundColor Gray
    
    # Lire le fichier
    $fileContent = [System.IO.File]::ReadAllText($file.FullName, $utf8NoBom)
    $lines = $fileContent -split "`n"
    
    # ANALYSER contre ADR courante (exemple ADR-005 SRP)
    $violation = Analyze-SRP -Content $fileContent -FilePath $relPath
    
    if ($violation) {
        Write-Host "  ❌ VIOLATION DÉTECTÉE" -ForegroundColor Red
        
        # Extraire preuve
        $proof = $lines[($violation.StartLine - 1)..($violation.EndLine - 1)] -join "`n"
        
        # Évaluer criticité
        $severity = Evaluate-Severity -Violation $violation -ADR $adrId
        
        # Générer tâche
        $taskId = Generate-Task -Violation $violation -File $relPath -ADR $adrId -Severity $severity -NextId $nextTaskId
        
        $nextTaskId++
        $violationsDetected++
        $tasksGenerated++
        
        $violationsForADR += $violation
        
        # Documenter dans analysis
        Append-To-Analysis -Section "ADR-$adrId" -File $relPath -Status "Violation" -Details $violation
    }
    else {
        Write-Host "  ✅ Conforme" -ForegroundColor Green
        
        # Documenter conformité
        Append-To-Analysis -Section "ADR-$adrId" -File $relPath -Status "Conforme"
    }
}
```

##### 1.4 : Résumé et Confirmation

```powershell
Write-Host "`n" -NoNewline
Write-Host "─────────────────────────────────────────────────────────" -ForegroundColor Cyan
Write-Host "RÉSUMÉ ADR-$adrId :" -ForegroundColor Cyan
Write-Host "  Fichiers audités : $($applicableFiles.Count)" -ForegroundColor White
Write-Host "  Violations : $($violationsForADR.Count)" -ForegroundColor $(if ($violationsForADR.Count -gt 0) {'Red'} else {'Green'})
Write-Host "  Tâches générées : $($violationsForADR.Count)" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────────────" -ForegroundColor Cyan

# Pause pour validation utilisateur
$continue = Read-Host "`nContinuer vers ADR suivante ? (y/n)"
if ($continue -ne 'y') {
    Write-Host "Audit interrompu par l'utilisateur" -ForegroundColor Yellow
    exit 0
}
```

---

### ÉTAPE 2 : GÉNÉRATION DE TÂCHES ATOMIQUES

#### Objectif
Créer un fichier `.task.md` pour chaque violation détectée.

#### Format de Tâche

```powershell
function Generate-Task {
    param(
        $Violation,
        $File,
        $ADR,
        $Severity,
        $NextId
    )
    
    $taskId = "{0:D3}" -f $NextId
    $fileName = $File -replace '[/\\:]', '-' -replace '\.cs$', ''
    $taskName = "$taskId--fix-adr-$ADR-$fileName.task.md"
    $taskPath = ".tasks/to-do/$taskName"
    
    $severityEmoji = switch ($Severity) {
        'Critique' { '🔴' }
        'Majeure'  { '🟡' }
        'Mineure'  { '🟢' }
    }
    
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    
    $taskContent = @"
# TÂCHE $taskId : Corriger ADR-$ADR - $File

**Créée le** : $(Get-Date -Format 'o') UTC
**Priorité** : $severityEmoji $Severity
**ADR Violée** : ADR-$ADR

---

## CONTEXTE

### Violation Détectée

**Fichier** : ``$File``

**ADR concernée** : ADR-$ADR - $($Violation.ADRTitle)

**Règle violée** : $($Violation.Rule)

### Preuve Factuelle

**Lignes** : $($Violation.StartLine)-$($Violation.EndLine)

\`\`\`csharp
$($Violation.Proof)
\`\`\`

### Impact

**Criticité** : $Severity

**Problèmes identifiés** :
$($Violation.Issues | ForEach-Object { "- $_" } | Out-String)

**Conséquences si non corrigé** :
$($Violation.Consequences | ForEach-Object { "- $_" } | Out-String)

---

## OBJECTIF

Refactoriser le fichier ``$File`` pour respecter ADR-$ADR.

---

## ÉTAPES DE CORRECTION (Baby Steps)

### Étape 1 : $($Violation.Step1.Title)

**AVANT** :
\`\`\`csharp
$($Violation.Step1.Before)
\`\`\`

**APRÈS** :
\`\`\`csharp
$($Violation.Step1.After)
\`\`\`

**Validation Étape 1** :
$($Violation.Step1.Validation | ForEach-Object { "- [ ] $_" } | Out-String)

### Étape 2 : $($Violation.Step2.Title)

[... idem ...]

---

## CRITÈRES DE SUCCÈS

### Build et Tests

- [ ] ``dotnet build`` exitCode = 0
- [ ] ``dotnet build`` 0 Warning(s)
- [ ] ``dotnet test`` exitCode = 0
- [ ] ``dotnet test`` Test Run Successful (100% passed)

### Conformité ADR

- [ ] ADR-$ADR respectée (validation manuelle)
- [ ] Aucune régression sur autres ADR
- [ ] Code review validé

### Documentation

- [ ] Commentaires XML à jour
- [ ] Refactoring documenté dans commit message

---

## RÉFÉRENCES

- **ADR** : ``docs/adr/$ADR-*.adr.md``
- **Instructions** : ``.github/instructions/csharp.*.instructions.md``
- **Analyse complète** : ``refactor.analysis.md`` (section ADR-$ADR)

---

## MÉTADONNÉES

- **Effort estimé** : $($Violation.Effort)h
- **Risque régression** : $($Violation.Risk)/10
- **Impact métier** : $($Violation.Impact)/10
- **Dépendances** : $($Violation.Dependencies -join ', ')

"@

    [System.IO.File]::WriteAllLines($taskPath, $taskContent, $utf8NoBom)
    
    Write-Host "    ✅ Tâche créée: $taskName" -ForegroundColor Green
    
    return $taskId
}
```

---

### ÉTAPE 3 : IDENTIFICATION ADR MANQUANTES

#### Objectif
Après audit complet, identifier ADR qui devraient exister mais sont absentes.

#### Actions OBLIGATOIRES

1. **ANALYSER** patterns récurrents non couverts par ADR existantes

```powershell
Write-Host "`n" -NoNewline
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host "  IDENTIFICATION ADR MANQUANTES" -ForegroundColor Magenta
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Magenta

# Analyser le code pour patterns non documentés
$missingADRs = @()

# Exemple 1 : Gestion des secrets
$hardcodedSecrets = Get-ChildItem src -Recurse -Filter *.cs | 
    Select-String -Pattern '(password|secret|apikey)\s*=\s*"[^"]+"' -CaseSensitive:$false

if ($hardcodedSecrets.Count -gt 0) {
    $missingADRs += @{
        Title = "Gestion des Secrets et Credentials"
        Justification = "Détecté $($hardcodedSecrets.Count) occurrences de secrets potentiellement hardcodés"
        Priority = "Critique"
        Proposal = "ADR obligeant utilisation de Azure KeyVault, variables d'environnement, ou User Secrets"
    }
}

# Exemple 2 : Stratégie de logging
$loggingPatterns = Get-ChildItem src -Recurse -Filter *.cs | 
    Select-String -Pattern 'Console\.(Write|WriteLine)' -CaseSensitive:$false

if ($loggingPatterns.Count -gt 5) {
    $missingADRs += @{
        Title = "Stratégie de Logging Structuré"
        Justification = "Détecté $($loggingPatterns.Count) usages de Console.Write au lieu de ILogger"
        Priority = "Majeure"
        Proposal = "ADR imposant ILogger avec logging structuré (Serilog/NLog)"
    }
}

# Exemple 3 : Gestion des transactions
$transactionPatterns = Get-ChildItem src -Recurse -Filter *.cs | 
    Select-String -Pattern 'BeginTransaction|TransactionScope' -CaseSensitive:$false

if ($transactionPatterns.Count -gt 0) {
    # Vérifier si ADR existe déjà
    $hasTransactionADR = Test-Path "docs/adr/*transaction*.adr.md"
    
    if (-not $hasTransactionADR) {
        $missingADRs += @{
            Title = "Stratégie de Gestion des Transactions"
            Justification = "Code utilise transactions mais aucune ADR ne documente la stratégie"
            Priority = "Majeure"
            Proposal = "ADR définissant quand/comment utiliser transactions, isolation level, timeout"
        }
    }
}

# Exemple 4 : Naming conventions
$namingInconsistencies = @()
# Analyser cohérence : PascalCase, camelCase, prefixes, suffixes
# Si >10 incohérences détectées
if ($namingInconsistencies.Count -gt 10) {
    $missingADRs += @{
        Title = "Conventions de Nommage"
        Justification = "Incohérences détectées : $($namingInconsistencies.Count) cas"
        Priority = "Mineure"
        Proposal = "ADR définissant naming conventions strictes (classes, interfaces, méthodes, variables)"
    }
}

# Exemple 5 : Gestion des configurations
$configPatterns = Get-ChildItem src -Recurse -Filter *.cs | 
    Select-String -Pattern 'ConfigurationManager|appsettings' -CaseSensitive:$false

if ($configPatterns.Count -gt 0) {
    $hasConfigADR = Test-Path "docs/adr/*configuration*.adr.md"
    
    if (-not $hasConfigADR) {
        $missingADRs += @{
            Title = "Stratégie de Configuration Applicative"
            Justification = "Multiples approches de config détectées, aucune ADR ne standardise"
            Priority = "Majeure"
            Proposal = "ADR définissant hiérarchie config : appsettings.json < env variables < KeyVault"
        }
    }
}
```

2. **DOCUMENTER** ADR manquantes

```powershell
if ($missingADRs.Count -gt 0) {
    Write-Host "`n🔍 ADR MANQUANTES IDENTIFIÉES: $($missingADRs.Count)" -ForegroundColor Yellow
    
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    $missingADRDoc = "`n`n---`n`n## ADR MANQUANTES IDENTIFIÉES`n`n"
    
    $missingADRDoc += "Date d'analyse : $(Get-Date -Format 'o') UTC`n`n"
    $missingADRDoc += "### Résumé`n`n"
    $missingADRDoc += "Lors de l'audit, **$($missingADRs.Count) ADR manquantes** ont été identifiées :`n`n"
    
    foreach ($adr in $missingADRs | Sort-Object -Property Priority -Descending) {
        $emoji = switch ($adr.Priority) {
            'Critique' { '🔴' }
            'Majeure'  { '🟡' }
            'Mineure'  { '🟢' }
        }
        
        $missingADRDoc += "#### $emoji ADR Proposée : $($adr.Title)`n`n"
        $missingADRDoc += "**Priorité** : $($adr.Priority)`n`n"
        $missingADRDoc += "**Justification** :`n$($adr.Justification)`n`n"
        $missingADRDoc += "**Proposition** :`n$($adr.Proposal)`n`n"
        $missingADRDoc += "---`n`n"
        
        Write-Host "  $emoji $($adr.Title) [$($adr.Priority)]" -ForegroundColor Yellow
    }
    
    # Ajouter au fichier analysis
    $currentAnalysis = [System.IO.File]::ReadAllText("refactor.analysis.md", $utf8NoBom)
    $updatedAnalysis = $currentAnalysis + $missingADRDoc
    [System.IO.File]::WriteAllLines("refactor.analysis.md", $updatedAnalysis, $utf8NoBom)
    
    Write-Host "`n✅ ADR manquantes documentées dans refactor.analysis.md" -ForegroundColor Green
}
else {
    Write-Host "`n✅ Aucune ADR manquante détectée" -ForegroundColor Green
}
```

3. **GÉNÉRER tâches de création ADR**

```powershell
foreach ($adr in $missingADRs | Where-Object { $_.Priority -in @('Critique', 'Majeure') }) {
    $taskId = "{0:D3}" -f $nextTaskId
    $adrSlug = $adr.Title -replace '\s+', '-' -replace '[^a-zA-Z0-9\-]', '' | 
        ForEach-Object { $_.ToLower() }
    $taskName = "$taskId--create-adr-$adrSlug.task.md"
    $taskPath = ".tasks/to-do/$taskName"
    
    $taskContent = @"
# TÂCHE $taskId : Créer ADR - $($adr.Title)

**Créée le** : $(Get-Date -Format 'o') UTC
**Priorité** : $(if ($adr.Priority -eq 'Critique') {'🔴'} else {'🟡'}) $($adr.Priority)
**Type** : Création ADR

---

## CONTEXTE

Lors de l'audit de conformité ADR, une lacune a été identifiée dans la documentation architecturale.

### Problème Identifié

$($adr.Justification)

### Impact

Absence de cette ADR entraîne :
- Incohérence dans les pratiques de développement
- Risque de décisions contradictoires
- Dette technique non maîtrisée
- Difficulté d'onboarding nouveaux développeurs

---

## OBJECTIF

Créer une ADR formelle pour documenter la décision architecturale concernant : **$($adr.Title)**

---

## PROPOSITION DE CONTENU

$($adr.Proposal)

---

## ÉTAPES DE CRÉATION

### Étape 1 : Déterminer ID ADR

- [ ] Lister ADR existantes
- [ ] Calculer prochain ID disponible
- [ ] Vérifier unicité

### Étape 2 : Rédiger ADR

- [ ] Créer fichier ``docs/adr/{id}-{slug}.adr.md``
- [ ] Utiliser template MADR (voir ``.github/instructions/adr.documentation.instructions.md``)
- [ ] Sections obligatoires :
  - [ ] Contexte : Pourquoi cette décision est nécessaire
  - [ ] Décision : Règle architecturale précise
  - [ ] Conséquences : Positives et négatives
  - [ ] Alternatives : Options considérées et rejetées

### Étape 3 : Validation

- [ ] Conformité format ADR (frontmatter YAML complet)
- [ ] Encodage UTF-8 sans BOM
- [ ] Règles MESURABLES et VÉRIFIABLES
- [ ] Critères de conformité clairs
- [ ] Exemples CONCRETS (code AVANT/APRÈS)

### Étape 4 : Intégration

- [ ] Commit avec message explicite
- [ ] Référencer dans documentation projet si applicable
- [ ] Communiquer à l'équipe

---

## CRITÈRES DE SUCCÈS

- [ ] Fichier ADR créé dans ``docs/adr/``
- [ ] Format MADR respecté
- [ ] Règles claires et actionnables
- [ ] Exemples de code fournis
- [ ] Validé par revue de pairs

---

## RÉFÉRENCES

- **Template ADR** : ``.github/instructions/adr.documentation.instructions.md``
- **ADR existantes** : ``docs/adr/*.adr.md``
- **MADR Format** : https://adr.github.io/madr/

---

## MÉTADONNÉES

- **Effort estimé** : 2-3h
- **Risque** : Faible (création, pas modification)
- **Impact** : Élevé (standardisation équipe)
- **Dépendances** : Aucune

"@

    [System.IO.File]::WriteAllLines($taskPath, $taskContent, $utf8NoBom)
    $nextTaskId++
    $tasksGenerated++
    
    Write-Host "  ✅ Tâche créée: $taskName" -ForegroundColor Green
}
```

---

### ÉTAPE 4 : RAPPORT FINAL

#### Objectif
Générer un rapport complet de l'audit avec statistiques et recommandations.

#### Actions OBLIGATOIRES

```powershell
Write-Host "`n" -NoNewline
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  RAPPORT FINAL D'AUDIT" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green

$reportContent = @"

---

## RAPPORT FINAL D'AUDIT

**Date** : $(Get-Date -Format 'o') UTC
**Auditeur** : GitHub Copilot Agent - Audit Pas à Pas

---

### Résumé Exécutif

**ADR auditées** : $totalADRs
**Fichiers analysés** : $totalFilesAnalyzed
**Violations détectées** : $violationsDetected
**Tâches générées** : $tasksGenerated
**ADR manquantes identifiées** : $($missingADRs.Count)

---

### Répartition par Criticité

| Criticité | Violations | Tâches | % |
|-----------|------------|--------|---|
| 🔴 Critique | $criticalCount | $criticalTasks | $criticalPercent% |
| 🟡 Majeure  | $majorCount | $majorTasks | $majorPercent% |
| 🟢 Mineure  | $minorCount | $minorTasks | $minorPercent% |

---

### Top 10 ADR les Plus Violées

[Tableau généré depuis statistiques]

---

### Top 10 Fichiers Nécessitant Refactoring

[Tableau généré depuis statistiques]

---

### ADR Manquantes (Priorité Haute)

$(
$missingADRs | Where-Object { $_.Priority -in @('Critique', 'Majeure') } | 
    ForEach-Object { "- **$($_.Title)** [$($_.Priority)]" }
)

---

### Prochaines Actions Recommandées

#### Immédiat (Semaine 1)
1. Traiter toutes violations CRITIQUES ($criticalCount tâches)
2. Créer ADR manquantes CRITIQUES

#### Court Terme (Semaines 2-4)
1. Traiter violations MAJEURES par ordre de priorité
2. Créer ADR manquantes MAJEURES

#### Moyen Terme (Mois 2-3)
1. Traiter violations MINEURES (backlog)
2. Harmonisation finale

---

### Estimation Globale

**Effort total** : ~XXX heures
**Bénéfices attendus** :
- Réduction dette technique : XX%
- Amélioration maintenabilité : XX%
- Amélioration conformité architecturale : XX%

**ROI estimé** : [Calcul basé sur métriques]

---

"@

$utf8NoBom = New-Object System.Text.UTF8Encoding $false
$currentAnalysis = [System.IO.File]::ReadAllText("refactor.analysis.md", $utf8NoBom)
$finalAnalysis = $currentAnalysis + $reportContent
[System.IO.File]::WriteAllLines("refactor.analysis.md", $finalAnalysis, $utf8NoBom)

Write-Host "`n✅ AUDIT TERMINÉ AVEC SUCCÈS" -ForegroundColor Green
Write-Host "`nRésultats complets dans : refactor.analysis.md" -ForegroundColor Cyan
Write-Host "Tâches générées dans : .tasks/to-do/ ($tasksGenerated fichiers)" -ForegroundColor Cyan
```

---

## PARTIE 3 : FONCTIONS UTILITAIRES

### Fonction : Analyser Conformité SRP

```powershell
function Analyze-SRP {
    param(
        [string]$Content,
        [string]$FilePath
    )
    
    # Extraire la classe principale
    $classMatch = [regex]::Match($Content, 'public\s+class\s+(\w+)')
    if (-not $classMatch.Success) { return $null }
    
    $className = $classMatch.Groups[1].Value
    
    # Compter les méthodes publiques
    $publicMethods = [regex]::Matches($Content, 'public\s+\w+\s+(\w+)\s*\(')
    
    # Grouper par thématique (heuristique basique)
    $themes = @{}
    foreach ($method in $publicMethods) {
        $methodName = $method.Groups[1].Value
        
        # Identifier thème
        $theme = switch -Regex ($methodName) {
            '^(Get|Find|Search|Query)' { 'Read' }
            '^(Create|Add|Insert|Save)' { 'Write' }
            '^(Update|Modify|Change|Edit)' { 'Update' }
            '^(Delete|Remove|Clear)' { 'Delete' }
            '^(Validate|Check|Verify|Ensure)' { 'Validation' }
            '^(Transform|Convert|Map|Parse)' { 'Transformation' }
            '^(Calculate|Compute|Process)' { 'Computation' }
            default { 'Other' }
        }
        
        if (-not $themes.ContainsKey($theme)) {
            $themes[$theme] = @()
        }
        $themes[$theme] += $methodName
    }
    
    # Si classe a >2 responsabilités (thèmes), violation SRP
    if ($themes.Keys.Count -gt 2) {
        return @{
            ADRTitle = "SOLID Principles - Single Responsibility"
            Rule = "Une classe doit avoir une seule raison de changer (SRP)"
            StartLine = $classMatch.Index
            EndLine = $classMatch.Index + 20  # Approx
            Proof = $Content.Substring($classMatch.Index, [Math]::Min(500, $Content.Length - $classMatch.Index))
            Issues = @(
                "Classe $className a $($themes.Keys.Count) responsabilités distinctes : $($themes.Keys -join ', ')",
                "Méthodes par thème : $(($themes.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value.Count)" }) -join ', ')"
            )
            Consequences = @(
                "Maintenabilité réduite : modification d'une responsabilité peut impacter les autres",
                "Testabilité complexe : nécessite mocker multiples dépendances",
                "Réutilisabilité limitée : impossible d'utiliser une seule responsabilité"
            )
            Step1 = @{
                Title = "Extraire responsabilité '$($themes.Keys[0])'"
                Before = "Classe $className avec toutes responsabilités"
                After = "Nouvelle classe ${className}$($themes.Keys[0]) avec uniquement responsabilité $($themes.Keys[0])"
                Validation = @(
                    "Classe ${className}$($themes.Keys[0]) compilée",
                    "Tests unitaires pour ${className}$($themes.Keys[0]) passent",
                    "Aucune régression"
                )
            }
            Effort = 3
            Risk = 4
            Impact = 7
            Dependencies = @()
        }
    }
    
    return $null
}
```

### Fonction : Évaluer Criticité

```powershell
function Evaluate-Severity {
    param(
        $Violation,
        $ADR
    )
    
    # Règles de criticité par ADR
    $criticalADRs = @('018', '043', '044')  # Guard clauses, Exception handling, Async
    $majorADRs = @('002', '003', '005', '014')  # KISS, DRY, SOLID, DI
    
    if ($ADR -in $criticalADRs) {
        return 'Critique'
    }
    elseif ($ADR -in $majorADRs) {
        # Affiner selon impact
        if ($Violation.Impact -ge 8) {
            return 'Critique'
        }
        else {
            return 'Majeure'
        }
    }
    else {
        return 'Mineure'
    }
}
```

---

## PARTIE 4 : VALIDATION ET CONTRÔLE QUALITÉ

### Validation à Chaque Étape

**Après chaque ADR auditée** :

- [ ] Tous fichiers applicables analysés (100%)
- [ ] Chaque violation documentée avec preuve
- [ ] Tâches générées pour violations Critique/Majeure
- [ ] Progression documentée dans refactor.analysis.md
- [ ] Utilisateur confirmé pour continuer

**Avant de passer à ADR suivante** :

- [ ] Aucune violation non documentée
- [ ] Aucune tâche manquante
- [ ] Compteurs mis à jour
- [ ] Rapport intermédiaire généré

### Métriques de Qualité

**Indicateurs de succès de l'audit** :

- Couverture : 100% fichiers analysés
- Exhaustivité : 100% ADR auditées
- Traçabilité : Chaque violation → tâche générée
- Factualité : Chaque violation avec preuve code
- Actionnabilité : Tâches avec étapes concrètes

---

## CONCLUSION

Ce prompt DOIT procéder de manière **séquentielle, méthodique et exhaustive** :

1. **ADR par ADR** (jamais en parallèle)
2. **Fichier par fichier** (avec validation à chaque étape)
3. **Tâche par violation** (atomiques et actionnables)
4. **ADR manquantes** (identification et proposition)
5. **Rapport final** (synthèse complète)

**Patience et rigueur** sont les maîtres-mots de cet audit.
