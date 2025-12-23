#!/usr/bin/env pwsh
# Script pour traduire le fichier csharp.documentation.instructions.md en anglais américain

$filePath = "/workspaces/proxy/.github/instructions/csharp.documentation.instructions.md"
$utf8NoBom = New-Object System.Text.UTF8Encoding $false

Write-Host "Reading file..." -ForegroundColor Cyan
$content = [System.IO.File]::ReadAllText($filePath, $utf8NoBom)

# Traductions françaises vers anglais
$translations = @{
    # Titres de sections
    "### Méthodes et Fonctions" = "### Methods and Functions"
    "### Propriétés" = "### Properties"
    "### Événements" = "### Events"
    "### Enums" = "### Enums"
    
    # Texte descriptif
    "**TOUJOURS documenter :**" = "**ALWAYS document:**"
    "Action effectuée" = "Action performed"
    "verbe d'action à l'infinitif ou 3ᵉ personne" = "action verb in infinitive or 3rd person"
    "Description de CHAQUE paramètre" = "Description of EACH parameter"
    "Description précise de la valeur retournée" = "Precise description of the returned value"
    "TOUTES les exceptions pouvant être levées" = "ALL exceptions that can be thrown"
    "Ce que représente la propriété" = "What the property represents"
    "pas ""obtient ou définit""" = "not ""gets or sets"""
    "Type de valeur et contraintes éventuelles" = "Value type and possible constraints"
    "Comportements spéciaux" = "Special behaviors"
    "lecture seule, calcul, validation" = "read-only, calculated, validation"
    "Quand l'événement est déclenché" = "When the event is triggered"
    "Informations sur les gestionnaires et le contexte" = "Information about handlers and context"
    "Ce que représente l'énumération" = "What the enumeration represents"
    "Signification précise de la valeur" = "Precise meaning of the value"
    
    # Commentaires dans le code
    "Représente un service de gestion des notifications par e-mail" = "Represents an email notification management service"
    "Ce service utilise un système de file d'attente pour envoyer les e-mails de manière asynchrone" = "This service uses a queue system to send emails asynchronously"
    "Les e-mails échoués sont automatiquement réessayés jusqu'à 3 fois" = "Failed emails are automatically retried up to 3 times"
    "Recherche un utilisateur par son adresse e-mail dans la base de données" = "Searches for a user by their email address in the database"
    "L'adresse e-mail à rechercher" = "The email address to search for"
    "insensible à la casse" = "case-insensitive"
    "Jeton d'annulation pour interrompre l'opération si nécessaire" = "Cancellation token to interrupt the operation if needed"
    "L'utilisateur correspondant à l'adresse e-mail, ou" = "The user corresponding to the email address, or"
    "si aucun utilisateur n'est trouvé" = "if no user is found"
    "Levée si" = "Thrown when"
    "est" = "is"
    "ou vide" = "or empty"
    "Levée en cas d'erreur de connexion ou de requête à la base de données" = "Thrown in case of connection or database query error"
    "Identifiant unique de l'entité, généré automatiquement à la création" = "Unique identifier of the entity, automatically generated upon creation"
    "Un GUID unique attribué lors de l'instanciation de l'objet" = "A unique GUID assigned upon object instantiation"
    "Cette propriété est en lecture seule après l'initialisation" = "This property is read-only after initialization"
    "Âge de l'utilisateur calculé à partir de sa date de naissance" = "User's age calculated from their birth date"
    "L'âge en années complètes, ou" = "The age in complete years, or"
    "si la date de naissance n'est pas définie" = "if the birth date is not defined"
    "Cette propriété est recalculée à chaque accès en fonction de la date actuelle" = "This property is recalculated on each access based on the current date"
    "Déclenché lorsque la valeur de la propriété change" = "Triggered when the property value changes"
    "Cet événement est levé APRÈS la modification de la valeur" = "This event is raised AFTER the value modification"
    "Les gestionnaires reçoivent l'ancienne et la nouvelle valeur" = "Handlers receive the old and new value"
    "Définit les différents niveaux de priorité pour les tâches" = "Defines the different priority levels for tasks"
    "Priorité basse" = "Low priority"
    "la tâche peut être traitée ultérieurement" = "the task can be processed later"
    "Priorité normale" = "Normal priority"
    "la tâche doit être traitée dans les délais standard" = "the task must be processed within standard timeframes"
    "Priorité haute" = "High priority"
    "la tâche nécessite un traitement rapide" = "the task requires fast processing"
    "Priorité critique" = "Critical priority"
    "la tâche doit être traitée immédiatement" = "the task must be processed immediately"
}

Write-Host "Applying translations..." -ForegroundColor Cyan
foreach ($key in $translations.Keys) {
    if ($content -match [regex]::Escape($key)) {
        $content = $content -replace [regex]::Escape($key), $translations[$key]
        Write-Host "  ✓ Translated: $key" -ForegroundColor Green
    }
}

Write-Host "Writing file..." -ForegroundColor Cyan
[System.IO.File]::WriteAllText($filePath, $content, $utf8NoBom)

Write-Host "✅ Translation completed!" -ForegroundColor Green
