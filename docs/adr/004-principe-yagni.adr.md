# 4. Principe YAGNI - You Ain't Gonna Need It

Date: 2025-12-21

## Statut

Accepté

## Contexte

Les développeurs anticipent souvent des besoins futurs hypothétiques :
- "On pourrait avoir besoin de supporter plusieurs bases de données"
- "Un jour, on aura sûrement des clients internationaux"
- "Prévoyons une API GraphQL au cas où"

Cette anticipation mène à :
- **Code mort** : Fonctionnalités jamais utilisées (études montrent 45-65% du code)
- **Complexité gratuite** : Abstractions pour des cas qui n'arrivent jamais
- **Retards de livraison** : Temps passé sur du code spéculatif
- **Maintenance inutile** : Maintenir du code qui n'apporte pas de valeur

## Décision

**Appliquer le principe YAGNI : ne pas implémenter une fonctionnalité tant qu'elle n'est pas nécessaire.**

### 1. Règle fondamentale

```csharp
// ❌ YAGNI VIOLATION : Prévoir l'internationalisation "au cas où"
public class PriceFormatter
{
    private readonly ICultureProvider _cultureProvider;
    private readonly ICurrencyConverter _converter;
    private readonly ILocalizationService _localization;
    
    public string Format(decimal price)
    {
        var culture = _cultureProvider.GetCurrentCulture();
        var converted = _converter.Convert(price, culture.Currency);
        return _localization.FormatCurrency(converted, culture);
    }
}

// ✅ YAGNI : Implémenter ce qui est nécessaire maintenant
public class PriceFormatter
{
    public string Format(decimal price) => $"{price:C}";
}
```

### 2. Interfaces prématurées

```csharp
// ❌ YAGNI VIOLATION : Interface pour une seule implémentation
public interface IEmailSender { }
public interface IEmailSenderFactory { }
public interface IEmailTemplate { }
public interface IEmailTemplateRepository { }
public class SmtpEmailSender : IEmailSender { }

// ✅ YAGNI : Classe simple, interface quand nécessaire (test, 2ème impl)
public class EmailSender
{
    public void Send(string to, string subject, string body) { /* ... */ }
}
```

### 3. Configurations extensibles

```csharp
// ❌ YAGNI VIOLATION : Système de plugins pour 2 providers
public interface ILLMProviderPlugin { }
public class PluginLoader { }
public class PluginRegistry { }
public class PluginConfiguration { }

// ✅ YAGNI : Code direct pour les providers actuels
public class OpenAIProvider { /* ... */ }
public class AnthropicProvider { /* ... */ }
```

### 4. Critères de décision

**Implémenter SI :**
- Le besoin est dans le sprint actuel
- Un utilisateur/client a explicitement demandé la fonctionnalité
- C'est requis pour que le code actuel fonctionne

**NE PAS implémenter SI :**
- "On pourrait en avoir besoin"
- "Ce serait bien d'avoir"
- "C'est une bonne pratique générale"
- "Au cas où dans le futur"

### 5. YAGNI vs Bonne architecture

YAGNI ne signifie PAS :
- ❌ Écrire du code non testable
- ❌ Ignorer les principes SOLID
- ❌ Créer du couplage fort partout
- ❌ Négliger la qualité du code

YAGNI signifie :
- ✅ Ne pas ajouter de fonctionnalités spéculatives
- ✅ Garder les abstractions au minimum nécessaire
- ✅ Reporter les décisions jusqu'au dernier moment responsable

## Conséquences

### Positives

- **Livraison rapide** : Focus sur les fonctionnalités à valeur immédiate
- **Code minimal** : Moins de code à maintenir, tester, documenter
- **Flexibilité** : Décisions reportées = meilleures décisions (plus d'information)
- **Réduction du gaspillage** : Pas de temps sur du code inutilisé
- **Simplicité** : Système plus facile à comprendre

### Négatives

- **Refactoring ultérieur** : Certaines évolutions nécessiteront des modifications
  - *Mitigation* : Refactorer du code simple est peu coûteux
- **Résistance des développeurs** : Frustration de ne pas "bien faire" tout de suite
  - *Mitigation* : Former l'équipe sur la valeur du YAGNI

### Neutres

- YAGNI demande de la discipline pour résister à la tentation d'anticiper

## Alternatives considérées

### Option A : Architecture évolutive anticipée

- **Description** : Prévoir toutes les évolutions possibles dès le départ
- **Avantages** : Pas de refactoring si l'évolution se réalise
- **Inconvénients** : Sur-ingénierie massive, prédictions souvent fausses
- **Raison du rejet** : Le coût de la complexité dépasse le coût du refactoring

### Option B : Prototypage extensif

- **Description** : Créer des prototypes pour explorer les besoins futurs
- **Avantages** : Meilleure compréhension des possibilités
- **Inconvénients** : Temps investi sur des explorations potentiellement inutiles
- **Raison du rejet** : Les prototypes deviennent souvent du code de production

## Références

- [Extreme Programming Explained - Kent Beck](https://www.amazon.com/Extreme-Programming-Explained-Embrace-Change/dp/0321278658)
- [YAGNI - Martin Fowler](https://martinfowler.com/bliki/Yagni.html)
- [You Aren't Gonna Need It - c2 wiki](https://wiki.c2.com/?YouArentGonnaNeedIt)
