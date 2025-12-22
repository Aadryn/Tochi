using Bogus;
using FluentAssertions;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Common;

namespace LLMProxy.Application.Tests.Entities;

/// <summary>
/// Tests unitaires pour <see cref="OutboxMessage"/>.
/// </summary>
public sealed class OutboxMessageTests
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
    public void Create_AvecEvenementValide_DoitCreerOutboxMessageAvecProprietesCorrectes()
    {
        // Arrange
        var testEvent = new TestEvent { TestData = _faker.Lorem.Sentence() };

        // Act
        var outboxMessage = OutboxMessage.Create(testEvent);

        // Assert
        outboxMessage.Should().NotBeNull();
        outboxMessage.Id.Should().NotBeEmpty();
        outboxMessage.Type.Should().Contain(nameof(TestEvent));
        outboxMessage.Content.Should().Contain(testEvent.TestData);
        outboxMessage.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        outboxMessage.ProcessedAt.Should().BeNull();
        outboxMessage.Error.Should().BeNull();
        outboxMessage.RetryCount.Should().Be(0);
    }

    [Fact]
    public void Create_AvecEvenementNull_DoitLeverArgumentNullException()
    {
        // Act
        Action act = () => OutboxMessage.Create<TestEvent>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("domainEvent");
    }

    [Fact]
    public void MarkAsProcessed_DoitDefinirProcessedAtEtViderError()
    {
        // Arrange
        var testEvent = new TestEvent();
        var outboxMessage = OutboxMessage.Create(testEvent);
        outboxMessage.MarkAsFailed("Erreur de test");

        // Act
        outboxMessage.MarkAsProcessed();

        // Assert
        outboxMessage.ProcessedAt.Should().NotBeNull();
        outboxMessage.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        outboxMessage.Error.Should().BeNull();
    }

    [Fact]
    public void MarkAsFailed_AvecMessageErreur_DoitIncrementerRetryCountEtStockerErreur()
    {
        // Arrange
        var testEvent = new TestEvent();
        var outboxMessage = OutboxMessage.Create(testEvent);
        var errorMessage = _faker.Lorem.Sentence();

        // Act
        outboxMessage.MarkAsFailed(errorMessage);

        // Assert
        outboxMessage.RetryCount.Should().Be(1);
        outboxMessage.Error.Should().Be(errorMessage);
        outboxMessage.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public void MarkAsFailed_AvecMessageErreurTresLong_DoitTronquerA2000Caracteres()
    {
        // Arrange
        var testEvent = new TestEvent();
        var outboxMessage = OutboxMessage.Create(testEvent);
        var longError = new string('X', 3000);

        // Act
        outboxMessage.MarkAsFailed(longError);

        // Assert
        outboxMessage.Error.Should().HaveLength(2000);
        outboxMessage.Error.Should().StartWith("XXX");
    }

    [Fact]
    public void MarkAsFailed_AppelesMultiples_DoitIncrementerRetryCountAChaqueFois()
    {
        // Arrange
        var testEvent = new TestEvent();
        var outboxMessage = OutboxMessage.Create(testEvent);

        // Act
        outboxMessage.MarkAsFailed("Erreur 1");
        outboxMessage.MarkAsFailed("Erreur 2");
        outboxMessage.MarkAsFailed("Erreur 3");

        // Assert
        outboxMessage.RetryCount.Should().Be(3);
        outboxMessage.Error.Should().Be("Erreur 3");
    }

    [Fact]
    public void ShouldMoveToDeadLetter_QuandRetryCountInferieurAuMax_DoitRetournerFalse()
    {
        // Arrange
        var testEvent = new TestEvent();
        var outboxMessage = OutboxMessage.Create(testEvent);
        outboxMessage.MarkAsFailed("Erreur");
        outboxMessage.MarkAsFailed("Erreur");

        // Act
        var result = outboxMessage.ShouldMoveToDeadLetter(maxRetries: 3);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldMoveToDeadLetter_QuandRetryCountEgalAuMax_DoitRetournerTrue()
    {
        // Arrange
        var testEvent = new TestEvent();
        var outboxMessage = OutboxMessage.Create(testEvent);
        outboxMessage.MarkAsFailed("Erreur");
        outboxMessage.MarkAsFailed("Erreur");
        outboxMessage.MarkAsFailed("Erreur");

        // Act
        var result = outboxMessage.ShouldMoveToDeadLetter(maxRetries: 3);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanBeDeleted_QuandMessageNonTraiteEtProcessedAtNull_DoitRetournerFalse()
    {
        // Arrange
        var testEvent = new TestEvent();
        var outboxMessage = OutboxMessage.Create(testEvent);

        // Act
        var result = outboxMessage.CanBeDeleted(retentionPeriod: TimeSpan.FromDays(7));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanBeDeleted_QuandMessageTraiteRecentApresRetention_DoitRetournerFalse()
    {
        // Arrange
        var testEvent = new TestEvent();
        var outboxMessage = OutboxMessage.Create(testEvent);
        outboxMessage.MarkAsProcessed();

        // Act
        var result = outboxMessage.CanBeDeleted(retentionPeriod: TimeSpan.FromDays(7));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanBeDeleted_QuandMessageTraiteAvantRetention_DoitRetournerTrue()
    {
        // Arrange
        var testEvent = new TestEvent();
        var outboxMessage = OutboxMessage.Create(testEvent);
        
        // Simuler un traitement ancien (via reflection car propriété privée)
        var processedAt = DateTimeOffset.UtcNow.AddDays(-10);
        typeof(OutboxMessage)
            .GetProperty(nameof(OutboxMessage.ProcessedAt))!
            .SetValue(outboxMessage, processedAt);

        // Act
        var result = outboxMessage.CanBeDeleted(retentionPeriod: TimeSpan.FromDays(7));

        // Assert
        result.Should().BeTrue();
    }
}
