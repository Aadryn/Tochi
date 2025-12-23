#!/usr/bin/env python3
"""Translate all French code examples and comments to American English."""

import codecs
import re

file_path = "/workspaces/proxy/.github/instructions/csharp.documentation.instructions.md"

# Read file
with codecs.open(file_path, 'r', encoding='utf-8-sig') as f:
    content = f.read()

# Comprehensive translations for code examples and comments
translations = {
    # Action verbs in method names and comments
    r'\bCalcule\b': 'Calculates',
    r'\bValide\b': 'Validates',
    r'\bEnregistre\b': 'Saves',
    r'\bRecherche\b': 'Searches',
    r'\bRetourne\b': 'Returns',
    r'\bObtient\b': 'Gets',
    r'\bDéfinit\b': 'Sets',
    
    # Common French phrases in examples
    r'le total des éléments dans la collection': 'the total of elements in the collection',
    r"que l'adresse e-mail respecte le format standard": 'that the email address follows the standard format',
    r"Cette méthode vérifie la présence d'un '@' et d'un domaine valide": "This method checks for the presence of an '@' and a valid domain",
    r'un utilisateur par son adresse e-mail dans la base de données': 'a user by their email address in the database',
    r"L'utilisateur correspondant à l'adresse e-mail, ou <c>null</c> si aucun utilisateur n'est trouvé": "The user matching the email address, or <c>null</c> if no user is found",
    r"Levée en cas d'erreur de connexion ou de requête à la base de données": "Thrown in case of connection error or database query failure",
    r"Ce que représente la propriété": "What the property represents",
    r'pas "obtient ou définit"': 'not "gets or sets"',
    r"Cette propriété est en lecture seule après l'initialisation": "This property is read-only after initialization",
    r"Âge de l'utilisateur calculé à partir de sa date de naissance": "User's age calculated from their date of birth",
    r"Cette propriété est recalculée à chaque accès en fonction de la date actuelle": "This property is recalculated on each access based on the current date",
    r"Déclenché lorsque la valeur de la propriété change": "Triggered when the property value changes",
    r"Définit les différents niveaux de priorité pour les tâches": "Defines the different priority levels for tasks",
    
    # Table entries
    r'Méthodes avec paramètres': 'Methods with parameters',
    r'Description de CHAQUE paramètre': 'Description of EACH parameter',
    r'Méthodes non-void': 'Non-void methods',
    r'Description précise de la valeur retournée': 'Precise description of the returned value',
    r'méthodes complexes': 'complex methods',
    r'Détails supplémentaires, cas d\'usage, contraintes': 'Additional details, use cases, constraints',
    r'Références à d\'autres types': 'References to other types',
    r'Lien vers classe, méthode ou propriété liée': 'Link to related class, method or property',
    
    # Code comment phrases
    r'Une instance de type <typeparamref name="T"/> contenant les données désérialisées': 'An instance of type <typeparamref name="T"/> containing the deserialized data',
    r"Levée si <paramref name=\"json\"/> n'est pas un JSON valide": 'Thrown if <paramref name="json"/> is not valid JSON',
    r'Cette méthode utilise <see cref="System.Text.Json.JsonSerializer"/> pour la désérialisation': 'This method uses <see cref="System.Text.Json.JsonSerializer"/> for deserialization',
    
    # Best practices
    r'Commencer par un verbe d\'action \(Calcule, Valide, Enregistre, Recherche\)': 'Start with an action verb (Calculates, Validates, Saves, Searches)',
    r'Descriptions vagues \("Fait quelque chose", "Gère les données"\)': 'Vague descriptions ("Does something", "Handles data")',
    r'Répéter le nom de la méthode \("GetUser obtient un utilisateur"\)': 'Repeating the method name ("GetUser gets a user")',
    
    # Specific examples
    r'le prix total en euros, incluant la TVA à 20%': 'the total price in euros, including 20% VAT',
    r'le prix': 'the price',
    r'Une collection vide retourne toujours <c>null</c>': 'An empty collection always returns <c>null</c>',
    r"l'utilisateur dans la base de données de manière asynchrone": 'the user to the database asynchronously',
    r"<param name=\"user\">L'utilisateur à enregistrer.</param>": '<param name="user">The user to save.</param>',
    r"Le résultat contient l'identifiant de l'utilisateur créé": "The result contains the created user's identifier",
    r'Cette méthode ne bloque pas le thread appelant': 'This method does not block the calling thread',
    r'Définit un contrat pour les services de notification': 'Defines a contract for notification services',
    r'Obtient ou définit le nom': 'Gets or sets the name',
    r"Nom complet de l'utilisateur \(prénom et nom de famille\)": "User's full name (first and last name)",
    r"une liste d'utilisateurs actifs": 'a list of active users',
    r"Une liste d'utilisateurs": 'A list of users',
    r"une collection d'utilisateurs filtrés selon leur statut": 'a collection of users filtered by their status',
    r"Si <c>true</c>, inclut également les utilisateurs inactifs": 'If <c>true</c>, also includes inactive users',
    r'sinon, retourne uniquement les utilisateurs actifs': 'otherwise, returns only active users',
    r"Une collection énumérable d'utilisateurs correspondant au filtre": 'An enumerable collection of users matching the filter',
    r"Cette méthode modifie la collection d'origine": 'This method modifies the original collection',
    r'Pour les grandes collections \(> 10 000 éléments\), privilégier la méthode asynchrone': 'For large collections (> 10,000 elements), prefer the asynchronous method',
    r'Gère les données': 'Handles data',
    r"Valide et enregistre les modifications apportées à l'entité dans la base de données": 'Validates and saves the changes made to the entity in the database',
    r"<c>true</c> si l'enregistrement a réussi": '<c>true</c> if the save was successful',
    r'Toutes les méthodes non-void ont un `<returns>`': 'All non-void methods have a `<returns>`',
    r'Documentation en français correct \(grammaire, orthographe\)': 'Documentation in correct American English (grammar, spelling)',
    r'Représente un point géographique immuable avec coordonnées GPS': 'Represents an immutable geographic point with GPS coordinates',
    r'Fournit des méthodes d\'extension pour la manipulation de chaînes de caractères': 'Provides extension methods for string manipulation',
    r"Valide une adresse e-mail et retourne une version normalisée": 'Validates an email address and returns a normalized version',
    r"L'adresse e-mail à valider. Peut être <c>null</c> ou vide": 'The email address to validate. Can be <c>null</c> or empty',
    r"Sortie : L'adresse e-mail normalisée \(minuscules, espaces supprimés\) si valide": 'Output: The normalized email address (lowercase, spaces removed) if valid',
    r"<c>true</c> si l'adresse est valide ; <c>false</c> sinon": '<c>true</c> if the address is valid; <c>false</c> otherwise',
    r"Une adresse <c>null</c> ou vide est considérée comme invalide": 'A <c>null</c> or empty address is considered invalid',
    r'Calcule le coût d\'expédition selon le type de produit': 'Calculates the shipping cost based on the product type',
    r'Référentiel pour la gestion des utilisateurs dans la base de données': 'Repository for managing users in the database',
    r"Cette implémentation utilise Entity Framework Core pour l'accès aux données": 'This implementation uses Entity Framework Core for data access',
    r'Recherche des utilisateurs selon plusieurs critères de filtrage': 'Searches for users based on multiple filtering criteria',
    r'Paramètres de pagination \(page, taille\). Si <c>null</c>, retourne tous les résultats': 'Pagination parameters (page, size). If <c>null</c>, returns all results',
    r"<item><c>Items</c> : Les utilisateurs correspondant aux critères</item>": '<item><c>Items</c>: Users matching the criteria</item>',
    r'Console.WriteLine\(\$"Trouvé \{results.TotalCount\} utilisateurs"\)': 'Console.WriteLine($"Found {results.TotalCount} users")',
    r"Commande pour créer un nouvel utilisateur dans le système": 'Command to create a new user in the system',
    r"<item>Validation des données \(e-mail unique, mot de passe conforme\)</item>": '<item>Data validation (unique email, compliant password)</item>',
    r"<item>Création de l'enregistrement en base</item>": '<item>Creating the database record</item>',
    r"Gestionnaire de la commande de création d'utilisateur": 'Handler for the create user command',
    r"Traite la commande de création d'un utilisateur": 'Processes the command to create a user',
    r"<param name=\"command\">La commande contenant les données de l'utilisateur à créer.</param>": '<param name="command">The command containing the data of the user to create.</param>',
    r"<item><c>UserId</c> : L'identifiant unique de l'utilisateur créé</item>": "<item><c>UserId</c>: The created user's unique identifier</item>",
    r"Levée si les données de la commande ne respectent pas les règles métier": 'Thrown if the command data does not comply with business rules',
    r"Levée si un utilisateur avec cet e-mail existe déjà": 'Thrown if a user with this email already exists',
    r"<item>Cette méthode charge TOUS les produits en mémoire \(eager loading\)</item>": '<item>This method loads ALL products into memory (eager loading)</item>',
    r'Pour de grandes quantités de données \(> 5000 produits\), privilégier :': 'For large amounts of data (> 5000 products), prefer:',
    r'Fonction de traitement appelée pour chaque produit': 'Processing function called for each product',
    r'Cette méthode utilise un cache en mémoire avec les caractéristiques suivantes :': 'This method uses an in-memory cache with the following characteristics:',
    r"Authentifie un utilisateur avec ses identifiants": 'Authenticates a user with their credentials',
    r"<param name=\"email\">L'adresse e-mail de l'utilisateur.</param>": '<param name="email">The user\'s email address.</param>',
    r"Un jeton JWT valide pendant 1 heure si l'authentification réussit": 'A valid JWT token for 1 hour if authentication succeeds',
    r'<item>Valide les URLs dans les liens et images \(http/https uniquement\)</item>': '<item>Validates URLs in links and images (http/https only)</item>',
    r'Cette méthode utilise la bibliothèque HtmlSanitizer conforme OWASP': 'This method uses the OWASP-compliant HtmlSanitizer library',
    r'Récupère un utilisateur par son identifiant numérique': 'Retrieves a user by their numeric identifier',
    r"<param name=\"id\">L'identifiant numérique de l'utilisateur.</param>": '<param name="id">The user\'s numeric identifier.</param>',
    r"<returns>L'utilisateur correspondant, ou <c>null</c> si introuvable.</returns>": '<returns>The matching user, or <c>null</c> if not found.</returns>',
    r'⚠️ DÉPRÉCIÉ : Cette méthode sera supprimée dans la version 3.0 \(prévue pour juin 2026\)': '⚠️ DEPRECATED: This method will be removed in version 3.0 (scheduled for June 2026)',
    
    # Single words that might have been missed
    r'\butilisateur\b': 'user',
    r'\butilisateurs\b': 'users',
    r'\bdonnées\b': 'data',
    r'\bméthode\b': 'method',
    r'\bméthodes\b': 'methods',
    r'\bpropriété\b': 'property',
    r'\bpropriétés\b': 'properties',
    r'\bfrançais\b': 'American English',
}

print("Translating all French code examples and comments...")
count = 0

for french, english in translations.items():
    if re.search(french, content):
        old_content = content
        content = re.sub(french, english, content)
        if content != old_content:
            count += 1
            # Truncate for display
            display_fr = french[:70] + '...' if len(french) > 70 else french
            display_en = english[:70] + '...' if len(english) > 70 else english
            print(f"  ✓ {display_fr}")

# Write back
with codecs.open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print(f"\n✅ Applied {count} code example translations")
print(f"✅ File updated: {file_path}")

# Final check
french_pattern = r'\b(utilisateur|données|méthode|propriété|fonction|retourne|obtient|définit|calcule|valide|enregistre|français)\b'
remaining = set(re.findall(french_pattern, content, re.IGNORECASE))

if remaining:
    print(f"\n⚠️ Found {len(remaining)} unique French words remaining:")
    for word in sorted(remaining, key=str.lower):
        count_word = len(re.findall(r'\b' + re.escape(word) + r'\b', content, re.IGNORECASE))
        print(f"  - {word} ({count_word} occurrences)")
else:
    print("\n✅ No French words detected! Translation fully complete.")
