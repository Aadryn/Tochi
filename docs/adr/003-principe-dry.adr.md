# 3. Principe DRY - Don't Repeat Yourself

Date: 2025-12-21

## Statut

Accepté

## Contexte

La duplication de code est une source majeure de dette technique :
- **Bugs en cascade** : Corriger un bug à un endroit mais pas aux autres
- **Incohérences** : Comportements divergents entre copies
- **Maintenance multipliée** : Chaque modification doit être répétée N fois
- **Tests redondants** : Tester la même logique plusieurs fois

Types de duplication rencontrés :
1. **Duplication de code** : Copier-coller de blocs de code
2. **Duplication de logique** : Même algorithme implémenté différemment
3. **Duplication de données** : Même information stockée à plusieurs endroits
4. **Duplication de configuration** : Mêmes paramètres dans plusieurs fichiers

## Décision

**Appliquer le principe DRY : chaque connaissance doit avoir une représentation unique, non ambiguë et faisant autorité dans le système.**

### 1. Extraction de méthodes

```csharp
// ❌ DUPLICATION : Même logique répétée
public decimal CalculateOrderTotal(Order order)
{
    var subtotal = order.Items.Sum(i => i.Price * i.Quantity);
    var tax = subtotal * 0.20m;
    return subtotal + tax;
}

public decimal CalculateQuoteTotal(Quote quote)
{
    var subtotal = quote.Items.Sum(i => i.Price * i.Quantity);
    var tax = subtotal * 0.20m;
    return subtotal + tax;
}

// ✅ DRY : Logique centralisée
public decimal CalculateTotal(IEnumerable<ILineItem> items, decimal taxRate = 0.20m)
{
    var subtotal = items.Sum(i => i.Price * i.Quantity);
    return subtotal * (1 + taxRate);
}
```

### 2. Constantes et configuration

```csharp
// ❌ DUPLICATION : Magic numbers répétés
if (password.Length < 8) { /* ... */ }
// Ailleurs dans le code...
var minLength = 8;

// ✅ DRY : Constante unique
public static class PasswordPolicy
{
    public const int MinimumLength = 8;
    public const int MaximumLength = 128;
}
```

### 3. Classes de base et composition

```csharp
// ❌ DUPLICATION : Propriétés d'audit répétées
public class Order
{
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}

public class Invoice
{
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    // ... même chose
}

// ✅ DRY : Classe de base ou interface
public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}
```

### 4. Règle des 3 (Rule of Three)

- **1ère occurrence** : Écrire le code directement
- **2ème occurrence** : Tolérer la duplication (noter pour refactoring)
- **3ème occurrence** : **Extraire et centraliser obligatoirement**

### 5. Limites du DRY

**Ne pas appliquer DRY quand :**
- La "duplication" est accidentelle (même code, contextes différents)
- L'abstraction créée serait plus complexe que la duplication
- Les évolutions des copies sont susceptibles de diverger

```csharp
// ⚠️ FAUX DRY : Ces validations peuvent évoluer différemment
// Garder séparées même si similaires aujourd'hui
public class UserValidator { /* validation utilisateur */ }
public class AdminValidator { /* validation admin */ }
```

## Conséquences

### Positives

- **Source unique de vérité** : Une modification = un seul endroit
- **Cohérence garantie** : Comportement identique partout
- **Tests simplifiés** : Tester une fois, couvert partout
- **Maintenance réduite** : Moins de code à maintenir
- **Réduction des bugs** : Pas d'oubli de mise à jour

### Négatives

- **Couplage accru** : Les composants partagent des dépendances communes
  - *Mitigation* : Utiliser l'injection de dépendances et des interfaces
- **Abstractions prématurées** : Risque de créer des abstractions bancales
  - *Mitigation* : Appliquer la règle des 3 avant d'extraire
- **Complexité des généralisations** : Code générique parfois plus difficile
  - *Mitigation* : Préférer la composition à l'héritage

### Neutres

- Le temps de refactoring pour éliminer la duplication est un investissement

## Alternatives considérées

### Option A : Copier-coller assumé

- **Description** : Dupliquer le code pour éviter les dépendances
- **Avantages** : Indépendance totale entre composants
- **Inconvénients** : Maintenance multipliée, incohérences inévitables
- **Raison du rejet** : Le coût de maintenance dépasse largement le coût du couplage

### Option B : DRY absolu

- **Description** : Éliminer toute forme de duplication systématiquement
- **Avantages** : Zéro duplication
- **Inconvénients** : Abstractions forcées, couplage excessif, complexité
- **Raison du rejet** : Le DRY dogmatique crée plus de problèmes qu'il n'en résout

## Références

- [The Pragmatic Programmer - Andy Hunt & Dave Thomas](https://pragprog.com/titles/tpp20/the-pragmatic-programmer-20th-anniversary-edition/)
- [DRY Principle - Wikipedia](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself)
- [The Wrong Abstraction - Sandi Metz](https://sandimetz.com/blog/2016/1/20/the-wrong-abstraction)
