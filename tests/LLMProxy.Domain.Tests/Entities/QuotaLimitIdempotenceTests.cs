using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using Xunit;

namespace LLMProxy.Domain.Tests.Entities;

/// <summary>
/// Tests pour l'idempotence des opérations QuotaLimit.
/// Conforme à ADR-022 (Idempotence).
/// </summary>
public sealed class QuotaLimitIdempotenceTests
{
    [Fact]
    public void RecordUsage_FirstCall_ReturnsTokensAdded()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var quotaResult = QuotaLimit.Create(userId, tenantId, QuotaType.TokensPerHour, 1000, QuotaPeriod.Hour);
        var quota = quotaResult.Value;
        var transactionId = Guid.NewGuid();

        // Act
        var result = quota.RecordUsage(transactionId, 100);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value); // 100 tokens effectivement ajoutés
    }

    [Fact]
    public void RecordUsage_SameTransactionIdTwice_IsIdempotent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var quotaResult = QuotaLimit.Create(userId, tenantId, QuotaType.TokensPerHour, 1000, QuotaPeriod.Hour);
        var quota = quotaResult.Value;
        var transactionId = Guid.NewGuid();

        // Act - Première application
        var firstResult = quota.RecordUsage(transactionId, 100);

        // Act - Deuxième application (même transaction ID)
        var secondResult = quota.RecordUsage(transactionId, 100);

        // Assert - Premier appel ajoute 100 tokens
        Assert.True(firstResult.IsSuccess);
        Assert.Equal(100, firstResult.Value);

        // Assert - Deuxième appel retourne 0 (idempotence)
        Assert.True(secondResult.IsSuccess);
        Assert.Equal(0, secondResult.Value); // Aucun token ajouté (déjà traité)
    }

    [Fact]
    public void RecordUsage_DifferentTransactionIds_AccumulatesTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var quotaResult = QuotaLimit.Create(userId, tenantId, QuotaType.TokensPerHour, 1000, QuotaPeriod.Hour);
        var quota = quotaResult.Value;

        // Act - Deux transactions différentes
        var result1 = quota.RecordUsage(Guid.NewGuid(), 100);
        var result2 = quota.RecordUsage(Guid.NewGuid(), 50);

        // Assert - Les deux ajoutent des tokens
        Assert.True(result1.IsSuccess);
        Assert.Equal(100, result1.Value);
        
        Assert.True(result2.IsSuccess);
        Assert.Equal(50, result2.Value);
    }

    [Fact]
    public void RecordUsage_EmptyTransactionId_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var quotaResult = QuotaLimit.Create(userId, tenantId, QuotaType.TokensPerHour, 1000, QuotaPeriod.Hour);
        var quota = quotaResult.Value;

        // Act
        var result = quota.RecordUsage(Guid.Empty, 100);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Transaction ID cannot be empty", result.Error);
    }

    [Fact]
    public void RecordUsage_ZeroTokens_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var quotaResult = QuotaLimit.Create(userId, tenantId, QuotaType.TokensPerHour, 1000, QuotaPeriod.Hour);
        var quota = quotaResult.Value;
        var transactionId = Guid.NewGuid();

        // Act
        var result = quota.RecordUsage(transactionId, 0);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Token amount must be positive", result.Error);
    }

    [Fact]
    public void RecordUsage_NegativeTokens_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var quotaResult = QuotaLimit.Create(userId, tenantId, QuotaType.TokensPerHour, 1000, QuotaPeriod.Hour);
        var quota = quotaResult.Value;
        var transactionId = Guid.NewGuid();

        // Act
        var result = quota.RecordUsage(transactionId, -50);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Token amount must be positive", result.Error);
    }

    [Fact]
    public void RecordUsage_MultipleReplaysSameTransaction_AlwaysReturnsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var quotaResult = QuotaLimit.Create(userId, tenantId, QuotaType.TokensPerHour, 1000, QuotaPeriod.Hour);
        var quota = quotaResult.Value;
        var transactionId = Guid.NewGuid();

        // Act - Application initiale
        quota.RecordUsage(transactionId, 100);

        // Act - Replays multiples (simule retries réseau)
        var replay1 = quota.RecordUsage(transactionId, 100);
        var replay2 = quota.RecordUsage(transactionId, 100);
        var replay3 = quota.RecordUsage(transactionId, 100);

        // Assert - Tous les replays retournent 0 (idempotence)
        Assert.Equal(0, replay1.Value);
        Assert.Equal(0, replay2.Value);
        Assert.Equal(0, replay3.Value);
    }
}
