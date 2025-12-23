#!/usr/bin/env python3
"""
Script to translate the C# documentation instructions file from French to American English.
"""

import re
import codecs

# File path
file_path = "/workspaces/proxy/.github/instructions/csharp.documentation.instructions.md"

# Read the file with UTF-8 encoding (no BOM)
with codecs.open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Dictionary of translations (French -> English)
translations = {
    # Section headings
    "Documentation des Valeurs Spéciales": "Special Values Documentation",
    "Documentation des Comportements Asynchrones": "Asynchronous Behaviors Documentation",
    "Documentation des Interfaces": "Interfaces Documentation",
    "Anti-Patterns à Éviter": "Anti-Patterns to Avoid",
    "Documentation Redondante avec le Code": "Code-Redundant Documentation",
    "Documentation Obsolète": "Obsolete Documentation",
    "Documentation Trop Technique": "Overly Technical Documentation",
    "Documentation Vague": "Vague Documentation",
    "Checklist de Validation": "Validation Checklist",
    "Complétude": "Completeness",
    "Qualité": "Quality",
    "Contenu": "Content",
    "Format": "Format",
    "Cas Spéciaux et Patterns Avancés": "Special Cases and Advanced Patterns",
    "Records et Types Immutables": "Records and Immutable Types",
    "Extension Methods": "Extension Methods",
    "Generic Constraints": "Generic Constraints",
    "Nullable Reference Types": "Nullable Reference Types",
    "Operators Overloading": "Operator Overloading",
    "Pattern Matching et Switch Expressions": "Pattern Matching and Switch Expressions",
    "Scénarios Métier Complexes": "Complex Business Scenarios",
    "Repository Pattern": "Repository Pattern",
    "Command/Query Handlers (CQRS)": "Command/Query Handlers (CQRS)",
    "Domain Events": "Domain Events",
    "Performance et Optimisation": "Performance and Optimization",
    "Documentation des Considérations de Performance": "Performance Considerations Documentation",
    "Caching et Mémoization": "Caching and Memoization",
    "Sécurité et Validation": "Security and Validation",
    "Documentation des Contraintes de Sécurité": "Security Constraints Documentation",
    "Validation et Sanitization": "Validation and Sanitization",
    "Gestion d'Erreurs et Résilience": "Error Handling and Resilience",
    "Retry et Circuit Breaker": "Retry and Circuit Breaker",
    "Migration et Dépréciation": "Migration and Deprecation",
    "Documentation de Code Déprécié": "Deprecated Code Documentation",
    "Ressources et Références": "Resources and References",
    "Documentation Officielle Microsoft": "Microsoft Official Documentation",
    "Standards": "Standards",
    
    # Common phrases  
    "**TOUJOURS préciser :**": "**ALWAYS specify:**",
    "**À FAIRE :**": "**DO:**",
    "**À ÉVITER :**": "**AVOID:**",
    "Action effectuée (verbe d'action à l'infinitif ou 3ᵉ personne)": "Action performed (action verb in infinitive or 3rd person)",
    "Description de CHAQUE paramètre": "Description of EACH parameter",
    "Description précise de la valeur retournée": "Precise description of the returned value",
    "TOUTES les exceptions pouvant être levées": "ALL exceptions that can be thrown",
    "Ce que représente la propriété (pas \"obtient ou définit\")": "What the property represents (not \"gets or sets\")",
    "Type de valeur et contraintes éventuelles": "Value type and possible constraints",
    "Comportements spéciaux (lecture seule, calcul, validation)": "Special behaviors (read-only, calculated, validation)",
    "Quand l'événement est déclenché": "When the event is triggered",
    "Informations sur les gestionnaires et le contexte": "Information about handlers and context",
    "Ce que représente l'énumération": "What the enumeration represents",
    "Signification précise de la valeur": "Precise meaning of the value",
    
    # Code comment translations
    "Représente un service de gestion des notifications par e-mail.": "Represents an email notification management service.",
    "Ce service utilise un système de file d'attente pour envoyer les e-mails de manière asynchrone.": "This service uses a queue system to send emails asynchronously.",
    "Les e-mails échoués sont automatiquement réessayés jusqu'à 3 fois.": "Failed emails are automatically retried up to 3 times.",
    "Recherche un utilisateur par son adresse e-mail dans la base de données.": "Searches for a user by their email address in the database.",
    "L'adresse e-mail à rechercher (insensible à la casse).": "The email address to search for (case-insensitive).",
    "Jeton d'annulation pour interrompre l'opération si nécessaire.": "Cancellation token to interrupt the operation if needed.",
    "L'utilisateur correspondant à l'adresse e-mail, ou <c>null</c> si aucun utilisateur n'est trouvé.": "The user corresponding to the email address, or <c>null</c> if no user is found.",
    "Levée si <paramref name=\"email\"/> est <c>null</c> ou vide.": "Thrown when <paramref name=\"email\"/> is <c>null</c> or empty.",
    "Levée en cas d'erreur de connexion ou de requête à la base de données.": "Thrown in case of connection or database query error.",
    "Identifiant unique de l'entité, généré automatiquement à la création.": "Unique identifier of the entity, automatically generated upon creation.",
    "Un GUID unique attribué lors de l'instanciation de l'objet.": "A unique GUID assigned upon object instantiation.",
    "Cette propriété est en lecture seule après l'initialisation.": "This property is read-only after initialization.",
    "Âge de l'utilisateur calculé à partir de sa date de naissance.": "User's age calculated from their birth date.",
    "L'âge en années complètes, ou <c>null</c> si la date de naissance n'est pas définie.": "The age in complete years, or <c>null</c> if the birth date is not defined.",
    "Cette propriété est recalculée à chaque accès en fonction de la date actuelle.": "This property is recalculated on each access based on the current date.",
    "Déclenché lorsque la valeur de la propriété change.": "Triggered when the property value changes.",
    "Cet événement est levé APRÈS la modification de la valeur.": "This event is raised AFTER the value modification.",
    "Les gestionnaires reçoivent l'ancienne et la nouvelle valeur.": "Handlers receive the old and new value.",
    "Définit les différents niveaux de priorité pour les tâches.": "Defines the different priority levels for tasks.",
    "Priorité basse : la tâche peut être traitée ultérieurement.": "Low priority: the task can be processed later.",
    "Priorité normale : la tâche doit être traitée dans les délais standard.": "Normal priority: the task must be processed within standard timeframes.",
    "Priorité haute : la tâche nécessite un traitement rapide.": "High priority: the task requires fast processing.",
    "Priorité critique : la tâche doit être traitée immédiatement.": "Critical priority: the task must be processed immediately.",
}

# Apply translations
print("Applying translations...")
count = 0
for french, english in translations.items():
    if french in content:
        content = content.replace(french, english)
        count += 1
        print(f"  ✓ Translated: {french[:50]}...")

print(f"\n✅ Applied {count} translations")

# Write back the file with UTF-8 encoding (no BOM)
with codecs.open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print(f"✅ File updated: {file_path}")
