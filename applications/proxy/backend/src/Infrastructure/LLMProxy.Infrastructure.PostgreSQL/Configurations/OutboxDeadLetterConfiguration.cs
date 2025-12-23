using LLMProxy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LLMProxy.Infrastructure.PostgreSQL.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité OutboxDeadLetter.
/// </summary>
/// <remarks>
/// Conforme à l'ADR-040 Outbox Pattern.
/// </remarks>
public sealed class OutboxDeadLetterConfiguration : IEntityTypeConfiguration<OutboxDeadLetter>
{
    public void Configure(EntityTypeBuilder<OutboxDeadLetter> builder)
    {
        builder.ToTable("outbox_dead_letters", "messaging");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalMessageId)
            .IsRequired()
            .HasComment("Identifiant du message Outbox original");

        builder.Property(x => x.Type)
            .HasMaxLength(500)
            .IsRequired()
            .HasComment("Type complet de l'événement");

        builder.Property(x => x.Content)
            .HasColumnType("jsonb")
            .IsRequired()
            .HasComment("Contenu JSON de l'événement");

        builder.Property(x => x.Error)
            .HasMaxLength(2000)
            .HasComment("Dernier message d'erreur");

        builder.Property(x => x.RetryCount)
            .IsRequired()
            .HasComment("Nombre de tentatives avant Dead Letter");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasComment("Date de création du message original (UTC)");

        builder.Property(x => x.DeadLetteredAt)
            .IsRequired()
            .HasComment("Date de déplacement vers Dead Letter (UTC)");

        builder.Property(x => x.Resolved)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indique si le message a été résolu manuellement");

        builder.Property(x => x.ResolvedAt)
            .HasComment("Date de résolution manuelle (UTC)");

        builder.Property(x => x.ResolutionNotes)
            .HasMaxLength(4000)
            .HasComment("Notes de résolution (investigation, actions)");

        // Index sur OriginalMessageId pour tracer l'historique
        builder.HasIndex(x => x.OriginalMessageId)
            .HasDatabaseName("IX_outbox_dead_letters_original_message_id");

        // Index sur les messages non résolus (pour monitoring)
        builder.HasIndex(x => x.Resolved)
            .HasFilter("resolved = false")
            .HasDatabaseName("IX_outbox_dead_letters_unresolved")
            .HasAnnotation("Description", "Index pour lister les Dead Letters non résolues");

        // Index sur DeadLetteredAt pour analyse temporelle
        builder.HasIndex(x => x.DeadLetteredAt)
            .HasDatabaseName("IX_outbox_dead_letters_dead_lettered_at");
    }
}
