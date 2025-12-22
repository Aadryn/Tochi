using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Entité représentant un message Outbox qui a échoué de manière définitive (Dead Letter).
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-040 Outbox Pattern.
/// </para>
/// <para>
/// Les messages qui ont dépassé le nombre maximum de tentatives de traitement sont déplacés
/// dans cette table pour investigation manuelle. Cela évite de bloquer le processeur Outbox
/// avec des messages corrompus ou impossibles à traiter.
/// </para>
/// </remarks>
public sealed class OutboxDeadLetter
{
    /// <summary>
    /// Identifiant unique du message Dead Letter.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Identifiant du message Outbox original.
    /// </summary>
    public Guid OriginalMessageId { get; init; }

    /// <summary>
    /// Type complet de l'événement.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Contenu JSON de l'événement.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Dernier message d'erreur avant le déplacement vers Dead Letter.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Nombre de tentatives avant déplacement vers Dead Letter.
    /// </summary>
    public int RetryCount { get; init; }

    /// <summary>
    /// Date de création du message Outbox original (UTC).
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Date de déplacement vers Dead Letter (UTC).
    /// </summary>
    public DateTimeOffset DeadLetteredAt { get; init; }

    /// <summary>
    /// Indique si le message a été retraité manuellement avec succès.
    /// </summary>
    public bool Resolved { get; private set; }

    /// <summary>
    /// Date de résolution manuelle (UTC).
    /// </summary>
    public DateTimeOffset? ResolvedAt { get; private set; }

    /// <summary>
    /// Notes de résolution (investigation, actions prises).
    /// </summary>
    public string? ResolutionNotes { get; private set; }

    /// <summary>
    /// Initialise une nouvelle instance Dead Letter à partir d'un message Outbox échoué.
    /// </summary>
    /// <param name="failedMessage">Message Outbox qui a échoué définitivement.</param>
    /// <exception cref="ArgumentNullException">Si <paramref name="failedMessage"/> est null.</exception>
    public OutboxDeadLetter(OutboxMessage failedMessage)
    {
        ArgumentNullException.ThrowIfNull(failedMessage);

        Id = Guid.NewGuid();
        OriginalMessageId = failedMessage.Id;
        Type = failedMessage.Type;
        Content = failedMessage.Content;
        Error = failedMessage.Error;
        RetryCount = failedMessage.RetryCount;
        CreatedAt = failedMessage.CreatedAt;
        DeadLetteredAt = DateTimeOffset.UtcNow;
        Resolved = false;
        ResolvedAt = null;
        ResolutionNotes = null;
    }

    /// <summary>
    /// Marque le message Dead Letter comme résolu.
    /// </summary>
    /// <param name="resolutionNotes">Notes décrivant la résolution.</param>
    public void MarkAsResolved(string? resolutionNotes = null)
    {
        Resolved = true;
        ResolvedAt = DateTimeOffset.UtcNow;
        ResolutionNotes = resolutionNotes;
    }
}
