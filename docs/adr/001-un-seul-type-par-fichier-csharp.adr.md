# 1. Un seul type par fichier C#

Date: 2025-12-21

## Statut

Accepté

## Contexte

Dans un projet C# de taille significative, l'organisation du code source a un impact majeur sur :
- La **lisibilité** : Trouver rapidement une classe ou interface
- La **maintenabilité** : Modifier un type sans risquer d'impacter accidentellement d'autres types
- La **navigation** : Utiliser efficacement les outils de l'IDE (Go to Definition, Find All References)
- La **gestion de version** : Historique Git clair, diffs précis, réduction des conflits de merge
- La **revue de code** : Pull requests plus faciles à comprendre et valider

Pratiques problématiques observées dans certains projets :
- Fichiers contenant plusieurs classes (ex: `Models.cs` avec 10 classes)
- Fichiers mélangeant interfaces et implémentations
- Fichiers regroupant enums, records et classes par "thème"
- Classes imbriquées (nested classes) utilisées abusivement

Ces pratiques rendent le code difficile à naviguer, augmentent les conflits Git et compliquent la maintenance.

## Décision

**Chaque fichier C# DOIT contenir un seul type de premier niveau.**

Règles appliquées :
1. **Un fichier = un type** : Une classe, une interface, un enum, un record, une struct ou un delegate par fichier
2. **Nom du fichier = nom du type** : Le fichier `TenantService.cs` contient uniquement la classe `TenantService`
3. **Pas de types multiples** : Interdiction de regrouper plusieurs types dans un même fichier, même s'ils sont "liés"
4. **Classes imbriquées limitées** : Autorisées uniquement pour des types privés strictement internes (ex: `Builder` pattern, `Comparer` privé)

**Exemples conformes :**
```
Entities/
├── Tenant.cs           # Contient uniquement la classe Tenant
├── User.cs             # Contient uniquement la classe User
├── ApiKey.cs           # Contient uniquement la classe ApiKey

Interfaces/
├── ITenantRepository.cs    # Contient uniquement l'interface ITenantRepository
├── IUserRepository.cs      # Contient uniquement l'interface IUserRepository

Enums/
├── TenantStatus.cs     # Contient uniquement l'enum TenantStatus
├── UserRole.cs         # Contient uniquement l'enum UserRole
```

**Exemples NON conformes :**
```csharp
// ❌ INTERDIT : Plusieurs classes dans un fichier
// Fichier: Models.cs
public class Tenant { }
public class User { }
public class ApiKey { }

// ❌ INTERDIT : Interface et implémentation ensemble
// Fichier: TenantService.cs
public interface ITenantService { }
public class TenantService : ITenantService { }

// ❌ INTERDIT : Mélange de types différents
// Fichier: TenantTypes.cs
public enum TenantStatus { }
public record TenantDto { }
public class TenantValidator { }
```

## Conséquences

### Positives

- **Navigation instantanée** : Le nom du fichier correspond exactement au type recherché
- **Historique Git précis** : Chaque modification de type est isolée dans son fichier propre
- **Réduction des conflits** : Moins de risques de conflits lors des merges (fichiers plus petits et ciblés)
- **Revues de code efficaces** : Chaque fichier modifié a une responsabilité claire
- **Cohérence avec les conventions .NET** : Alignement avec les recommandations Microsoft et les pratiques de la communauté
- **Refactoring facilité** : Renommer, déplacer ou supprimer un type est une opération atomique
- **IDE optimisé** : Les outils comme "Go to Definition" ouvrent directement le bon fichier

### Négatives

- **Plus de fichiers** : Le projet contient davantage de fichiers, ce qui peut sembler verbeux
  - *Mitigation* : Organisation en sous-dossiers par domaine/fonctionnalité ; les IDE modernes gèrent très bien de nombreux fichiers
- **Navigation initiale** : Un développeur nouveau doit explorer plus de fichiers
  - *Mitigation* : Structure de dossiers logique et utilisation de la recherche par nom de type (Ctrl+T dans Visual Studio/VS Code)

### Neutres

- Le nombre de namespaces reste inchangé (la structure des namespaces est indépendante de cette règle)
- Aucun impact sur les performances de compilation (le compilateur traite les fichiers indépendamment)

## Alternatives considérées

### Option A : Regroupement par fonctionnalité

- **Description** : Autoriser plusieurs types liés dans un même fichier si appartenant à la même fonctionnalité
- **Avantages** : Moins de fichiers, types liés physiquement proches
- **Inconvénients** : Définition floue de "fonctionnalité liée", conflits Git fréquents, difficile à maintenir
- **Raison du rejet** : Introduit de la subjectivité et des incohérences ; les dossiers remplissent déjà ce rôle de regroupement

### Option B : Regroupement interfaces + implémentations

- **Description** : Placer une interface et son implémentation dans le même fichier
- **Avantages** : Visibilité immédiate du contrat et de l'implémentation
- **Inconvénients** : Viole le principe de séparation, complique le remplacement d'implémentation, fichiers volumineux
- **Raison du rejet** : L'interface et l'implémentation évoluent souvent indépendamment ; les séparer favorise le découplage

### Option C : Pas de règle stricte

- **Description** : Laisser chaque développeur décider de l'organisation
- **Avantages** : Flexibilité maximale
- **Inconvénients** : Incohérence, dette technique, confusion dans l'équipe
- **Raison du rejet** : L'absence de convention nuit à la maintenabilité à long terme

## Références

- [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET Runtime Coding Guidelines](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)
- StyleCop Rule SA1402: File may only contain a single type
