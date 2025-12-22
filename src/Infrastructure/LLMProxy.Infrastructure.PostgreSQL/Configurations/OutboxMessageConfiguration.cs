using LLMProxy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LLMProxy.Infrastructure.PostgreSQL.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité OutboxMessage.
/// </summary>
/// <remarks>
/// Conforme à l'ADR-040 Outbox Pattern.
/// </remarks>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "messaging");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasMaxLength(500)
            .IsRequired()
            .HasComment("Type complet de l'événement (AssemblyQualifiedName)");

        builder.Property(x => x.Content)
            .HasColumnType("jsonb")
            .IsRequired()
            .HasComment("Contenu JSON de l'événement sérialisé");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasComment("Date de création du message (UTC)");

        builder.Property(x => x.ProcessedAt)
            .HasComment("Date de traitement réussi (NULL si non traité)");

        builder.Property(x => x.Error)
            .HasMaxLength(2000)
            .HasComment("Message d'erreur du dernier échec");

        builder.Property(x => x.RetryCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Nombre de tentatives de traitement échouées");

        // Index pour le traitement (messages non traités)
        // Partial index : seulement les messages non traités (ProcessedAt IS NULL)
        builder.HasIndex(x => x.ProcessedAt)
            .HasFilter("processed_at IS NULL")
            .HasDatabaseName("IX_outbox_messages_unprocessed")
            .HasAnnotation("Description", "Index pour récupérer les messages non traités (OutboxProcessor)");

        // Index pour le cleanup (messages traités)
        // Partial index : seulement les messages traités (ProcessedAt IS NOT NULL)
        builder.HasIndex(x => new { x.ProcessedAt, x.CreatedAt })
            .HasFilter("processed_at IS NOT NULL")
            .HasDatabaseName("IX_outbox_messages_cleanup")
            .HasAnnotation("Description", "Index pour le nettoyage des messages traités (OutboxCleanupService)");

        // Index pour identifier les messages en échec
        builder.HasIndex(x => new { x.RetryCount, x.ProcessedAt })
            .HasFilter("processed_at IS NULL")
            .HasDatabaseName("IX_outbox_messages_failed")
            .HasAnnotation("Description", "Index pour identifier les messages en échec (OutboxDeadLetterService)");
    }
}
