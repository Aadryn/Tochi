#!/usr/bin/env python3
"""
Complete translation script for C# documentation instructions.
Translates all remaining French text to American English.
"""

import re
import codecs

file_path = "/workspaces/proxy/.github/instructions/csharp.documentation.instructions.md"

# Read file
with codecs.open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Comprehensive translation dictionary
translations = {
    # Verb forms in code comments
    "Calcule le total des éléments dans la collection.": "Calculates the total of items in the collection.",
    "Valide que l'adresse e-mail respecte le format standard (ex: utilisateur@domaine.com).": "Validates that the email address follows the standard format (e.g., user@domain.com).",
    "Valide que l'adresse e-mail respecte le format standard": "Validates that the email address follows the standard format",
    "Convertit une chaîne JSON en objet typé.": "Converts a JSON string to a typed object.",
    "Calcule le prix total en euros, incluant la TVA à 20%.": "Calculates the total price in euros, including 20% VAT.",
    "Calcule le prix.": "Calculates the price.",
    "Enregistre l'utilisateur dans la base de données de manière asynchrone.": "Saves the user to the database asynchronously.",
    "Enregistre l'utilisateur dans la base de données.": "Saves the user to the database.",
    "Valide et enregistre les modifications apportées à l'entité dans la base de données.": "Validates and saves changes made to the entity in the database.",
    "Valide une adresse e-mail et retourne une version normalisée.": "Validates an email address and returns a normalized version.",
    "Calcule le coût d'expédition selon le type de produit.": "Calculates the shipping cost based on the product type.",
    "Charge l'ensemble des produits avec leurs catégories et images associées.": "Loads all products with their associated categories and images.",
    
    # Best practices section
    "Commencer par un verbe d'action (Calcule, Valide, Enregistre, Recherche)": "Start with an action verb (Calculates, Validates, Saves, Searches)",
    "Commencer par un verbe d'action": "Start with an action verb",
    "Phrases complètes avec sujet-verbe-complément": "Complete sentences with subject-verb-object",
    "Préciser les unités (secondes, mètres, euros)": "Specify units (seconds, meters, euros)",
    "Indiquer les valeurs possibles": "Indicate possible values",
    "Descriptions vagues": "Vague descriptions",
    "Répéter le nom de la méthode": "Repeat the method name",
    "Termes ambigus": "Ambiguous terms",
    "Jargon technique non expliqué": "Unexplained technical jargon",
    "Précis et actionnable": "Precise and actionable",
    "Vague et imprécis": "Vague and imprecise",
    
    # Documentation specific terms
    "Le prix hors taxes en euros.": "The price excluding tax in euros.",
    "Le prix TTC arrondi à 2 décimales.": "The price including tax rounded to 2 decimals.",
    "Le prix.": "The price.",
    "Le total.": "The total.",
    "L'utilisateur à enregistrer.": "The user to save.",
    "L'utilisateur.": "The user.",
    "Jeton permettant d'annuler l'opération en cours.": "Token allowing to cancel the operation in progress.",
    "Une tâche représentant l'opération asynchrone.": "A task representing the asynchronous operation.",
    "Le résultat contient l'identifiant de l'utilisateur créé.": "The result contains the identifier of the created user.",
    "Cette méthode ne bloque pas le thread appelant.": "This method does not block the calling thread.",
    "En cas d'annulation via": "In case of cancellation via",
    "une": "a",
    "est levée.": "is thrown.",
    
    # Special cases translations
    "Valeurs <c>null</c> acceptées ou retournées": "<c>null</c> values accepted or returned",
    "Collections vides vs <c>null</c>": "Empty collections vs <c>null</c>",
    "Valeurs par défaut": "Default values",
    "Valeurs limites (min/max)": "Limit values (min/max)",
    "Recherche le premier élément correspondant au prédicat.": "Finds the first element matching the predicate.",
    "La condition de recherche. Ne doit pas être <c>null</c>.": "The search condition. Must not be <c>null</c>.",
    "Ne doit pas être": "Must not be",
    "L'élément trouvé, ou": "The found element, or",
    "si aucun élément ne correspond.": "if no element matches.",
    "Une collection vide retourne toujours": "An empty collection always returns",
    
    # Async documentation
    "Jeton permettant d'annuler l'opération en cours.": "Token allowing to cancel the ongoing operation.",
    "Le résultat contient": "The result contains",
    
    # Interface documentation
    "Définit un contrat pour les services de notification.": "Defines a contract for notification services.",
    "Les implémentations de cette interface DOIVENT garantir": "Implementations of this interface MUST guarantee",
    "L'envoi asynchrone des notifications": "Asynchronous sending of notifications",
    "La gestion des erreurs d'envoi": "Handling of send errors",
    "La traçabilité des notifications envoyées": "Traceability of sent notifications",
    "Envoie une notification à un destinataire.": "Sends a notification to a recipient.",
    "L'adresse du destinataire.": "The recipient's address.",
    "Le contenu de la notification.": "The notification content.",
    "Le résultat indique si l'envoi a réussi.": "The result indicates whether the send was successful.",
    
    # Anti-patterns
    "Répète le code sans apporter de valeur": "Repeats the code without adding value",
    "Obtient ou définit le nom.": "Gets or sets the name.",
    "Apporte une information utile": "Provides useful information",
    "Nom complet de l'utilisateur (prénom et nom de famille).": "User's full name (first name and last name).",
    "Limité à 100 caractères. Les espaces multiples sont automatiquement réduits.": "Limited to 100 characters. Multiple spaces are automatically reduced.",
    "La documentation ne correspond plus au code": "The documentation no longer matches the code",
    "Retourne une liste d'utilisateurs actifs.": "Returns a list of active users.",
    "Une liste d'utilisateurs.": "A list of users.",
    "Nouveau paramètre non documenté": "New parameter not documented",
    "Retourne une collection d'utilisateurs filtrés selon leur statut.": "Returns a collection of users filtered by their status.",
    "Si <c>true</c>, inclut également les utilisateurs inactifs ;": "If <c>true</c>, also includes inactive users;",
    "sinon, retourne uniquement les utilisateurs actifs.": "otherwise, returns only active users.",
    "Une collection énumérable d'utilisateurs correspondant au filtre.": "An enumerable collection of users matching the filter.",
    "Détails d'implémentation excessifs": "Excessive implementation details",
    "Utilise l'algorithme de tri QuickSort avec pivot médian pour trier": "Uses QuickSort algorithm with median pivot to sort",
    "la collection en O(n log n) via une implémentation récursive tail-call optimisée.": "the collection in O(n log n) via an optimized tail-call recursive implementation.",
    "Se concentre sur l'usage": "Focuses on usage",
    "Trie la collection par ordre croissant.": "Sorts the collection in ascending order.",
    "Cette méthode modifie la collection d'origine.": "This method modifies the original collection.",
    "Pour les grandes collections": "For large collections",
    "privilégier la méthode asynchrone.": "prefer the asynchronous method.",
    "Trop vague, pas actionnable": "Too vague, not actionable",
    "Gère les données.": "Manages the data.",
    "Résultat de l'opération.": "Operation result.",
    "Précis et descriptif": "Precise and descriptive",
    "si la validation a échoué.": "if validation failed.",
    "si l'enregistrement a réussi ;": "if the save was successful;",
    
    # Checklist
    "Tous les membres publics ont un": "All public members have a",
    "Tous les paramètres ont un": "All parameters have a",
    "Toutes les méthodes non-void ont un": "All non-void methods have a",
    "Toutes les exceptions levées ont un": "All thrown exceptions have an",
    "Les APIs complexes ont un": "Complex APIs have an",
    "Documentation en français correct (grammaire, orthographe)": "Documentation in correct American English (grammar, spelling)",
    "Ton didactique et compréhensible par novices": "Didactic tone understandable by novices",
    "Aucun pronom personnel": "No personal pronouns",
    "Aucune référence à des outils/processus/IDs internes": "No references to internal tools/processes/IDs",
    "Descriptions précises et factuelles (pas d'invention)": "Precise and factual descriptions (no invention)",
    "documentées (paramètres et retours)": "documented (parameters and returns)",
    "Exceptions documentées avec conditions de déclenchement": "Exceptions documented with trigger conditions",
    "Comportements asynchrones expliqués": "Asynchronous behaviors explained",
    "Unités et formats spécifiés (dates, montants, durées)": "Units and formats specified (dates, amounts, durations)",
    "Contraintes et validations mentionnées": "Constraints and validations mentioned",
    "Tags XML valides et bien formés": "Valid and well-formed XML tags",
    "Références": "References",
    "correctes": "correct",
    "Code d'exemple compilable dans": "Compilable example code in",
    "Indentation cohérente des commentaires XML": "Consistent XML comments indentation",
    
    # Special cases - Records
    "Représente un point géographique immuable avec coordonnées GPS.": "Represents an immutable geographic point with GPS coordinates.",
    "Latitude en degrés décimaux (valeur entre -90 et +90).": "Latitude in decimal degrees (value between -90 and +90).",
    "Longitude en degrés décimaux (valeur entre -180 et +180).": "Longitude in decimal degrees (value between -180 and +180).",
    "Ce record est immutable : les valeurs ne peuvent pas être modifiées après création.": "This record is immutable: values cannot be modified after creation.",
    "Utilisez l'expression <c>with</c> pour créer une copie modifiée.": "Use the <c>with</c> expression to create a modified copy.",
    
    # Extension methods
    "Fournit des méthodes d'extension pour la manipulation de chaînes de caractères.": "Provides extension methods for string manipulation.",
    "Tronque la chaîne à la longueur spécifiée en ajoutant des points de suspension si nécessaire.": "Truncates the string to the specified length by adding ellipsis if needed.",
    "La chaîne à tronquer.": "The string to truncate.",
    "Longueur maximale de la chaîne résultante, points de suspension inclus.": "Maximum length of the resulting string, ellipsis included.",
    "Doit être supérieur ou égal à 3.": "Must be greater than or equal to 3.",
    "La chaîne tronquée avec \"...\" si elle dépasse": "The truncated string with \"...\" if it exceeds",
    "sinon la chaîne originale.": "otherwise the original string.",
    "Levée si <paramref name=\"value\"/> est <c>null</c>.": "Thrown when <paramref name=\"value\"/> is <c>null</c>.",
    "Levée si <paramref name=\"maxLength\"/> est inférieur à 3.": "Thrown when <paramref name=\"maxLength\"/> is less than 3.",
    
    # Generic constraints
    "Référentiel générique pour accéder aux entités d'un type spécifique.": "Generic repository for accessing entities of a specific type.",
    "Le type d'entité géré par ce référentiel.": "The entity type managed by this repository.",
    "Doit implémenter": "Must implement",
    "et avoir un constructeur sans paramètre.": "and have a parameterless constructor.",
    "Le type de l'identifiant de l'entité.": "The entity's identifier type.",
    "Doit être un type valeur comparable.": "Must be a comparable value type.",
    "Cette classe fournit les opérations CRUD de base pour toute entité du domaine.": "This class provides basic CRUD operations for any domain entity.",
    "Les contraintes génériques garantissent la cohérence des types manipulés.": "Generic constraints ensure consistency of manipulated types.",
    "Récupère une entité par son identifiant.": "Retrieves an entity by its identifier.",
    "L'identifiant unique de l'entité.": "The entity's unique identifier.",
    "L'entité correspondante, ou": "The corresponding entity, or",
    "si aucune entité avec cet identifiant n'existe.": "if no entity with this identifier exists.",
    
    # Nullable reference types
    "Service de validation d'adresses e-mail avec support nullable.": "Email address validation service with nullable support.",
    "L'adresse e-mail à valider. Peut être <c>null</c> ou vide.": "The email address to validate. Can be <c>null</c> or empty.",
    "Sortie :": "Output:",
    "L'adresse e-mail normalisée (minuscules, espaces supprimés) si valide,": "The normalized email address (lowercase, spaces removed) if valid,",
    "sinon": "otherwise",
    "Une adresse <c>null</c> ou vide est considérée comme invalide.": "A <c>null</c> or empty address is considered invalid.",
    "La validation vérifie le format selon la RFC 5322 (simplifié).": "Validation checks the format according to RFC 5322 (simplified).",
    
    # Operator overloading
    "Représente une durée en heures et minutes.": "Represents a duration in hours and minutes.",
    "Additionne deux durées.": "Adds two durations.",
    "La première durée.": "The first duration.",
    "La seconde durée.": "The second duration.",
    "Une nouvelle durée représentant la somme des deux durées.": "A new duration representing the sum of the two durations.",
    "Les minutes sont automatiquement converties en heures si elles dépassent 59.": "Minutes are automatically converted to hours if they exceed 59.",
    
    # Pattern matching
    "Le produit à expédier.": "The product to ship.",
    "Le coût d'expédition en euros.": "The shipping cost in euros.",
    "Le calcul utilise les règles suivantes": "The calculation uses the following rules",
    "Produit physique": "Physical product",
    "Produit numérique": "Digital product",
    "Produit sur mesure": "Custom product",
    
    # Business scenarios - Repository
    "Référentiel pour la gestion des utilisateurs dans la base de données.": "Repository for user management in the database.",
    "Cette implémentation utilise Entity Framework Core pour l'accès aux données.": "This implementation uses Entity Framework Core for data access.",
    "Toutes les opérations sont tracées via": "All operations are traced via",
    "Recherche des utilisateurs selon plusieurs critères de filtrage.": "Searches for users based on multiple filter criteria.",
    "Les critères de recherche. Tous les champs <c>null</c> sont ignorés.": "The search criteria. All <c>null</c> fields are ignored.",
    "Paramètres de pagination (page, taille). Si <c>null</c>, retourne tous les résultats.": "Pagination parameters (page, size). If <c>null</c>, returns all results.",
    "Jeton d'annulation pour interrompre l'opération.": "Cancellation token to interrupt the operation.",
    "Une tâche contenant les résultats paginés": "A task containing the paginated results",
    "Les utilisateurs correspondant aux critères": "Users matching the criteria",
    "Nombre total de résultats (avant pagination)": "Total number of results (before pagination)",
    "Numéro de la page actuelle (base 1)": "Current page number (1-based)",
    "Taille de la page": "Page size",
    "Levée si <paramref name=\"criteria\"/> est <c>null</c>.": "Thrown when <paramref name=\"criteria\"/> is <c>null</c>.",
    "Levée si l'opération est annulée via": "Thrown if the operation is cancelled via",
    
    # CQRS
    "Commande pour créer un nouvel utilisateur dans le système.": "Command to create a new user in the system.",
    "Cette commande déclenche les actions suivantes": "This command triggers the following actions",
    "Validation des données (e-mail unique, mot de passe conforme)": "Data validation (unique email, compliant password)",
    "Hachage sécurisé du mot de passe": "Secure password hashing",
    "Création de l'enregistrement en base": "Creation of the database record",
    "Envoi d'un e-mail de bienvenue": "Sending a welcome email",
    "Publication d'un événement": "Publishing an event",
    "Gestionnaire de la commande de création d'utilisateur.": "Handler for the user creation command.",
    "Traite la commande de création d'un utilisateur.": "Processes the user creation command.",
    "La commande contenant les données de l'utilisateur à créer.": "The command containing the data of the user to create.",
    "Jeton d'annulation.": "Cancellation token.",
    "Une tâche contenant le résultat de la création": "A task containing the creation result",
    "L'identifiant unique de l'utilisateur créé": "The unique identifier of the created user",
    "Indique si la création a réussi": "Indicates whether the creation was successful",
    "Liste des erreurs de validation si applicable": "List of validation errors if applicable",
    "Levée si les données de la commande ne respectent pas les règles métier.": "Thrown if the command data does not comply with business rules.",
    "Contient la liste détaillée des erreurs de validation.": "Contains the detailed list of validation errors.",
    "Levée si un utilisateur avec cet e-mail existe déjà.": "Thrown if a user with this email already exists.",
    
    # Domain events
    "Événement déclenché lorsqu'une commande est confirmée par le client.": "Event triggered when an order is confirmed by the customer.",
    "Cet événement marque la transition de l'état \"En attente\" vers \"Confirmée\".": "This event marks the transition from \"Pending\" state to \"Confirmed\".",
    "Les gestionnaires de cet événement déclenchent généralement": "Handlers of this event typically trigger",
    "Notification au vendeur": "Notification to the seller",
    "Déclenchement du processus de préparation": "Triggering the preparation process",
    "Mise à jour du stock": "Stock update",
    "Création de la facture": "Invoice creation",
    "Identifiant unique de la commande confirmée.": "Unique identifier of the confirmed order.",
    "Date et heure UTC de la confirmation.": "UTC date and time of confirmation.",
    "Montant total de la commande en euros.": "Total order amount in euros.",
    "Crée un nouvel événement de confirmation de commande.": "Creates a new order confirmation event.",
    "L'identifiant de la commande.": "The order identifier.",
    "Le montant total en euros.": "The total amount in euros.",
    
    # Performance
    "⚠️ ATTENTION PERFORMANCE": "⚠️ PERFORMANCE WARNING",
    "Cette méthode charge TOUS les produits en mémoire (eager loading)": "This method loads ALL products into memory (eager loading)",
    "Utilise 3 requêtes SQL via Include() pour éviter le problème N+1": "Uses 3 SQL queries via Include() to avoid the N+1 problem",
    "Temps d'exécution typique": "Typical execution time",
    "pour 1000 produits": "for 1000 products",
    "Mémoire consommée": "Memory consumed",
    "Pour de grandes quantités de données": "For large amounts of data",
    "privilégier": "prefer",
    "pour traitement par flux": "for stream processing",
    "pour pagination": "for pagination",
    "✅ OPTIMISÉ POUR GRANDES QUANTITÉS": "✅ OPTIMIZED FOR LARGE QUANTITIES",
    "Traite les produits par lots de": "Processes products in batches of",
    "éléments": "elements",
    "Libère la mémoire entre chaque lot": "Frees memory between each batch",
    "Convient pour plus de": "Suitable for more than",
    "produits": "products",
    "Mémoire maximale": "Maximum memory",
    "quelle que soit la quantité totale": "regardless of total quantity",
    "Traite les produits par flux pour minimiser l'utilisation mémoire.": "Processes products by stream to minimize memory usage.",
    "Fonction de traitement appelée pour chaque produit.": "Processing function called for each product.",
    "Nombre de produits traités par lot (défaut : 100).": "Number of products processed per batch (default: 100).",
    
    # Caching
    "Récupère les paramètres de configuration avec mise en cache.": "Retrieves configuration parameters with caching.",
    "La clé du paramètre.": "The parameter key.",
    "La valeur du paramètre, ou": "The parameter value, or",
    "si la clé n'existe pas.": "if the key does not exist.",
    "Cette méthode utilise un cache en mémoire avec les caractéristiques suivantes": "This method uses an in-memory cache with the following characteristics",
    "Durée de vie (TTL)": "Time to live (TTL)",
    "minutes": "minutes",
    "Invalidation automatique en cas de mise à jour": "Automatic invalidation on update",
    "Premier appel": "First call",
    "lecture BDD": "database read",
    "Appels suivants": "Subsequent calls",
    "lecture cache": "cache read",
    "Le cache est partagé entre toutes les instances de cette classe (singleton).": "The cache is shared among all instances of this class (singleton).",
    "Pour forcer le rafraîchissement, utiliser": "To force refresh, use",
    
    # Security
    "Authentifie un utilisateur avec ses identifiants.": "Authenticates a user with their credentials.",
    "L'adresse e-mail de l'utilisateur.": "The user's email address.",
    "Le mot de passe en clair (sera haché avant comparaison).": "The password in plain text (will be hashed before comparison).",
    "Un jeton JWT valide pendant 1 heure si l'authentification réussit,": "A JWT token valid for 1 hour if authentication succeeds,",
    "🔒 SÉCURITÉ": "🔒 SECURITY",
    "Le mot de passe n'est JAMAIS stocké en clair": "The password is NEVER stored in plain text",
    "Utilise BCrypt avec 12 rounds de hachage": "Uses BCrypt with 12 hashing rounds",
    "Protection contre les attaques par timing (comparison constante)": "Protection against timing attacks (constant comparison)",
    "Limite de 5 tentatives par 15 minutes (IP + e-mail)": "Limit of 5 attempts per 15 minutes (IP + email)",
    "Logs des tentatives échouées pour audit": "Logs of failed attempts for audit",
    "⚠️ Le paramètre": "⚠️ The parameter",
    "est sensible et ne doit": "is sensitive and must",
    "JAMAIS être loggué ou affiché dans les messages d'erreur.": "NEVER be logged or displayed in error messages.",
    "Levée si le compte est temporairement bloqué après trop de tentatives échouées.": "Thrown if the account is temporarily locked after too many failed attempts.",
    "Le compte se débloque automatiquement après 15 minutes.": "The account unlocks automatically after 15 minutes.",
    
    # Sanitization
    "Nettoie une chaîne HTML en supprimant les balises dangereuses.": "Cleans an HTML string by removing dangerous tags.",
    "Le contenu HTML à nettoyer.": "The HTML content to clean.",
    "Liste des balises HTML autorisées (défaut : p, br, strong, em, a, ul, ol, li).": "List of allowed HTML tags (default: p, br, strong, em, a, ul, ol, li).",
    "Le contenu HTML nettoyé, sécurisé contre les injections XSS.": "The cleaned HTML content, secured against XSS injections.",
    "🔒 PROTECTION XSS": "🔒 XSS PROTECTION",
    "Supprime tous les scripts JavaScript (balises, événements, attributs)": "Removes all JavaScript scripts (tags, events, attributes)",
    "Nettoie les attributs dangereux (onclick, onerror, onload, etc.)": "Cleans dangerous attributes (onclick, onerror, onload, etc.)",
    "Encode les caractères spéciaux dans les attributs": "Encodes special characters in attributes",
    "Valide les URLs dans les liens et images (http/https uniquement)": "Validates URLs in links and images (http/https only)",
    "Supprime les balises non autorisées": "Removes unauthorized tags",
    "Cette méthode utilise la bibliothèque HtmlSanitizer conforme OWASP.": "This method uses the OWASP-compliant HtmlSanitizer library.",
    "Résultat": "Result",
    
    # Retry and Circuit Breaker
    "Appelle un service externe avec politique de réessai automatique.": "Calls an external service with automatic retry policy.",
    "L'URL du service à appeler.": "The URL of the service to call.",
    "La réponse du service si l'appel réussit.": "The service response if the call succeeds.",
    "🔄 POLITIQUE DE RÉSILIENCE": "🔄 RESILIENCE POLICY",
    "3 tentatives maximum avec délai exponentiel": "3 maximum attempts with exponential delay",
    "Circuit breaker ouvert après 5 échecs consécutifs (fenêtre de 30s)": "Circuit breaker opened after 5 consecutive failures (30s window)",
    "Timeout de 10 secondes par tentative": "10 seconds timeout per attempt",
    "Retry uniquement sur erreurs transitoires (5xx, timeout, réseau)": "Retry only on transient errors (5xx, timeout, network)",
    "Pas de retry sur erreurs client (4xx)": "No retry on client errors (4xx)",
    "Lorsque le circuit breaker est ouvert, les appels échouent immédiatement": "When the circuit breaker is open, calls fail immediately",
    "avec": "with",
    "pour éviter de surcharger le service défaillant.": "to avoid overloading the failing service.",
    "Levée après épuisement des tentatives de réessai.": "Thrown after exhausting retry attempts.",
    "Levée si le circuit breaker est ouvert (service considéré comme défaillant).": "Thrown if the circuit breaker is open (service considered failing).",
    "Levée si le timeout global (30s) est atteint.": "Thrown if the global timeout (30s) is reached.",
    
    # Deprecated code
    "Récupère un utilisateur par son identifiant numérique.": "Retrieves a user by their numeric identifier.",
    "L'identifiant numérique de l'utilisateur.": "The user's numeric identifier.",
    "L'utilisateur correspondant, ou": "The corresponding user, or",
    "si introuvable.": "if not found.",
    "⚠️ DÉPRÉCIÉ : Cette méthode sera supprimée dans la version 3.0 (prévue pour juin 2026).": "⚠️ DEPRECATED: This method will be removed in version 3.0 (scheduled for June 2026).",
    "Raison de la dépréciation": "Deprecation reason",
    "Migration des identifiants de <c>int</c> vers <c>Guid</c> pour améliorer": "Migration of identifiers from <c>int</c> to <c>Guid</c> to improve",
    "la scalabilité et la sécurité (ADR-042).": "scalability and security (ADR-042).",
    "Migration recommandée": "Recommended migration",
    "Utiliser": "Use",
    "à la place.": "instead.",
    "Ancien code (déprécié)": "Old code (deprecated)",
    "Nouveau code (recommandé)": "New code (recommended)",
}

# Apply all translations
print("Applying comprehensive translations...")
count = 0
for french, english in translations.items():
    if french in content:
        content = content.replace(french, english)
        count += 1
        print(f"  ✓ {french[:60]}...")

print(f"\n✅ Applied {count} translations")

# Write back
with codecs.open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print(f"✅ File updated: {file_path}")

# Check remaining French
print("\nChecking for remaining French words...")
french_pattern = re.compile(r'\b(utilisateur|données|méthode|propriété|fonction|retourne|obtient|définit|calcule|valide|enregistre)\b', re.IGNORECASE)
matches = french_pattern.findall(content)
if matches:
    print(f"⚠️ Found {len(set(matches))} unique French words remaining:")
    for word in sorted(set(matches)):
        print(f"  - {word}")
else:
    print("✅ No common French words detected!")
