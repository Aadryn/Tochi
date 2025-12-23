---
applyTo: "tasks/**/*.md"
---

# Instructions pour la Création de Tâches Atomiques

## Principes Fondamentaux

### Atomicité Stricte
- **UNE tâche = UN sujet technique précis** (une entité, un repository, une commande CQRS)
- **Durée maximale : 2-8 heures** par tâche
- **Si > 8h** : découper obligatoirement en sous-tâches
- **Aucune ambiguïté** : fichiers exacts, classes exactes, méthodes exactes spécifiées

### Langue et Format
- **Langue obligatoire** : Français pour tout le contenu
- **Format** : Markdown structuré avec metadata table, sections et checklists
- **Nomenclature** : `TASK-XXX-[nom-court-kebab-case].md`
- **Emplacement initial** : `tasks/to-do/`

### Approche TDD Obligatoire
Chaque tâche **DOIT** suivre le cycle RED-GREEN-REFACTOR :
1. **RED** : Écrire les tests unitaires qui échouent (45min)
2. **GREEN** : Implémenter le code minimal pour passer les tests (1h30)
3. **REFACTOR** : Améliorer le code tout en gardant les tests verts (30min)

---

## Structure Obligatoire des Tâches

### Template Complet

```markdown
# TASK-XXX : [Titre Court et Précis en Français]

| **Metadata**        | **Value**                                          |
|---------------------|----------------------------------------------------|
| **Status**          | 🔵 To Do                                           |
| **Priority**        | 🔴 HIGH / 🟡 MEDIUM / 🟢 LOW                       |
| **Estimated Effort**| X heures (2-8h max)                                |
| **Actual Effort**   | [Rempli une fois terminé]                          |
| **Sprint**          | Sprint N (Week X-Y)                                |
| **Dependencies**    | TASK-XXX, TASK-XXY                                 |
| **Related ADRs**    | ADR-XXX, ADR-XXY (liens directs obligatoires)      |
| **Related US**      | US-XXX (User Story parente)                        |
| **Assigned To**     | [Nom développeur ou GitHub Copilot]               |
| **Created**         | YYYY-MM-DD                                         |
| **Completed**       | [Date de complétion]                               |

---

## 📋 Analysis

### Contexte Fonctionnel
[Pourquoi cette tâche existe ? Quelle User Story ou Feature implémente-t-elle ?]
[Quel besoin métier adresse-t-elle ?]

### Problème Technique à Résoudre
[Description précise du problème technique]
[État actuel vs état cible]

### Périmètre Exact

**Fichiers à CRÉER** :
- `sources/[chemin-exact]/[Fichier1.cs]` - [Description précise]
- `sources/[chemin-exact]/[Fichier1.Units.Tests.cs]` - Tests unitaires associés

**Fichiers à MODIFIER** :
- `sources/[chemin-exact]/[Fichier2.cs]` - [Modifications précises : lignes X-Y]
  - Ajouter méthode `MethodName` à ligne X
  - Modifier propriété `PropertyName` à ligne Y

**Fichiers à SUPPRIMER** :
- `sources/[chemin-exact]/[ObsoleteFichier.cs]` - [Raison de suppression]

**Classes/Interfaces/Méthodes** :
- Interface `INomInterface` : Créer avec méthodes X, Y, Z
- Classe `NomClasse` : Implémenter INomInterface avec pattern X
- ValueObject `NomValueObject` : Créer avec validations A, B, C

### Impact Architectural

**ADRs Applicables** (OBLIGATOIRE) :
- **ADR-001 (Onion Architecture)** : [Impact spécifique - ex: Couche Domain, pas de dépendance vers Infrastructure]
- **ADR-002 (CQRS/MediatR)** : [Impact spécifique - ex: Créer Command + Handler]
- **ADR-008 (Result Pattern)** : [Impact spécifique - ex: Retourner Result<T> pour gestion erreurs]
- **ADR-010 (Testing Strategy)** : [Impact spécifique - ex: 80% coverage, mutation 85%+]
- **ADR-013 (Naming Conventions)** : [Impact spécifique - ex: PascalCase, suffixes Command/Query]
- **ADR-030 (Dependencies Isolation)** : [Impact spécifique - ex: Wrapper pour librairie tierce]

**Dépendances entre Couches** (Onion Architecture) :
```
Domain (Core)
  └─> Aucune dépendance externe ✅
Infrastructure
  └─> Dépend de Domain.Abstractions ✅
Application
  └─> Dépend de Domain + Domain.Abstractions ✅
Hosting
  └─> Dépend de Application + Infrastructure (DI uniquement) ✅
```

---

## 🎯 Implementation Steps (TDD Obligatoire)

### Étape 0 : Préparation Git (5min)

**Créer branche dédiée** :
```bash
git checkout -b features/tasks/TASK-XXX-[description-courte]
git push -u origin features/tasks/TASK-XXX-[description-courte]
```

**Vérifications préalables** :
- [ ] `dotnet build janus.sln --verbosity quiet` : ✅ SUCCESS
- [ ] `dotnet test janus.sln --no-build` : ✅ ALL PASS
- [ ] Aucune tâche similaire dans `tasks/done/` ou `tasks/to-do/`

---

### Étape 1 : RED - Tests Unitaires qui Échouent (45min)

**Créer projet de tests si nécessaire** :
```bash
cd sources/[couche]/src
dotnet new xunit -n Exakis.Janus.[Projet].Units.Tests
dotnet add Exakis.Janus.[Projet].Units.Tests reference Exakis.Janus.[Projet]
dotnet add Exakis.Janus.[Projet].Units.Tests package xUnit
dotnet add Exakis.Janus.[Projet].Units.Tests package NSubstitute
dotnet add Exakis.Janus.[Projet].Units.Tests package NFluent
dotnet sln add Exakis.Janus.[Projet].Units.Tests
```

**Créer fichier de tests** : `sources/.../[Projet].Units.Tests/[NomTests].cs`

```csharp
namespace Exakis.Janus.[Namespace].Units.Tests;

public sealed class [NomClasse]Tests
{
    [Fact]
    public void MethodName_ValidInput_ReturnsExpectedResult()
    {
        // Arrange
        var sut = new [NomClasse]();
        var input = [valeur-test];
        
        // Act
        var result = sut.MethodName(input);
        
        // Assert
        Check.That(result).IsNotNull();
        Check.That(result.Value).IsEqualTo(expected);
    }
    
    [Fact]
    public void MethodName_InvalidInput_ReturnsError()
    {
        // Arrange
        var sut = new [NomClasse]();
        var invalidInput = [valeur-invalide];
        
        // Act
        var result = sut.MethodName(invalidInput);
        
        // Assert
        Check.That(result.IsFailure).IsTrue();
        Check.That(result.Error.Code).IsEqualTo("EXPECTED-ERROR-CODE");
    }
    
    [Fact]
    public void MethodName_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new [NomClasse]();
        
        // Act & Assert
        Check.ThatCode(() => sut.MethodName(null!))
             .Throws<ArgumentNullException>();
    }
}
```

**Cas de tests OBLIGATOIRES** :
- ✅ **Cas nominal** : données valides, résultat attendu correct
- ✅ **Cas d'erreur** : données invalides, retour d'erreur approprié
- ✅ **Cas limites** : null, empty, valeurs min/max, collections vides
- ✅ **Cas métier spécifiques** : règles business particulières

**Convention de nommage des tests** (ADR-013) :
```
[MethodName]_[Condition]_[ExpectedBehavior]
```

**Vérification RED** :
```bash
dotnet test [Projet].Units.Tests.csproj --verbosity normal
# ❌ Résultat attendu : FAILED (tests échouent car code non implémenté)
```

**Checklist Étape 1** :
- [ ] Projet de tests créé et référencé dans solution
- [ ] Fichier de tests créé avec namespace correct
- [ ] Minimum 4 tests écrits (nominal, erreur, null, cas métier)
- [ ] Tests suivent convention AAA (Arrange-Act-Assert)
- [ ] Tests suivent naming convention (Method_Condition_Behavior)
- [ ] `dotnet test` échoue avec erreurs de compilation : ✅ RED confirmé

---

### Étape 2 : GREEN - Implémentation Minimale (1h30)

**Créer fichier source** : `sources/.../[Projet]/[NomFichier].cs`

```csharp
namespace Exakis.Janus.[Namespace];

/// <summary>
/// [Description XML claire de la classe]
/// </summary>
public sealed class [NomClasse]
{
    // Implémentation minimale pour passer les tests
    
    public Result<[Type]> MethodName([Type] input)
    {
        // Validation basique
        if (input is null)
        {
            return Result<[Type]>.Failure(
                Error.NullValue("INPUT-NULL-001", "Input cannot be null")
            );
        }
        
        // Logique métier minimale
        // ...
        
        return Result<[Type]>.Success(result);
    }
}
```

**Principes d'implémentation GREEN** :
- ✅ **Code minimal** : juste assez pour passer les tests
- ✅ **Pas d'optimisation prématurée** : clarté > performance à ce stade
- ✅ **Respect des ADRs** : patterns imposés (Result, ValueObject, etc.)
- ✅ **Commentaires XML** sur classes/méthodes publiques

**Vérification GREEN** :
```bash
# Build doit réussir
dotnet build [Projet].csproj --verbosity quiet
# ✅ Résultat attendu : BUILD SUCCEEDED

# Tests doivent passer
dotnet test [Projet].Units.Tests.csproj --verbosity normal
# ✅ Résultat attendu : PASSED (tous les tests verts)
```

**Checklist Étape 2** :
- [ ] Fichier source créé dans le bon namespace
- [ ] Classes/interfaces implémentées avec documentation XML
- [ ] Validation des entrées (null, empty, invalid)
- [ ] Logique métier implémentée (minimal mais fonctionnel)
- [ ] `dotnet build` réussit sans warnings : ✅ 0 errors, 0 warnings
- [ ] `dotnet test` passe à 100% : ✅ GREEN confirmé

---

### Étape 3 : REFACTOR - Amélioration et Nettoyage (30min)

**Actions de refactoring** :
1. **Extraire constantes magiques**
   ```csharp
   // ❌ AVANT
   if (value.Length > 50) { ... }
   
   // ✅ APRÈS
   private const int MaxLength = 50;
   if (value.Length > MaxLength) { ... }
   ```

2. **Appliquer SOLID**
   - **SRP** : Une classe = une responsabilité
   - **OCP** : Ouvert à l'extension, fermé à la modification
   - **LSP** : Substitution de Liskov respectée
   - **ISP** : Interfaces ségrégées
   - **DIP** : Dépendances vers abstractions

3. **Améliorer lisibilité**
   - Noms de variables explicites
   - Méthodes < 20 lignes
   - Complexité cyclomatique < 10

4. **Documenter**
   ```csharp
   /// <summary>
   /// [Description claire de la méthode]
   /// </summary>
   /// <param name="input">[Description du paramètre]</param>
   /// <returns>[Description du retour]</returns>
   /// <exception cref="ArgumentNullException">Thrown when input is null</exception>
   public Result<T> MethodName(T input) { ... }
   ```

**Vérification REFACTOR** :
```bash
# Tests doivent toujours passer
dotnet test [Projet].Units.Tests.csproj --verbosity normal
# ✅ Résultat attendu : PASSED (aucune régression)

# Build sans warnings
dotnet build [Projet].csproj --verbosity quiet
# ✅ Résultat attendu : 0 warnings
```

**Checklist Étape 3** :
- [ ] Constantes extraites (pas de magic numbers)
- [ ] SOLID appliqué (vérification manuelle)
- [ ] Noms explicites (classes, méthodes, variables)
- [ ] Documentation XML complète (classes publiques)
- [ ] `dotnet test` toujours à 100% : ✅ Pas de régression
- [ ] `dotnet build` : 0 warnings

---

### Étape 4 : Quality Gates (45min)

**4.1 Code Coverage (15min)**

```bash
# Installer coverlet si nécessaire
dotnet add [Projet].Units.Tests package coverlet.collector

# Générer rapport de couverture
dotnet test [Projet].Units.Tests.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults

# Vérifier couverture > 80%
```

**Objectif** : ✅ **Couverture ≥ 80%** (lignes + branches)

**Si < 80%** :
1. Identifier les lignes non couvertes
2. Ajouter tests manquants (cas limites, erreurs)
3. Relancer jusqu'à 80%+

**4.2 Mutation Testing (30min)**

```bash
cd sources/[couche]/src/[Projet].Units.Tests

# Lancer Stryker.NET
dotnet stryker \
  --reporter html \
  --reporter json \
  --reporter progress \
  --verbosity info

# Ouvrir rapport : StrykerOutput/[date]/reports/mutation-report.html
```

**Objectif** : ✅ **Mutation Score ≥ 85%** (idéal: 100%)

**Si < 85%** :
1. Analyser mutants survivants (rapport HTML)
2. Ajouter tests pour tuer mutants
3. Relancer jusqu'à 85%+

**Types de mutants à cibler** :
- Opérateurs logiques (`&&` ↔ `||`)
- Opérateurs arithmétiques (`+` ↔ `-`)
- Conditions (`==` ↔ `!=`, `<` ↔ `<=`)
- Retours de méthodes
- Initialisations de variables

**Checklist Étape 4** :
- [ ] Code coverage ≥ 80% (ligne + branche)
- [ ] Mutation score ≥ 85% (idéal 100%)
- [ ] Rapport Stryker généré et analysé
- [ ] Tests manquants ajoutés si nécessaire
- [ ] Build final sans erreurs ni warnings

---

### Étape 5 : Tests d'Architecture (15min)

**Vérifier respect Onion Architecture** :

```bash
# Si pas encore fait, créer projet Architecture.Tests
cd sources/backend/tests
dotnet new xunit -n Exakis.Janus.Architecture.Tests
dotnet add package NetArchTest.Rules
dotnet sln add Exakis.Janus.Architecture.Tests
```

**Test de dépendances** :

```csharp
[Fact]
public void Domain_Should_Not_Depend_On_Infrastructure()
{
    var result = Types.InAssembly(typeof([DomainType]).Assembly)
        .Should()
        .NotHaveDependencyOn("Exakis.Janus.Infrastructures")
        .GetResult();
    
    Check.That(result.IsSuccessful).IsTrue();
}
```

**Tests obligatoires selon couche** :
- **Domain** : Aucune dépendance vers Infrastructure, Application, Hosting
- **Application** : Peut dépendre de Domain, pas de Infrastructure
- **Infrastructure** : Peut dépendre de Domain.Abstractions
- **Hosting** : Peut dépendre de tous (orchestration DI uniquement)

**Checklist Étape 5** :
- [ ] Tests d'architecture créés pour la couche concernée
- [ ] Aucune dépendance cyclique détectée
- [ ] Respect strict Onion Architecture validé

---

### Étape 6 : Documentation (15min)

**6.1 Amender ADR si nécessaire**

Si la tâche introduit une **nouvelle décision architecturale** :

**Créer** : `documentations/technicals/adr/XXX-[nom-decision].adr.md`

```markdown
# ADR-XXX : [Titre de la Décision]

| **Metadata**        | **Value**                     |
|---------------------|-------------------------------|
| **Status**          | ✅ Accepted                   |
| **Date**            | YYYY-MM-DD                    |
| **Decision Makers** | Tech Lead, Team               |
| **Related ADRs**    | ADR-001, ADR-XXX              |
| **Impact**          | 🔴 Breaking / 🟡 Minor / 🟢 Doc |

## Context
[Problème à résoudre]

## Decision
[Décision prise et justification]

## Consequences
[Impacts positifs et négatifs]

## Alternatives Considered
[Options écartées et pourquoi]
```

**Ou mettre à jour ADR existant** si complément d'information.

**6.2 Mettre à jour User Story**

Si critère d'acceptation US validé :

**Modifier** : `documentations/functionnals/us-XXX-[nom].us.md`

```markdown
## Acceptance Criteria

- [x] **Critère 1** : Implémenté dans TASK-XXX ✅
- [ ] **Critère 2** : En cours (TASK-XXY)
- [ ] **Critère 3** : À faire
```

**6.3 Mettre à jour TASK actuelle**

**Modifier** : `tasks/in-progress/TASK-XXX-[nom].md`

- Remplir section "Actual Effort"
- Remplir "Success Metrics" avec valeurs réelles
- Déplacer vers `tasks/done/` une fois terminé

**Checklist Étape 6** :
- [ ] ADR créé/amendé si décision architecturale
- [ ] User Story mise à jour (critères d'acceptation)
- [ ] Fichier TASK mis à jour (effort réel, métriques)
- [ ] Pas de documentation en dehors de `documentations/`

---

### Étape 7 : Commit et Push (10min)

**Commit atomique** :

```bash
# Vérifier statut
git status

# Ajouter fichiers
git add sources/[...]/[fichiers-modifiés]
git add documentations/[...]
git add tasks/[...]

# Commit descriptif
git commit -m "feat(TASK-XXX): [Description courte]

- [Détail changement 1]
- [Détail changement 2]
- [Détail changement 3]

Related: US-XXX, ADR-XXX
Tests: Coverage 85%, Mutation 90%
"

# Push
git push origin features/tasks/TASK-XXX-[description]
```

**Convention Conventional Commits** :
- `feat(TASK-XXX):` - Nouvelle fonctionnalité
- `fix(TASK-XXX):` - Correction de bug
- `refactor(TASK-XXX):` - Refactoring sans changement fonctionnel
- `test(TASK-XXX):` - Ajout/modification de tests
- `docs(TASK-XXX):` - Documentation seule

**Checklist Étape 7** :
- [ ] `git status` propre (pas de fichiers non trackés importants)
- [ ] Commit message suit Conventional Commits
- [ ] Message référence TASK, US, ADR pertinents
- [ ] Métriques tests incluses dans message
- [ ] Push vers branche features/tasks/TASK-XXX

---

## ✅ Acceptance Criteria

### Critères Fonctionnels
[Liste des critères métier vérifiables liés à la User Story]
- [ ] [Critère fonctionnel 1 avec test manuel/automatique]
- [ ] [Critère fonctionnel 2 avec test manuel/automatique]

### Critères Techniques (Non-négociables)

**Build & Tests** :
- [ ] `dotnet build janus.sln --verbosity quiet` : ✅ SUCCESS, 0 warnings
- [ ] `dotnet test [Projet].Units.Tests.csproj` : ✅ 100% PASS
- [ ] Aucune régression sur tests existants (autres projets)

**Qualité Code** :
- [ ] Code coverage ≥ 80% (lignes + branches)
- [ ] Mutation score ≥ 85% (idéal 100%)
- [ ] Pas de code smell (SonarQube si activé)
- [ ] Pas de duplication de code (DRY respecté)

**Respect ADRs** :
- [ ] ADR-001 (Onion) : Dépendances respectées (tests architecture ✅)
- [ ] ADR-002 (CQRS) : Commands/Queries via MediatR si applicable
- [ ] ADR-008 (Result Pattern) : Gestion erreurs via Result<T>
- [ ] ADR-010 (Testing) : TDD appliqué, quality gates passés
- [ ] ADR-013 (Naming) : Conventions nommage respectées
- [ ] ADR-030 (Dependencies) : Librairies tierces via abstraction

**Documentation** :
- [ ] Documentation XML sur classes/méthodes publiques
- [ ] ADR créé/amendé si décision architecturale
- [ ] User Story mise à jour (critères d'acceptation)
- [ ] Pas de fichiers markdown hors `documentations/`

**Git** :
- [ ] Branche `features/tasks/TASK-XXX` créée
- [ ] Commit(s) atomique(s) avec Conventional Commits
- [ ] Message référence TASK, US, ADR
- [ ] Push vers origin réussi

---

## 📊 Success Metrics

| **Metric**              | **Target**     | **Actual**     |
|-------------------------|----------------|----------------|
| Code Coverage (Line)    | ≥ 80%          | [X%]           |
| Code Coverage (Branch)  | ≥ 80%          | [X%]           |
| Mutation Score          | ≥ 85%          | [X%]           |
| Build Warnings          | 0              | [X]            |
| Test Failures           | 0              | [X/Y]          |
| Estimated Effort        | X heures       | [Y heures]     |
| ADRs Respected          | 100%           | [X/Y]          |

---

## 🔗 Dependencies

### Dépendances Bloquantes (Pré-requis)
[Tâches qui DOIVENT être complétées avant de démarrer celle-ci]
- **TASK-XXX** : [Raison de la dépendance]
- **ADR-XXX** : [Décision qui doit être validée avant]

### Tâches Dépendantes (Suivi)
[Tâches qui NE PEUVENT PAS démarrer avant la complétion de celle-ci]
- **TASK-XXY** : [Raison de la dépendance]

### ADRs Applicables (Contraintes)
[Liste exhaustive des ADRs qui s'appliquent à cette tâche]
- **ADR-001** (Onion Architecture) : [Impact précis]
- **ADR-002** (CQRS/MediatR) : [Impact précis]
- **ADR-008** (Result Pattern) : [Impact précis]
- **ADR-010** (Testing Strategy) : [Impact précis]
- **ADR-013** (Naming Conventions) : [Impact précis]
- **ADR-030** (Dependencies Isolation) : [Impact précis]

### User Stories Implémentées (Traçabilité)
[Portion de User Story couverte par cette tâche]
- **US-XXX** : [Critères d'acceptation spécifiques validés]

---

## 📝 Notes et Risques

### Risques Identifiés
[Risques techniques, dépendances externes, complexité]
- ⚠️ **Risque 1** : [Description] - **Mitigation** : [Stratégie]
- ⚠️ **Risque 2** : [Description] - **Mitigation** : [Stratégie]

### Points d'Attention
[Conseils d'implémentation, pièges à éviter]
- 💡 **Attention** : [Point technique important]
- 💡 **Conseil** : [Recommandation d'implémentation]

### Décisions Techniques
[Décisions prises pendant implémentation]
- ✅ **Décision 1** : [Description et justification]
- ✅ **Décision 2** : [Description et justification]

---

## 🔄 Workflow Git

### Création Branche
```bash
git checkout develop
git pull origin develop
git checkout -b features/tasks/TASK-XXX-[description-courte]
git push -u origin features/tasks/TASK-XXX-[description-courte]
```

### Commits Réguliers
```bash
# Après chaque étape importante (RED, GREEN, REFACTOR)
git add .
git commit -m "[type](TASK-XXX): [description]"
git push origin features/tasks/TASK-XXX
```

### Pull Request (une fois terminé)
1. Vérifier que tous les critères d'acceptation sont ✅
2. Créer PR depuis `features/tasks/TASK-XXX` vers `develop`
3. Titre PR : `[TASK-XXX] [Titre court]`
4. Description PR : Copier sections Analysis + Success Metrics
5. Assigner reviewer(s)
6. Attendre review + merge

```

---

## Règles de Validation Avant Création

### Vérifications Obligatoires

**1. Unicité** :
```bash
# Vérifier qu'aucune tâche similaire existe
ls tasks/done/TASK-*[mot-clé]*.md
ls tasks/to-do/TASK-*[mot-clé]*.md
ls tasks/in-progress/TASK-*[mot-clé]*.md
```

**2. Atomicité** :
- [ ] La tâche traite UN SEUL sujet technique
- [ ] Durée estimée entre 2-8 heures
- [ ] Peut être testée indépendamment
- [ ] Aucune ambiguïté sur le périmètre

**3. Complétude** :
- [ ] Tous les champs metadata remplis
- [ ] Fichiers exacts spécifiés (chemins complets)
- [ ] ADRs applicables listés avec impact
- [ ] Critères d'acceptation testables définis
- [ ] Étapes d'implémentation détaillées (RED-GREEN-REFACTOR)

**4. Cohérence** :
- [ ] Dépendances vérifiées (TASK, ADR, US)
- [ ] Nomenclature respectée (TASK-XXX-kebab-case)
- [ ] Priorité cohérente avec roadmap
- [ ] Sprint assigné logiquement

### Anti-Patterns à Éviter

❌ **Tâches trop larges** :
- "Implémenter la gestion des ressources" → Trop vague, découper en 10+ tâches

❌ **Tâches sans tests** :
- Toute tâche DOIT inclure tests unitaires + mutation testing

❌ **Tâches sans ADR** :
- Toujours référencer les ADRs applicables (minimum ADR-001, ADR-010, ADR-013)

❌ **Tâches avec [TBD]** :
- Aucun champ ne doit rester "à définir"

❌ **Tâches sans périmètre exact** :
- Toujours spécifier fichiers/classes/méthodes précises

---

## Exemples de Tâches Atomiques Correctes

### Exemple 1 : Domain Layer

**TASK-050 : Créer Entité Resource avec ValueObjects**
- **Couche** : Domain
- **Fichiers** : `Resource.cs`, `ResourceEmail.cs`, `ResourceSkills.cs`, `ResourceAvailability.cs`
- **Tests** : 15+ tests (nominal, email invalide, skills vides, null availability)
- **ADRs** : ADR-001 (Onion), ADR-013 (Naming), ADR-008 (Result Pattern)
- **Durée** : 6h

### Exemple 2 : Infrastructure Layer

**TASK-052 : Implémenter ResourceRepository avec Dapper**
- **Couche** : Infrastructure.Data
- **Fichiers** : `ResourceRepository.cs`, `ResourceRepositoryTests.cs`
- **Tests** : 20+ tests (CRUD, transactions, exceptions, concurrency)
- **ADRs** : ADR-003 (Repository), ADR-004 (Dapper), ADR-011 (PostgreSQL)
- **Dépendances** : TASK-050 (entité), TASK-051 (interface)
- **Durée** : 8h

### Exemple 3 : Application Layer

**TASK-053 : Implémenter CreateResourceCommand + Handler**
- **Couche** : Application
- **Fichiers** : `CreateResourceCommand.cs`, `CreateResourceCommandHandler.cs`, `CreateResourceCommandValidator.cs`
- **Tests** : 12+ tests (validation OK, validation KO, repository errors)
- **ADRs** : ADR-002 (CQRS/MediatR), ADR-008 (Result Pattern)
- **Dépendances** : TASK-052 (repository)
- **Durée** : 5h

---

## Checklist Finale Avant Déplacement vers to-do/

Avant de considérer une tâche comme "prête à démarrer" :

- [ ] **Metadata** : Tous les champs remplis (pas de [TBD])
- [ ] **Analysis** : Contexte fonctionnel + problème technique clairs
- [ ] **Périmètre** : Fichiers exacts listés (chemins complets)
- [ ] **ADRs** : Minimum 3 ADRs référencés avec impact précis
- [ ] **Steps** : 7 étapes détaillées (Git, RED, GREEN, REFACTOR, Quality Gates, Doc, Commit)
- [ ] **Acceptance Criteria** : Critères fonctionnels + techniques testables
- [ ] **Dependencies** : Graphe de dépendances vérifié (pas de cycle)
- [ ] **Metrics** : Tableau Success Metrics avec targets
- [ ] **Risques** : Minimum 2 risques identifiés avec mitigations
- [ ] **Unicité** : Aucune duplication avec tasks existantes
- [ ] **Atomicité** : Durée 2-8h, UN sujet technique précis
- [ ] **Langue** : 100% en français

---

## Références Rapides

### Commandes Essentielles

```bash
# Build
dotnet build janus.sln --verbosity quiet

# Tests unitaires
dotnet test [Projet].Units.Tests.csproj --verbosity normal

# Coverage
dotnet test --collect:"XPlat Code Coverage"

# Mutation testing
cd [ProjetTests]; dotnet stryker --reporter html --reporter json --reporter progress

# Architecture tests
dotnet test Exakis.Janus.Architecture.Tests.csproj
```

### ADRs Critiques à Connaître

- **ADR-001** : Onion Architecture (dépendances entre couches)
- **ADR-002** : CQRS avec MediatR (Commands/Queries)
- **ADR-008** : Result Pattern (gestion erreurs)
- **ADR-010** : Testing Strategy (TDD, 80% coverage, 85% mutation)
- **ADR-013** : Naming Conventions (PascalCase, suffixes)
- **ADR-030** : Dependencies Isolation (wrapper librairies tierces)

### Librairies Autorisées

**Tests** : xUnit, NSubstitute, NFluent, Bogus, Respan, Stryker.NET, NetArchTest
**Domain** : Aucune librairie tierce (pure C#)
**Application** : MediatR, FluentValidation
**Infrastructure** : Dapper, Npgsql, StackExchange.Redis

---

**Objectif Final** : Chaque tâche doit être SI complète et détaillée qu'un développeur peut l'exécuter de A à Z sans jamais avoir besoin de clarification supplémentaire.