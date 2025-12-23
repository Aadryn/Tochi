using Bogus;
using FluentAssertions;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Common;

namespace LLMProxy.Application.Tests.Entities;

/// <summary>
/// Tests unitaires pour <see cref="OutboxDeadLetter"/>.
/// </summary>
public sealed class OutboxDeadLetterTests
{
    /// <summary>
    /// Générateur de données aléatoires.
    /// </summary>
    private readonly Faker _faker = new();

    /// <summary>
    /// Événement de test pour les scénarios.
    /// </summary>
    private sealed class TestEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public string TestData { get; set; } = string.Empty;
    }

    [Fact]
    public void Constructor_AvecOutboxMessageValide_DoitCopierToutesLesProprietesEtInitialiserResolved()
    {
        // Arrange
        var testEvent = new TestEvent { TestData = _faker.Lorem.Sentence() };
        var outboxMessage = OutboxMessage.Create(testEvent);
        outboxMessage.MarkAsFailed("Erreur de test");
        outboxMessage.MarkAsFailed("Erreur de test 2");

        // Act
        var deadLetter = new OutboxDeadLetter(outboxMessage);

        // Assert
        deadLetter.Id.Should().NotBeEmpty();
        deadLetter.OriginalMessageId.Should().Be(outboxMessage.Id);
        deadLetter.Type.Should().Be(outboxMessage.Type);
        deadLetter.Content.Should().Be(outboxMessage.Content);
        deadLetter.CreatedAt.Should().Be(outboxMessage.CreatedAt);
        deadLetter.Error.Should().Be(outboxMessage.Error);
        deadLetter.RetryCount.Should().Be(2);
        deadLetter.DeadLetteredAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        deadLetter.Resolved.Should().BeFalse();
        deadLetter.ResolvedAt.Should().BeNull();
        deadLetter.ResolutionNotes.Should().BeNull();
    }

    [Fact]
    public void Constructor_AvecOutboxMessageNull_DoitLeverArgumentNullException()
    {
        // Act
        Action act = () => new OutboxDeadLetter(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("failedMessage");
    }

    [Fact]
    public void MarkAsResolved_SansNotes_DoitMarquerCommeResoluAvecDateEtNotesVides()
    {
        // Arrange
        var testEvent = new TestEvent();
        var outboxMessage = OutboxMessage.Create(testEvent);
        outboxMessage.MarkAsFailed("Erreur");
        var deadLetter = new OutboxDeadLetter(outboxMessage);

        // Act
        deadLetter.MarkAsResolved();

        // Assert
        deadLetter.Resolved.Should().BeTrue();
        deadLetter.ResolvedAt.Should().NotBeNull();
        deadLetter.ResolvedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        deadLetter.ResolutionNotes.Should().BeNull();
    }

    [Fact]
    public void MarkAsResolved_AvecNotes_DoitMarquerCommeResoluAvecDateEtNotes()
    {
        // Arrange
        var testEvent = new TestEvent();
        var outboxMessage = OutboxMessage.Create(testEvent);
        outboxMessage.MarkAsFailed("Erreur");
        var deadLetter = new OutboxDeadLetter(outboxMessage);
        var notes = _faker.Lorem.Paragraph();

        // Act
        deadLetter.MarkAsResolved(notes);

        // Assert
        deadLetter.Resolved.Should().BeTrue();
        deadLetter.ResolvedAt.Should().NotBeNull();
        deadLetter.ResolvedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        deadLetter.ResolutionNotes.Should().Be(notes);
    }

    [Fact]
    public async Task MarkAsResolved_AppelesMultiples_DoitMettreAJourResolutionDateAChaqueFois()
    {
        // Arrange
        var testEvent = new TestEvent();
        var outboxMessage = OutboxMessage.Create(testEvent);
        outboxMessage.MarkAsFailed("Erreur");
        var deadLetter = new OutboxDeadLetter(outboxMessage);

        // Act
        deadLetter.MarkAsResolved("Première résolution");
        var firstResolutionDate = deadLetter.ResolvedAt;

        await Task.Delay(100); // Attendre un peu pour avoir une date différente

        deadLetter.MarkAsResolved("Seconde résolution");
        var secondResolutionDate = deadLetter.ResolvedAt;

        // Assert
        deadLetter.Resolved.Should().BeTrue();
        deadLetter.ResolutionNotes.Should().Be("Seconde résolution");
        secondResolutionDate.Should().BeAfter(firstResolutionDate!.Value);
    }
}
