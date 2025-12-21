# 2. Principe KISS - Keep It Simple, Stupid

Date: 2025-12-21

## Statut

Accepté

## Contexte

La complexité logicielle est l'ennemi principal de la maintenabilité. Les projets qui échouent souffrent souvent de :
- **Sur-ingénierie** : Solutions sophistiquées pour des problèmes simples
- **Abstractions prématurées** : Couches d'indirection inutiles
- **Patterns mal appliqués** : Design patterns utilisés par dogme plutôt que par nécessité
- **Code "clever"** : Code astucieux mais incompréhensible

Ces pratiques augmentent :
- Le temps de compréhension pour les nouveaux développeurs
- Le risque de bugs (plus de code = plus de bugs potentiels)
- Le coût de maintenance
- La dette technique

## Décision

**Appliquer le principe KISS : toujours privilégier la solution la plus simple qui résout le problème.**

Règles d'application :

### 1. Simplicité du code

```csharp
// ✅ SIMPLE : Direct et lisible
public bool IsAdult(int age) => age >= 18;

// ❌ COMPLEXE : Sur-ingénierie inutile
public bool IsAdult(int age)
{
    var ageValidator = new AgeValidatorFactory()
        .CreateValidator(ValidationType.Adult)
        .WithStrategy(new AdultAgeStrategy());
    return ageValidator.Validate(age);
}
```

### 2. Choix des structures de données

```csharp
// ✅ SIMPLE : List suffit pour la plupart des cas
private readonly List<User> _users = new();

// ❌ COMPLEXE : ConcurrentDictionary quand pas de concurrence
private readonly ConcurrentDictionary<Guid, User> _users = new();
```

### 3. Architecture

- Commencer avec une architecture simple (monolithe modulaire)
- Ajouter de la complexité uniquement quand le besoin est avéré
- Éviter les microservices tant que la charge/équipe ne le justifie pas

### 4. Critères de décision

Avant d'ajouter de la complexité, répondre à ces questions :
1. **Est-ce nécessaire maintenant ?** (pas dans 6 mois hypothétiquement)
2. **Un développeur junior comprendrait-il ce code ?**
3. **Peut-on résoudre le problème plus simplement ?**
4. **La complexité ajoutée apporte-t-elle une valeur mesurable ?**

## Conséquences

### Positives

- **Onboarding rapide** : Nouveaux développeurs productifs plus vite
- **Moins de bugs** : Moins de code = moins de surface d'erreur
- **Maintenance facilitée** : Code compréhensible = modifications sûres
- **Revues de code efficaces** : Code simple à valider
- **Refactoring aisé** : Code simple plus facile à faire évoluer

### Négatives

- **Tentation de sous-optimiser** : Risque de confondre "simple" et "simpliste"
  - *Mitigation* : La simplicité doit résoudre le problème actuel correctement
- **Évolution future** : Code très simple peut nécessiter refactoring si les besoins changent
  - *Mitigation* : Acceptable car refactorer du code simple est facile

### Neutres

- La simplicité est subjective et dépend du contexte et de l'expérience de l'équipe

## Alternatives considérées

### Option A : Anticipation systématique

- **Description** : Toujours prévoir les évolutions futures dans le design initial
- **Avantages** : Moins de refactoring si l'évolution anticipée se réalise
- **Inconvénients** : Sur-ingénierie fréquente, complexité inutile dans 80% des cas
- **Raison du rejet** : Les prédictions sont souvent fausses ; YAGNI complète KISS

### Option B : Pas de règle

- **Description** : Laisser chaque développeur décider du niveau de complexité
- **Avantages** : Liberté totale
- **Inconvénients** : Incohérence, accumulation de complexité accidentelle
- **Raison du rejet** : La complexité non contrôlée tue les projets à long terme

## Références

- [KISS Principle - Wikipedia](https://en.wikipedia.org/wiki/KISS_principle)
- [Simple Made Easy - Rich Hickey](https://www.infoq.com/presentations/Simple-Made-Easy/)
- [The Art of Unix Programming - Eric S. Raymond](http://www.catb.org/esr/writings/taoup/html/)
