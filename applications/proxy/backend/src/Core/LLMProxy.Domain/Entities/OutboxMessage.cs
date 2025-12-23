using LLMProxy.Domain.Common;
using System.Text.Json;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Entité représentant un message dans la table Outbox.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-040 Outbox Pattern.
/// </para>
/// <para>
/// L'Outbox Pattern garantit la publication fiable des événements en les sauvegardant
/// dans la même transaction que les données métier. Un processus en arrière-plan (worker)
/// se charge ensuite de publier ces événements sur le message broker.
/// </para>
/// <para>
/// Cette approche résout le problème du "dual write" où l'on doit sauvegarder les données
/// ET publier des événements de manière atomique.
/// </para>
/// </remarks>
public sealed class OutboxMessage
{
    /// <summary>
    /// Identifiant unique du message.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Type complet de l'événement (AssemblyQualifiedName pour désérialisation).
    /// </summary>
    /// <remarks>
    /// Exemple : "LLMProxy.Domain.Entities.TenantCreatedEvent, LLMProxy.Domain, Version=1.0.0.0"
    /// </remarks>
    public string Type { get; private set; } = string.Empty;

    /// <summary>
    /// Contenu JSON de l'événement sérialisé.
    /// </summary>
    /// <remarks>
    /// Stocké au format JSON pour faciliter le debugging et l'analyse.
    /// Utilise camelCase pour la sérialisation.
    /// </remarks>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Date et heure de création du message (UTC).
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Date et heure de traitement réussi du message (UTC).
    /// Null si le message n'a pas encore été traité.
    /// </summary>
    public DateTimeOffset? ProcessedAt { get; private set; }

    /// <summary>
    /// Message d'erreur lors du dernier échec de traitement.
    /// Null si aucune erreur ou traitement réussi.
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Nombre de tentatives de traitement échouées.
    /// </summary>
    /// <remarks>
    /// Utilisé pour identifier les messages qui doivent être déplacés vers la Dead Letter Queue.
    /// </remarks>
    public int RetryCount { get; private set; }

    /// <summary>
    /// Constructeur privé pour EF Core.
    /// </summary>
    private OutboxMessage() { }

    /// <summary>
    /// Crée un nouveau message Outbox à partir d'un événement de domaine.
    /// </summary>
    /// <typeparam name="TEvent">Type de l'événement de domaine.</typeparam>
    /// <param name="domainEvent">Événement de domaine à sérialiser.</param>
    /// <returns>Instance de OutboxMessage prête à être sauvegardée.</returns>
    /// <exception cref="ArgumentNullException">Si domainEvent est null.</exception>
    /// <exception cref="JsonException">Si la sérialisation échoue.</exception>
    public static OutboxMessage Create<TEvent>(TEvent domainEvent)
        where TEvent : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var eventType = typeof(TEvent);
        var assemblyQualifiedName = eventType.AssemblyQualifiedName
            ?? throw new InvalidOperationException($"Cannot get AssemblyQualifiedName for type {eventType.Name}");

        var json = JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = assemblyQualifiedName,
            Content = json,
            CreatedAt = DateTimeOffset.UtcNow,
            RetryCount = 0
        };
    }

    /// <summary>
    /// Marque le message comme traité avec succès.
    /// </summary>
    /// <remarks>
    /// Efface également l'erreur précédente si elle existait.
    /// </remarks>
    public void MarkAsProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
        Error = null;
    }

    /// <summary>
    /// Marque le message comme ayant échoué lors du traitement.
    /// </summary>
    /// <param name="error">Message d'erreur décrivant l'échec.</param>
    /// <exception cref="ArgumentException">Si error est null ou vide.</exception>
    public void MarkAsFailed(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        Error = error.Length > 2000 ? error[..2000] : error; // Limitation à 2000 caractères
        RetryCount++;
    }

    /// <summary>
    /// Indique si le message a atteint le nombre maximum de tentatives.
    /// </summary>
    /// <param name="maxRetries">Nombre maximum de tentatives autorisées.</param>
    /// <returns>True si le message doit être déplacé vers la Dead Letter Queue.</returns>
    public bool ShouldMoveToDeadLetter(int maxRetries)
    {
        return ProcessedAt == null && RetryCount >= maxRetries;
    }

    /// <summary>
    /// Indique si le message peut être nettoyé (déjà traité et ancien).
    /// </summary>
    /// <param name="retentionPeriod">Période de rétention.</param>
    /// <returns>True si le message peut être supprimé.</returns>
    public bool CanBeDeleted(TimeSpan retentionPeriod)
    {
        return ProcessedAt.HasValue
            && DateTimeOffset.UtcNow - ProcessedAt.Value >= retentionPeriod;
    }
}
