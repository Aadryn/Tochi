---
description: XML documentation standards - French language, didactic tone, novice-friendly
name: CSharp_Documentation_Standards
applyTo: "**/*.cs"
---

# Documentation C# - Standards XML

Guide pour documenter le code C# avec des commentaires XML précis, didactiques et compréhensibles.

## ⛔ À NE PAS FAIRE

- **N'écris jamais** de documentation en anglais (français OBLIGATOIRE)
- **Ne laisse jamais** de membre public sans commentaire XML
- **N'utilise jamais** de commentaires vides ou génériques ("Gets or sets the value")
- **Ne copie jamais** le nom de la méthode comme description
- **N'oublie jamais** les tags `<param>`, `<returns>`, `<exception>` quand applicables
- **N'écris jamais** de documentation COMMENT (implémentation) mais QUOI/POURQUOI
- **Ne documente jamais** l'évident - ajoute de la valeur

## ✅ À FAIRE

- **Écris toujours** la documentation en français
- **Documente toujours** tous les membres publics avec `<summary>`
- **Utilise toujours** un ton didactique accessible aux débutants
- **Ajoute toujours** `<remarks>` pour les détails d'implémentation complexes
- **Documente toujours** les exceptions avec `<exception cref="...">Quand/Pourquoi</exception>`
- **Inclus toujours** `<example>` pour les usages non triviaux
- **Utilise toujours** `<see cref="..."/>` pour référencer d'autres types

## 🎯 Actions Obligatoires (Mandatory)

**À TOUJOURS respecter lors de la documentation C# :**

1. ✅ **Langue française OBLIGATOIRE** : TOUTE documentation DOIT être en français
   ```csharp
   // ✅ Bon
   /// <summary>
   /// Calcule le total des éléments dans la collection.
   /// </summary>
   
   // ❌ Mauvais
   /// <summary>
   /// Calculates the total of items in the collection.
   /// </summary>
   ```

2. ✅ **Documentation COMPLÈTE** : Classes, méthodes, propriétés, événements DOIVENT avoir des commentaires XML
   ```csharp
   // ✅ Bon - Tous les membres publics documentés
   /// <summary>
   /// Représente un utilisateur du système.
   /// </summary>
   public class User
   {
       /// <summary>
       /// Obtient ou définit l'identifiant unique de l'utilisateur.
       /// </summary>
       public Guid Id { get; set; }
   }
   
   // ❌ Mauvais - Pas de documentation
   public class User
   {
       public Guid Id { get; set; }
   }
   ```

3. ✅ **Tags XML STANDARDS** : Utiliser `<summary>`, `<param>`, `<returns>`, `<exception>`, `<remarks>`, `<example>`
   ```csharp
   // ✅ Bon - Tags appropriés
   /// <summary>
   /// Divise deux nombres décimaux.
   /// </summary>
   /// <param name="numerateur">Le nombre à diviser.</param>
   /// <param name="denominateur">Le nombre par lequel diviser.</param>
   /// <returns>Le résultat de la division.</returns>
   /// <exception cref="DivideByZeroException">
   /// Levée lorsque <paramref name="denominateur"/> est égal à zéro.
   /// </exception>
   public decimal Diviser(decimal numerateur, decimal denominateur)
   
   // ❌ Mauvais - Pas de paramètres ni exceptions documentés
   /// <summary>
   /// Divise deux nombres.
   /// </summary>
   public decimal Diviser(decimal numerateur, decimal denominateur)
   ```

4. ✅ **Ton DIDACTIQUE** : Documentation compréhensible par développeurs novices
   ```csharp
   // ✅ Bon - Clair et pédagogique
   /// <summary>
   /// Valide que l'adresse e-mail respecte le format standard (ex: utilisateur@domaine.com).
   /// </summary>
   /// <remarks>
   /// Cette méthode vérifie la présence d'un '@' et d'un domaine valide.
   /// Elle ne vérifie pas si l'adresse existe réellement.
   /// </remarks>
   
   // ❌ Mauvais - Trop technique ou vague
   /// <summary>
   /// Valide l'e-mail via regex RFC 5322.
   /// </summary>
   ```

5. ✅ **AUCUNE référence aux processus internes** : JAMAIS mentionner outils, workflows, IDs de tâches
   ```csharp
   // ✅ Bon - Se concentre sur la fonctionnalité
   /// <summary>
   /// Calcule le prix total incluant les taxes applicables.
   /// </summary>
   
   // ❌ Mauvais - Mentionne processus interne
   /// <summary>
   /// Calcule le prix total (implémenté dans la tâche JIRA-1234).
   /// </summary>
   ```

6. ✅ **Forme IMPERSONNELLE** : JAMAIS utiliser "je", "nous", "notre"
   ```csharp
   // ✅ Bon - Forme impersonnelle
   /// <summary>
   /// Enregistre l'utilisateur dans la base de données.
   /// </summary>
   
   // ❌ Mauvais - Pronoms personnels
   /// <summary>
   /// Nous enregistrons l'utilisateur dans notre base de données.
   /// </summary>
   ```

7. ✅ **PRÉCISION factuelle** : Documenter UNIQUEMENT ce qui est explicite dans le code
   ```csharp
   // ✅ Bon - Décrit exactement ce que fait le code
   /// <summary>
   /// Ajoute l'élément à la fin de la liste si elle contient moins de 100 éléments.
   /// </summary>
   /// <returns>
   /// <c>true</c> si l'élément a été ajouté ; <c>false</c> si la liste est pleine.
   /// </returns>
   
   // ❌ Mauvais - Invente des comportements non présents
   /// <summary>
   /// Ajoute l'élément de manière optimisée avec cache LRU.
   /// </summary>
   ```

8. ✅ **Exemples d'UTILISATION** : Fournir `<example>` pour les APIs complexes
   ```csharp
   /// <summary>
   /// Filtre une collection selon un prédicat et retourne les résultats paginés.
   /// </summary>
   /// <example>
   /// <code>
   /// var utilisateurs = new List&lt;User&gt; { /* ... */ };
   /// var resultats = utilisateurs.Filtrer(u => u.Actif, page: 1, taille: 10);
   /// </code>
   /// </example>
   ```

## Structure de Documentation par Type

### Classes et Interfaces

**TOUJOURS documenter :**
- `<summary>` : Rôle et responsabilité de la classe/interface
- `<remarks>` : Cas d'usage, contraintes, précisions importantes
- `<example>` : Exemple d'instanciation et utilisation (si pertinent)

```csharp
/// <summary>
/// Représente un service de gestion des notifications par e-mail.
/// </summary>
/// <remarks>
/// Ce service utilise un système de file d'attente pour envoyer les e-mails de manière asynchrone.
/// Les e-mails échoués sont automatiquement réessayés jusqu'à 3 fois.
/// </remarks>
/// <example>
/// <code>
/// var service = new EmailNotificationService(configuration);
/// await service.EnvoyerAsync("dest@exemple.com", "Sujet", "Corps du message");
/// </code>
/// </example>
public class EmailNotificationService : INotificationService
{
    // ...
}
```

### Méthodes et Fonctions

**TOUJOURS documenter :**
- `<summary>` : Action effectuée (verbe d'action à l'infinitif ou 3ᵉ personne)
- `<param>` : Description de CHAQUE paramètre
- `<returns>` : Description précise de la valeur retournée
- `<exception>` : TOUTES les exceptions pouvant être levées

```csharp
/// <summary>
/// Recherche un utilisateur par son adresse e-mail dans la base de données.
/// </summary>
/// <param name="email">L'adresse e-mail à rechercher (insensible à la casse).</param>
/// <param name="cancellationToken">
/// Jeton d'annulation pour interrompre l'opération si nécessaire.
/// </param>
/// <returns>
/// L'utilisateur correspondant à l'adresse e-mail, ou <c>null</c> si aucun utilisateur n'est trouvé.
/// </returns>
/// <exception cref="ArgumentNullException">
/// Levée si <paramref name="email"/> est <c>null</c> ou vide.
/// </exception>
/// <exception cref="DatabaseException">
/// Levée en cas d'erreur de connexion ou de requête à la base de données.
/// </exception>
public async Task<User?> RechercherParEmailAsync(
    string email, 
    CancellationToken cancellationToken = default)
{
    // ...
}
```

### Propriétés

**TOUJOURS documenter :**
- `<summary>` : Ce que représente la propriété (pas "obtient ou définit")
- `<value>` : Type de valeur et contraintes éventuelles
- `<remarks>` : Comportements spéciaux (lecture seule, calcul, validation)

```csharp
/// <summary>
/// Identifiant unique de l'entité, généré automatiquement à la création.
/// </summary>
/// <value>
/// Un GUID unique attribué lors de l'instanciation de l'objet.
/// </value>
/// <remarks>
/// Cette propriété est en lecture seule après l'initialisation.
/// </remarks>
public Guid Id { get; init; }

/// <summary>
/// Âge de l'utilisateur calculé à partir de sa date de naissance.
/// </summary>
/// <value>
/// L'âge en années complètes, ou <c>null</c> si la date de naissance n'est pas définie.
/// </value>
/// <remarks>
/// Cette propriété est recalculée à chaque accès en fonction de la date actuelle.
/// </remarks>
public int? Age => DateNaissance.HasValue 
    ? (DateTime.Now.Year - DateNaissance.Value.Year) 
    : null;
```

### Événements

**TOUJOURS documenter :**
- `<summary>` : Quand l'événement est déclenché
- `<remarks>` : Informations sur les gestionnaires et le contexte

```csharp
/// <summary>
/// Déclenché lorsque la valeur de la propriété change.
/// </summary>
/// <remarks>
/// Cet événement est levé APRÈS la modification de la valeur.
/// Les gestionnaires reçoivent l'ancienne et la nouvelle valeur.
/// </remarks>
public event EventHandler<ValueChangedEventArgs>? ValueChanged;
```

### Enums

**TOUJOURS documenter :**
- `<summary>` sur l'enum : Ce que représente l'énumération
- `<summary>` sur CHAQUE membre : Signification précise de la valeur

```csharp
/// <summary>
/// Définit les différents niveaux de priorité pour les tâches.
/// </summary>
public enum TaskPriority
{
    /// <summary>
    /// Priorité basse : la tâche peut être traitée ultérieurement.
    /// </summary>
    Low = 0,
    
    /// <summary>
    /// Priorité normale : la tâche doit être traitée dans les délais standard.
    /// </summary>
    Normal = 1,
    
    /// <summary>
    /// Priorité haute : la tâche nécessite un traitement rapide.
    /// </summary>
    High = 2,
    
    /// <summary>
    /// Priorité critique : la tâche doit être traitée immédiatement.
    /// </summary>
    Critical = 3
}
```

## Tags XML Standards

### Tags Obligatoires

| Tag | Contexte | Usage |
|-----|----------|-------|
| `<summary>` | TOUS les membres publics | Description brève et claire (1-3 phrases) |
| `<param>` | Méthodes avec paramètres | Description de CHAQUE paramètre |
| `<returns>` | Méthodes non-void | Description précise de la valeur retournée |
| `<exception>` | Méthodes levant des exceptions | Documentation de TOUTES les exceptions possibles |

### Tags Recommandés

| Tag | Contexte | Usage |
|-----|----------|-------|
| `<remarks>` | Classes, méthodes complexes | Détails supplémentaires, cas d'usage, contraintes |
| `<example>` | APIs complexes ou publiques | Code d'exemple d'utilisation |
| `<value>` | Propriétés | Description du type de valeur et contraintes |
| `<see cref="">` | Références à d'autres types | Lien vers classe, méthode ou propriété liée |
| `<seealso cref="">` | Références complémentaires | Lien vers documentation connexe |
| `<paramref name="">` | Dans les descriptions | Référence à un paramètre dans le texte |
| `<c>` | Code inline | Mot-clé ou valeur dans le texte (ex: `<c>null</c>`) |
| `<code>` | Bloc de code | Exemple de code multi-lignes |

### Exemples de Tags Avancés

```csharp
/// <summary>
/// Convertit une chaîne JSON en objet typé.
/// </summary>
/// <typeparam name="T">
/// Le type de l'objet à désérialiser. Doit avoir un constructeur sans paramètre.
/// </typeparam>
/// <param name="json">La chaîne JSON à convertir.</param>
/// <returns>
/// Une instance de type <typeparamref name="T"/> contenant les données désérialisées.
/// </returns>
/// <exception cref="JsonException">
/// Levée si <paramref name="json"/> n'est pas un JSON valide.
/// </exception>
/// <remarks>
/// Cette méthode utilise <see cref="System.Text.Json.JsonSerializer"/> pour la désérialisation.
/// Pour des besoins avancés, voir <seealso cref="JsonSerializerOptions"/>.
/// </remarks>
/// <example>
/// <code>
/// var user = DeserializeJson&lt;User&gt;("{\"Name\":\"Alice\",\"Age\":30}");
/// Console.WriteLine(user.Name); // Affiche : Alice
/// </code>
/// </example>
public T DeserializeJson<T>(string json) where T : new()
{
    // ...
}
```

## Bonnes Pratiques

### Descriptions Claires et Concises

✅ **À FAIRE :**
- Commencer par un verbe d'action (Calcule, Valide, Enregistre, Recherche)
- Phrases complètes avec sujet-verbe-complément
- Préciser les unités (secondes, mètres, euros)
- Indiquer les valeurs possibles (`null`, `true`, `false`)

❌ **À ÉVITER :**
- Descriptions vagues ("Fait quelque chose", "Gère les données")
- Répéter le nom de la méthode ("GetUser obtient un utilisateur")
- Termes ambigus ("peut-être", "normalement", "généralement")
- Jargon technique non expliqué

```csharp
// ✅ Bon - Précis et actionnable
/// <summary>
/// Calcule le prix total en euros, incluant la TVA à 20%.
/// </summary>
/// <param name="prixHT">Le prix hors taxes en euros.</param>
/// <returns>Le prix TTC arrondi à 2 décimales.</returns>

// ❌ Mauvais - Vague et imprécis
/// <summary>
/// Calcule le prix.
/// </summary>
/// <param name="prix">Le prix.</param>
/// <returns>Le total.</returns>
```

### Documentation des Valeurs Spéciales

**TOUJOURS préciser :**
- Valeurs `null` acceptées ou retournées
- Collections vides vs `null`
- Valeurs par défaut
- Valeurs limites (min/max)

```csharp
/// <summary>
/// Recherche le premier élément correspondant au prédicat.
/// </summary>
/// <param name="predicate">
/// La condition de recherche. Ne doit pas être <c>null</c>.
/// </param>
/// <returns>
/// L'élément trouvé, ou <c>null</c> si aucun élément ne correspond.
/// </returns>
/// <remarks>
/// Si <paramref name="predicate"/> est <c>null</c>, une exception 
/// <see cref="ArgumentNullException"/> est levée.
/// Une collection vide retourne toujours <c>null</c>.
/// </remarks>
```

### Documentation des Comportements Asynchrones

```csharp
/// <summary>
/// Enregistre l'utilisateur dans la base de données de manière asynchrone.
/// </summary>
/// <param name="user">L'utilisateur à enregistrer.</param>
/// <param name="cancellationToken">
/// Jeton permettant d'annuler l'opération en cours.
/// </param>
/// <returns>
/// Une tâche représentant l'opération asynchrone.
/// Le résultat contient l'identifiant de l'utilisateur créé.
/// </returns>
/// <remarks>
/// Cette méthode ne bloque pas le thread appelant.
/// En cas d'annulation via <paramref name="cancellationToken"/>,
/// une <see cref="OperationCanceledException"/> est levée.
/// </remarks>
public async Task<Guid> EnregistrerAsync(
    User user, 
    CancellationToken cancellationToken = default)
```

### Documentation des Interfaces

```csharp
/// <summary>
/// Définit un contrat pour les services de notification.
/// </summary>
/// <remarks>
/// Les implémentations de cette interface DOIVENT garantir :
/// <list type="bullet">
/// <item>L'envoi asynchrone des notifications</item>
/// <item>La gestion des erreurs d'envoi</item>
/// <item>La traçabilité des notifications envoyées</item>
/// </list>
/// </remarks>
public interface INotificationService
{
    /// <summary>
    /// Envoie une notification à un destinataire.
    /// </summary>
    /// <param name="destinataire">L'adresse du destinataire.</param>
    /// <param name="message">Le contenu de la notification.</param>
    /// <returns>
    /// Une tâche représentant l'opération. Le résultat indique si l'envoi a réussi.
    /// </returns>
    Task<bool> EnvoyerAsync(string destinataire, string message);
}
```

## Anti-Patterns à Éviter

### 1. Documentation Redondante avec le Code

```csharp
// ❌ MAUVAIS - Répète le code sans apporter de valeur
/// <summary>
/// Obtient ou définit le nom.
/// </summary>
public string Name { get; set; }

// ✅ BON - Apporte une information utile
/// <summary>
/// Nom complet de l'utilisateur (prénom et nom de famille).
/// </summary>
/// <remarks>
/// Limité à 100 caractères. Les espaces multiples sont automatiquement réduits.
/// </remarks>
public string Name { get; set; }
```

### 2. Documentation Obsolète

```csharp
// ❌ MAUVAIS - La documentation ne correspond plus au code
/// <summary>
/// Retourne une liste d'utilisateurs actifs.
/// </summary>
/// <returns>Une liste d'utilisateurs.</returns>
public IEnumerable<User> GetActiveUsers(bool includeInactive)
//                                       ^^^ Nouveau paramètre non documenté!

// ✅ BON - Documentation à jour
/// <summary>
/// Retourne une collection d'utilisateurs filtrés selon leur statut.
/// </summary>
/// <param name="includeInactive">
/// Si <c>true</c>, inclut également les utilisateurs inactifs ; 
/// sinon, retourne uniquement les utilisateurs actifs.
/// </param>
/// <returns>
/// Une collection énumérable d'utilisateurs correspondant au filtre.
/// </returns>
public IEnumerable<User> GetActiveUsers(bool includeInactive)
```

### 3. Documentation Trop Technique

```csharp
// ❌ MAUVAIS - Détails d'implémentation excessifs
/// <summary>
/// Utilise l'algorithme de tri QuickSort avec pivot médian pour trier
/// la collection en O(n log n) via une implémentation récursive tail-call optimisée.
/// </summary>

// ✅ BON - Se concentre sur l'usage
/// <summary>
/// Trie la collection par ordre croissant.
/// </summary>
/// <remarks>
/// Cette méthode modifie la collection d'origine.
/// Pour les grandes collections (> 10 000 éléments), privilégier la méthode asynchrone.
/// </remarks>
```

### 4. Documentation Vague

```csharp
// ❌ MAUVAIS - Trop vague, pas actionnable
/// <summary>
/// Gère les données.
/// </summary>
/// <returns>Résultat de l'opération.</returns>

// ✅ BON - Précis et descriptif
/// <summary>
/// Valide et enregistre les modifications apportées à l'entité dans la base de données.
/// </summary>
/// <returns>
/// <c>true</c> si l'enregistrement a réussi ; 
/// <c>false</c> si la validation a échoué.
/// </returns>
```

## Checklist de Validation

Avant de finaliser la documentation :

### Complétude
- [ ] Tous les membres publics ont un `<summary>`
- [ ] Tous les paramètres ont un `<param>`
- [ ] Toutes les méthodes non-void ont un `<returns>`
- [ ] Toutes les exceptions levées ont un `<exception>`
- [ ] Les APIs complexes ont un `<example>`

### Qualité
- [ ] Documentation en français correct (grammaire, orthographe)
- [ ] Ton didactique et compréhensible par novices
- [ ] Aucun pronom personnel ("je", "nous", "notre")
- [ ] Aucune référence à des outils/processus/IDs internes
- [ ] Descriptions précises et factuelles (pas d'invention)

### Contenu
- [ ] Valeurs `null` documentées (paramètres et retours)
- [ ] Exceptions documentées avec conditions de déclenchement
- [ ] Comportements asynchrones expliqués
- [ ] Unités et formats spécifiés (dates, montants, durées)
- [ ] Contraintes et validations mentionnées

### Format
- [ ] Tags XML valides et bien formés
- [ ] Références `<see cref="">` correctes
- [ ] Code d'exemple compilable dans `<example>`
- [ ] Indentation cohérente des commentaires XML

## Cas Spéciaux et Patterns Avancés

### Records et Types Immutables

```csharp
/// <summary>
/// Représente un point géographique immuable avec coordonnées GPS.
/// </summary>
/// <param name="Latitude">
/// Latitude en degrés décimaux (valeur entre -90 et +90).
/// </param>
/// <param name="Longitude">
/// Longitude en degrés décimaux (valeur entre -180 et +180).
/// </param>
/// <remarks>
/// Ce record est immutable : les valeurs ne peuvent pas être modifiées après création.
/// Utilisez l'expression <c>with</c> pour créer une copie modifiée.
/// </remarks>
/// <example>
/// <code>
/// var paris = new GeoPoint(48.8566, 2.3522);
/// var versailles = paris with { Latitude = 48.8049 };
/// </code>
/// </example>
public record GeoPoint(double Latitude, double Longitude);
```

### Extension Methods

```csharp
/// <summary>
/// Fournit des méthodes d'extension pour la manipulation de chaînes de caractères.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Tronque la chaîne à la longueur spécifiée en ajoutant des points de suspension si nécessaire.
    /// </summary>
    /// <param name="value">La chaîne à tronquer.</param>
    /// <param name="maxLength">
    /// Longueur maximale de la chaîne résultante, points de suspension inclus.
    /// Doit être supérieur ou égal à 3.
    /// </param>
    /// <returns>
    /// La chaîne tronquée avec "..." si elle dépasse <paramref name="maxLength"/>,
    /// sinon la chaîne originale.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Levée si <paramref name="value"/> est <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Levée si <paramref name="maxLength"/> est inférieur à 3.
    /// </exception>
    /// <example>
    /// <code>
    /// var texte = "Ceci est un texte très long";
    /// var tronque = texte.Truncate(15); // "Ceci est un..."
    /// </code>
    /// </example>
    public static string Truncate(this string value, int maxLength)
    {
        // ...
    }
}
```

### Generic Constraints

```csharp
/// <summary>
/// Référentiel générique pour accéder aux entités d'un type spécifique.
/// </summary>
/// <typeparam name="TEntity">
/// Le type d'entité géré par ce référentiel.
/// Doit implémenter <see cref="IEntity"/> et avoir un constructeur sans paramètre.
/// </typeparam>
/// <typeparam name="TKey">
/// Le type de l'identifiant de l'entité.
/// Doit être un type valeur comparable.
/// </typeparam>
/// <remarks>
/// Cette classe fournit les opérations CRUD de base pour toute entité du domaine.
/// Les contraintes génériques garantissent la cohérence des types manipulés.
/// </remarks>
public class Repository<TEntity, TKey> 
    where TEntity : IEntity<TKey>, new()
    where TKey : struct, IComparable<TKey>
{
    /// <summary>
    /// Récupère une entité par son identifiant.
    /// </summary>
    /// <param name="id">L'identifiant unique de l'entité.</param>
    /// <returns>
    /// L'entité correspondante, ou <c>null</c> si aucune entité avec cet identifiant n'existe.
    /// </returns>
    public TEntity? GetById(TKey id)
    {
        // ...
    }
}
```

### Nullable Reference Types

```csharp
/// <summary>
/// Service de validation d'adresses e-mail avec support nullable.
/// </summary>
public class EmailValidator
{
    /// <summary>
    /// Valide une adresse e-mail et retourne une version normalisée.
    /// </summary>
    /// <param name="email">
    /// L'adresse e-mail à valider. Peut être <c>null</c> ou vide.
    /// </param>
    /// <param name="normalized">
    /// Sortie : L'adresse e-mail normalisée (minuscules, espaces supprimés) si valide,
    /// sinon <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> si l'adresse est valide ; <c>false</c> sinon.
    /// </returns>
    /// <remarks>
    /// Une adresse <c>null</c> ou vide est considérée comme invalide.
    /// La validation vérifie le format selon la RFC 5322 (simplifié).
    /// </remarks>
    /// <example>
    /// <code>
    /// if (validator.TryValidate("User@Example.COM", out var normalized))
    /// {
    ///     Console.WriteLine(normalized); // Affiche : user@example.com
    /// }
    /// </code>
    /// </example>
    public bool TryValidate(string? email, [NotNullWhen(true)] out string? normalized)
    {
        // ...
    }
}
```

### Operators Overloading

```csharp
/// <summary>
/// Représente une durée en heures et minutes.
/// </summary>
public readonly struct Duration
{
    /// <summary>
    /// Additionne deux durées.
    /// </summary>
    /// <param name="left">La première durée.</param>
    /// <param name="right">La seconde durée.</param>
    /// <returns>
    /// Une nouvelle durée représentant la somme des deux durées.
    /// </returns>
    /// <remarks>
    /// Les minutes sont automatiquement converties en heures si elles dépassent 59.
    /// </remarks>
    /// <example>
    /// <code>
    /// var duration1 = new Duration(2, 30); // 2h30
    /// var duration2 = new Duration(1, 45); // 1h45
    /// var total = duration1 + duration2;   // 4h15
    /// </code>
    /// </example>
    public static Duration operator +(Duration left, Duration right)
    {
        // ...
    }
}
```

### Pattern Matching et Switch Expressions

```csharp
/// <summary>
/// Calcule le coût d'expédition selon le type de produit.
/// </summary>
/// <param name="product">Le produit à expédier.</param>
/// <returns>
/// Le coût d'expédition en euros.
/// </returns>
/// <remarks>
/// Le calcul utilise les règles suivantes :
/// <list type="bullet">
/// <item>Produit physique (> 5kg) : 15€</item>
/// <item>Produit physique (≤ 5kg) : 5€</item>
/// <item>Produit numérique : 0€</item>
/// <item>Produit sur mesure : 25€</item>
/// </list>
/// </remarks>
/// <exception cref="ArgumentNullException">
/// Levée si <paramref name="product"/> est <c>null</c>.
/// </exception>
public decimal CalculerFraisExpedition(Product product)
{
    ArgumentNullException.ThrowIfNull(product);
    
    return product switch
    {
        PhysicalProduct { Weight: > 5 } => 15m,
        PhysicalProduct => 5m,
        DigitalProduct => 0m,
        CustomProduct => 25m,
        _ => throw new NotSupportedException($"Type de produit non supporté : {product.GetType().Name}")
    };
}
```

## Scénarios Métier Complexes

### Repository Pattern

```csharp
/// <summary>
/// Référentiel pour la gestion des utilisateurs dans la base de données.
/// </summary>
/// <remarks>
/// Cette implémentation utilise Entity Framework Core pour l'accès aux données.
/// Toutes les opérations sont tracées via <see cref="ILogger{TCategoryName}"/>.
/// </remarks>
public class UserRepository : IUserRepository
{
    /// <summary>
    /// Recherche des utilisateurs selon plusieurs critères de filtrage.
    /// </summary>
    /// <param name="criteria">
    /// Les critères de recherche. Tous les champs <c>null</c> sont ignorés.
    /// </param>
    /// <param name="pagination">
    /// Paramètres de pagination (page, taille). Si <c>null</c>, retourne tous les résultats.
    /// </param>
    /// <param name="cancellationToken">
    /// Jeton d'annulation pour interrompre l'opération.
    /// </param>
    /// <returns>
    /// Une tâche contenant les résultats paginés :
    /// <list type="bullet">
    /// <item><c>Items</c> : Les utilisateurs correspondant aux critères</item>
    /// <item><c>TotalCount</c> : Nombre total de résultats (avant pagination)</item>
    /// <item><c>PageNumber</c> : Numéro de la page actuelle (base 1)</item>
    /// <item><c>PageSize</c> : Taille de la page</item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Levée si <paramref name="criteria"/> est <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Levée si l'opération est annulée via <paramref name="cancellationToken"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var criteria = new UserSearchCriteria 
    /// { 
    ///     IsActive = true, 
    ///     RoleId = 5 
    /// };
    /// var pagination = new PaginationParams(page: 1, size: 20);
    /// var results = await repository.SearchAsync(criteria, pagination);
    /// 
    /// Console.WriteLine($"Trouvé {results.TotalCount} utilisateurs");
    /// foreach (var user in results.Items)
    /// {
    ///     Console.WriteLine(user.Email);
    /// }
    /// </code>
    /// </example>
    public async Task<PagedResult<User>> SearchAsync(
        UserSearchCriteria criteria,
        PaginationParams? pagination = null,
        CancellationToken cancellationToken = default)
    {
        // ...
    }
}
```

### Command/Query Handlers (CQRS)

```csharp
/// <summary>
/// Commande pour créer un nouvel utilisateur dans le système.
/// </summary>
/// <remarks>
/// Cette commande déclenche les actions suivantes :
/// <list type="number">
/// <item>Validation des données (e-mail unique, mot de passe conforme)</item>
/// <item>Hachage sécurisé du mot de passe</item>
/// <item>Création de l'enregistrement en base</item>
/// <item>Envoi d'un e-mail de bienvenue</item>
/// <item>Publication d'un événement <see cref="UserCreatedEvent"/></item>
/// </list>
/// </remarks>
public record CreateUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName
) : ICommand<UserCreatedResult>;

/// <summary>
/// Gestionnaire de la commande de création d'utilisateur.
/// </summary>
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserCreatedResult>
{
    /// <summary>
    /// Traite la commande de création d'un utilisateur.
    /// </summary>
    /// <param name="command">La commande contenant les données de l'utilisateur à créer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>
    /// Une tâche contenant le résultat de la création :
    /// <list type="bullet">
    /// <item><c>UserId</c> : L'identifiant unique de l'utilisateur créé</item>
    /// <item><c>Success</c> : Indique si la création a réussi</item>
    /// <item><c>Errors</c> : Liste des erreurs de validation si applicable</item>
    /// </list>
    /// </returns>
    /// <exception cref="ValidationException">
    /// Levée si les données de la commande ne respectent pas les règles métier.
    /// Contient la liste détaillée des erreurs de validation.
    /// </exception>
    /// <exception cref="DuplicateEmailException">
    /// Levée si un utilisateur avec cet e-mail existe déjà.
    /// </exception>
    public async Task<UserCreatedResult> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        // ...
    }
}
```

### Domain Events

```csharp
/// <summary>
/// Événement déclenché lorsqu'une commande est confirmée par le client.
/// </summary>
/// <remarks>
/// Cet événement marque la transition de l'état "En attente" vers "Confirmée".
/// Les gestionnaires de cet événement déclenchent généralement :
/// <list type="bullet">
/// <item>Notification au vendeur</item>
/// <item>Déclenchement du processus de préparation</item>
/// <item>Mise à jour du stock</item>
/// <item>Création de la facture</item>
/// </list>
/// </remarks>
public sealed class OrderConfirmedEvent : IDomainEvent
{
    /// <summary>
    /// Identifiant unique de la commande confirmée.
    /// </summary>
    public Guid OrderId { get; init; }
    
    /// <summary>
    /// Date et heure UTC de la confirmation.
    /// </summary>
    public DateTime ConfirmedAtUtc { get; init; }
    
    /// <summary>
    /// Montant total de la commande en euros.
    /// </summary>
    public decimal TotalAmount { get; init; }
    
    /// <summary>
    /// Crée un nouvel événement de confirmation de commande.
    /// </summary>
    /// <param name="orderId">L'identifiant de la commande.</param>
    /// <param name="totalAmount">Le montant total en euros.</param>
    public OrderConfirmedEvent(Guid orderId, decimal totalAmount)
    {
        OrderId = orderId;
        TotalAmount = totalAmount;
        ConfirmedAtUtc = DateTime.UtcNow;
    }
}
```

## Performance et Optimisation

### Documentation des Considérations de Performance

```csharp
/// <summary>
/// Charge l'ensemble des produits avec leurs catégories et images associées.
/// </summary>
/// <returns>
/// Une collection de produits avec leurs relations chargées en mémoire.
/// </returns>
/// <remarks>
/// ⚠️ ATTENTION PERFORMANCE :
/// <list type="bullet">
/// <item>Cette méthode charge TOUS les produits en mémoire (eager loading)</item>
/// <item>Utilise 3 requêtes SQL via Include() pour éviter le problème N+1</item>
/// <item>Temps d'exécution typique : ~200ms pour 1000 produits</item>
/// <item>Mémoire consommée : ~50MB pour 1000 produits</item>
/// </list>
/// 
/// Pour de grandes quantités de données (> 5000 produits), privilégier :
/// <list type="number">
/// <item><see cref="GetProductsStreamAsync"/> pour traitement par flux</item>
/// <item><see cref="GetProductsPagedAsync"/> pour pagination</item>
/// </list>
/// </remarks>
public async Task<IReadOnlyList<Product>> GetAllProductsWithDetailsAsync()
{
    return await _context.Products
        .Include(p => p.Category)
        .Include(p => p.Images)
        .Include(p => p.Reviews)
        .ToListAsync();
}

/// <summary>
/// Traite les produits par flux pour minimiser l'utilisation mémoire.
/// </summary>
/// <param name="processor">
/// Fonction de traitement appelée pour chaque produit.
/// </param>
/// <param name="batchSize">
/// Nombre de produits traités par lot (défaut : 100).
/// </param>
/// <returns>
/// Une tâche représentant l'opération asynchrone.
/// </returns>
/// <remarks>
/// ✅ OPTIMISÉ POUR GRANDES QUANTITÉS :
/// <list type="bullet">
/// <item>Traite les produits par lots de <paramref name="batchSize"/> éléments</item>
/// <item>Libère la mémoire entre chaque lot</item>
/// <item>Convient pour plus de 10 000 produits</item>
/// <item>Mémoire maximale : ~5MB quelle que soit la quantité totale</item>
/// </list>
/// </remarks>
public async Task ProcessProductsStreamAsync(
    Func<Product, Task> processor,
    int batchSize = 100)
{
    // ...
}
```

### Caching et Mémoization

```csharp
/// <summary>
/// Récupère les paramètres de configuration avec mise en cache.
/// </summary>
/// <param name="key">La clé du paramètre.</param>
/// <returns>
/// La valeur du paramètre, ou <c>null</c> si la clé n'existe pas.
/// </returns>
/// <remarks>
/// Cette méthode utilise un cache en mémoire avec les caractéristiques suivantes :
/// <list type="bullet">
/// <item>Durée de vie (TTL) : 5 minutes</item>
/// <item>Invalidation automatique en cas de mise à jour</item>
/// <item>Premier appel : ~50ms (lecture BDD)</item>
/// <item>Appels suivants : ~1ms (lecture cache)</item>
/// </list>
/// 
/// Le cache est partagé entre toutes les instances de cette classe (singleton).
/// Pour forcer le rafraîchissement, utiliser <see cref="InvalidateCache"/>.
/// </remarks>
public async Task<string?> GetSettingAsync(string key)
{
    // ...
}
```

## Sécurité et Validation

### Documentation des Contraintes de Sécurité

```csharp
/// <summary>
/// Authentifie un utilisateur avec ses identifiants.
/// </summary>
/// <param name="email">L'adresse e-mail de l'utilisateur.</param>
/// <param name="password">Le mot de passe en clair (sera haché avant comparaison).</param>
/// <returns>
/// Un jeton JWT valide pendant 1 heure si l'authentification réussit,
/// sinon <c>null</c>.
/// </returns>
/// <remarks>
/// 🔒 SÉCURITÉ :
/// <list type="bullet">
/// <item>Le mot de passe n'est JAMAIS stocké en clair</item>
/// <item>Utilise BCrypt avec 12 rounds de hachage</item>
/// <item>Protection contre les attaques par timing (comparison constante)</item>
/// <item>Limite de 5 tentatives par 15 minutes (IP + e-mail)</item>
/// <item>Logs des tentatives échouées pour audit</item>
/// </list>
/// 
/// ⚠️ Le paramètre <paramref name="password"/> est sensible et ne doit
/// JAMAIS être loggué ou affiché dans les messages d'erreur.
/// </remarks>
/// <exception cref="AccountLockedException">
/// Levée si le compte est temporairement bloqué après trop de tentatives échouées.
/// Le compte se débloque automatiquement après 15 minutes.
/// </exception>
public async Task<string?> AuthenticateAsync(string email, string password)
{
    // ...
}
```

### Validation et Sanitization

```csharp
/// <summary>
/// Nettoie une chaîne HTML en supprimant les balises dangereuses.
/// </summary>
/// <param name="html">Le contenu HTML à nettoyer.</param>
/// <param name="allowedTags">
/// Liste des balises HTML autorisées (défaut : p, br, strong, em, a, ul, ol, li).
/// </param>
/// <returns>
/// Le contenu HTML nettoyé, sécurisé contre les injections XSS.
/// </returns>
/// <remarks>
/// 🔒 PROTECTION XSS :
/// <list type="bullet">
/// <item>Supprime tous les scripts JavaScript (balises, événements, attributs)</item>
/// <item>Nettoie les attributs dangereux (onclick, onerror, onload, etc.)</item>
/// <item>Encode les caractères spéciaux dans les attributs</item>
/// <item>Valide les URLs dans les liens et images (http/https uniquement)</item>
/// <item>Supprime les balises non autorisées</item>
/// </list>
/// 
/// Cette méthode utilise la bibliothèque HtmlSanitizer conforme OWASP.
/// </remarks>
/// <example>
/// <code>
/// var input = "&lt;p&gt;Texte&lt;/p&gt;&lt;script&gt;alert('XSS')&lt;/script&gt;";
/// var safe = SanitizeHtml(input);
/// // Résultat : "&lt;p&gt;Texte&lt;/p&gt;"
/// </code>
/// </example>
public string SanitizeHtml(string html, IEnumerable<string>? allowedTags = null)
{
    // ...
}
```

## Gestion d'Erreurs et Résilience

### Retry et Circuit Breaker

```csharp
/// <summary>
/// Appelle un service externe avec politique de réessai automatique.
/// </summary>
/// <param name="url">L'URL du service à appeler.</param>
/// <param name="cancellationToken">Jeton d'annulation.</param>
/// <returns>
/// La réponse du service si l'appel réussit.
/// </returns>
/// <remarks>
/// 🔄 POLITIQUE DE RÉSILIENCE :
/// <list type="bullet">
/// <item>3 tentatives maximum avec délai exponentiel (1s, 2s, 4s)</item>
/// <item>Circuit breaker ouvert après 5 échecs consécutifs (fenêtre de 30s)</item>
/// <item>Timeout de 10 secondes par tentative</item>
/// <item>Retry uniquement sur erreurs transitoires (5xx, timeout, réseau)</item>
/// <item>Pas de retry sur erreurs client (4xx)</item>
/// </list>
/// 
/// Lorsque le circuit breaker est ouvert, les appels échouent immédiatement
/// avec <see cref="BrokenCircuitException"/> pour éviter de surcharger le service défaillant.
/// </remarks>
/// <exception cref="HttpRequestException">
/// Levée après épuisement des tentatives de réessai.
/// </exception>
/// <exception cref="BrokenCircuitException">
/// Levée si le circuit breaker est ouvert (service considéré comme défaillant).
/// </exception>
/// <exception cref="TimeoutException">
/// Levée si le timeout global (30s) est atteint.
/// </exception>
public async Task<HttpResponseMessage> CallExternalServiceAsync(
    string url,
    CancellationToken cancellationToken = default)
{
    // ...
}
```

## Migration et Dépréciation

### Documentation de Code Déprécié

```csharp
/// <summary>
/// Récupère un utilisateur par son identifiant numérique.
/// </summary>
/// <param name="id">L'identifiant numérique de l'utilisateur.</param>
/// <returns>L'utilisateur correspondant, ou <c>null</c> si introuvable.</returns>
/// <remarks>
/// ⚠️ DÉPRÉCIÉ : Cette méthode sera supprimée dans la version 3.0 (prévue pour juin 2026).
/// 
/// <para>
/// <strong>Raison de la dépréciation :</strong>
/// Migration des identifiants de <c>int</c> vers <c>Guid</c> pour améliorer
/// la scalabilité et la sécurité (ADR-042).
/// </para>
/// 
/// <para>
/// <strong>Migration recommandée :</strong>
/// Utiliser <see cref="GetUserByIdAsync(Guid, CancellationToken)"/> à la place.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // ❌ Ancien code (déprécié)
/// var user = await repo.GetUserByIdAsync(12345);
/// 
/// // ✅ Nouveau code (recommandé)
/// var userId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
/// var user = await repo.GetUserByIdAsync(userId);
/// </code>
/// </example>
[Obsolete("Utiliser GetUserByIdAsync(Guid, CancellationToken) à la place. Sera supprimé en v3.0.", false)]
public async Task<User?> GetUserByIdAsync(int id)
{
    // ...
}
```

## Ressources et Références

### Documentation Officielle Microsoft
- [XML Documentation Comments (C#)](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)
- [Recommended XML tags for C# documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags)
- [Document your code with XML comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)

### Standards
- [Microsoft Docs Style Guide](https://learn.microsoft.com/en-us/style-guide/welcome/)
- [.NET API Documentation Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
