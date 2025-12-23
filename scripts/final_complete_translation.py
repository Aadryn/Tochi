#!/usr/bin/env python3
"""Final complete translation - clean all remaining French fragments."""

import codecs

file_path = "/workspaces/proxy/.github/instructions/csharp.documentation.instructions.md"

with codecs.open(file_path, 'r', encoding='utf-8-sig') as f:
    content = f.read()

# Complete list of remaining French phrases and fragments
final_translations = [
    # Mixed English-French sentences
    ("This method checks la présence d'un '@' et d'un domaine valide", "This method checks for the presence of an '@' and a valid domain"),
    ("Searches for a user par son adresse e-mail dans la base de données", "Searches for a user by their email address in the database"),
    ("Thrown in case d'erreur de connexion ou de requête à la base de données", "Thrown in case of connection error or database query failure"),
    ("This property is recalculated à chaque accès en fonction de la date actuelle", "This property is recalculated on each access based on the current date"),
    ("Saves the user dans la base de données de manière asynchrone", "Saves the user to the database asynchronously"),
    
    # Pure French sentences
    ("Nom complet de l'utilisateur (prénom et nom de famille)", "User's full name (first and last name)"),
    ("Retourne une liste d'utilisateurs actifs", "Returns a list of active users"),
    ("<returns>Une liste d'utilisateurs.</returns>", "<returns>A list of users.</returns>"),
    ("Retourne une collection d'utilisateurs filtrés selon leur statut", "Returns a collection of users filtered by their status"),
    ("Si <c>true</c>, inclut également les utilisateurs inactifs", "If <c>true</c>, also includes inactive users"),
    ("sinon, retourne uniquement les utilisateurs actifs", "otherwise, returns only active users"),
    ("Une collection énumérable d'utilisateurs correspondant au filtre", "An enumerable collection of users matching the filter"),
    ("Cette méthode modifie la collection d'origine", "This method modifies the original collection"),
    ("Pour les grandes collections (> 10 000 éléments), privilégier la méthode asynchrone", "For large collections (> 10,000 elements), prefer the asynchronous method"),
    ("<c>true</c> si l'enregistrement a réussi", "<c>true</c> if the save was successful"),
    ("Toutes les méthodes non-void ont un `<returns>`", "All non-void methods have a `<returns>`"),
    ("Documentation en français correct (grammaire, orthographe)", "Documentation in correct American English (grammar, spelling)"),
    ("Tags XML valides et bien formés", "Valid and well-formed XML tags"),
    ("Représente un point géographique immuable avec coordonnées GPS", "Represents an immutable geographic point with GPS coordinates"),
    ("Fournit des méthodes d'extension pour la manipulation de chaînes de caractères", "Provides extension methods for string manipulation"),
    ("Sortie : L'adresse e-mail normalisée (minuscules, espaces supprimés) si valide", "Output: The normalized email address (lowercase, spaces removed) if valid"),
    ("<c>true</c> si l'adresse est valide ; <c>false</c> sinon", "<c>true</c> if the address is valid; <c>false</c> otherwise"),
    ("Une adresse <c>null</c> ou vide est considérée comme invalide", "A <c>null</c> or empty address is considered invalid"),
    ("Calcule le coût d'expédition selon le type de produit", "Calculates the shipping cost based on the product type"),
    ("Référentiel pour la gestion des utilisateurs dans la base de données", "Repository for managing users in the database"),
    ("Cette implémentation utilise Entity Framework Core pour l'accès aux données", "This implementation uses Entity Framework Core for data access"),
    ("Recherche des utilisateurs selon plusieurs critères de filtrage", "Searches for users based on multiple filtering criteria"),
    ("Paramètres de pagination (page, taille). Si <c>null</c>, retourne tous les résultats", "Pagination parameters (page, size). If <c>null</c>, returns all results"),
    ("<item><c>Items</c> : Les utilisateurs correspondant aux critères</item>", "<item><c>Items</c>: Users matching the criteria</item>"),
    ('Console.WriteLine($"Trouvé {results.TotalCount} utilisateurs")', 'Console.WriteLine($"Found {results.TotalCount} users")'),
    ("Commande pour créer un nouvel utilisateur dans le système", "Command to create a new user in the system"),
    ("<item>Validation des données (e-mail unique, mot de passe conforme)</item>", "<item>Data validation (unique email, compliant password)</item>"),
    ("<item>Création de l'enregistrement en base</item>", "<item>Creating the database record</item>"),
    ("Gestionnaire de la commande de création d'utilisateur", "Handler for the create user command"),
    ("Traite la commande de création d'un utilisateur", "Processes the command to create a user"),
    ("<param name=\"command\">La commande contenant les données de l'utilisateur à créer.</param>", '<param name="command">The command containing the data of the user to create.</param>'),
    ("<item><c>UserId</c> : L'identifiant unique de l'utilisateur créé</item>", "<item><c>UserId</c>: The created user's unique identifier</item>"),
    ("Levée si les données de la commande ne respectent pas les règles métier", "Thrown if the command data does not comply with business rules"),
    ("Levée si un utilisateur avec cet e-mail existe déjà", "Thrown if a user with this email already exists"),
    ("<item>Cette méthode charge TOUS les produits en mémoire (eager loading)</item>", "<item>This method loads ALL products into memory (eager loading)</item>"),
    ("Pour de grandes quantités de données (> 5000 produits), privilégier :", "For large amounts of data (> 5000 products), prefer:"),
    ("Fonction de traitement appelée pour chaque produit", "Processing function called for each product"),
    ("Authentifie un utilisateur avec ses identifiants", "Authenticates a user with their credentials"),
    ("<param name=\"email\">L'adresse e-mail de l'utilisateur.</param>", '<param name="email">The user\'s email address.</param>'),
    ("Un jeton JWT valide pendant 1 heure si l'authentification réussit", "A valid JWT token for 1 hour if authentication succeeds"),
    ("<item>Valide les URLs dans les liens et images (http/https uniquement)</item>", "<item>Validates URLs in links and images (http/https only)</item>"),
    ("Récupère un utilisateur par son identifiant numérique", "Retrieves a user by their numeric identifier"),
    ("<param name=\"id\">L'identifiant numérique de l'utilisateur.</param>", '<param name="id">The user\'s numeric identifier.</param>'),
    ("⚠️ DÉPRÉCIÉ : Cette méthode sera supprimée dans la version 3.0 (prévue pour juin 2026)", "⚠️ DEPRECATED: This method will be removed in version 3.0 (scheduled for June 2026)"),
    
    # Verb forms in lists
    ("(Calcule, Valide, Enregistre, Recherche)", "(Calculates, Validates, Saves, Searches)"),
]

count = 0
for fr, en in final_translations:
    if fr in content:
        content = content.replace(fr, en)
        count += 1
        print(f"  ✓ {fr[:70]}...")

with codecs.open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print(f"\n✅ Applied {count} final translations")
print(f"✅ File updated: {file_path}")

# Final verification
import re
french_pattern = r'\b(utilisateur|données|méthode|propriété|fonction|retourne|obtient|définit|calcule|valide|enregistre|français)\b'
remaining = set(re.findall(french_pattern, content, re.IGNORECASE))

if remaining:
    print(f"\n⚠️ Found {len(remaining)} unique French words remaining:")
    for word in sorted(remaining, key=str.lower):
        count_word = len(re.findall(r'\b' + re.escape(word) + r'\b', content, re.IGNORECASE))
        print(f"  - {word} ({count_word} occurrences)")
else:
    print("\n🎉 TRANSLATION COMPLETE! No French words detected.")
