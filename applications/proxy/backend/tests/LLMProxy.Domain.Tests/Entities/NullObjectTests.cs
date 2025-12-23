using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Infrastructure.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;

namespace LLMProxy.Domain.Tests.Entities;

/// <summary>
/// Tests unitaires pour les Null Objects.
/// Conforme à ADR-026 (Null Object Pattern).
/// </summary>
public sealed class NullObjectTests
{
    [Fact]
    public void NullTenant_ShouldHaveSingletonInstance()
    {
        // ARRANGE & ACT
        var instance1 = NullTenant.Instance;
        var instance2 = NullTenant.Instance;

        // ASSERT
        Assert.Same(instance1, instance2);  // Même instance (Singleton)
    }

    [Fact]
    public void NullTenant_ShouldAlwaysBeInactive()
    {
        // ARRANGE
        var tenant = NullTenant.Instance;

        // ACT & ASSERT
        Assert.False(tenant.IsActive);
    }

    [Fact]
    public void NullTenant_ShouldHaveDefaultProperties()
    {
        // ARRANGE
        var tenant = NullTenant.Instance;

        // ACT & ASSERT
        Assert.Equal(Guid.Empty, tenant.Id);
        Assert.Equal("Default Tenant", tenant.Name);
        Assert.Equal("default", tenant.Slug);
    }

    [Fact]
    public void UnlimitedQuotaLimit_ShouldHaveSingletonInstance()
    {
        // ARRANGE & ACT
        var instance1 = UnlimitedQuotaLimit.Instance;
        var instance2 = UnlimitedQuotaLimit.Instance;

        // ASSERT
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void UnlimitedQuotaLimit_RecordUsage_ShouldAlwaysSucceed()
    {
        // ARRANGE
        var quota = UnlimitedQuotaLimit.Instance;
        var transactionId = Guid.NewGuid();

        // ACT
        var result = quota.RecordUsage(transactionId, 1_000_000);

        // ASSERT
        Assert.True(result.IsSuccess);
        Assert.Equal(0L, result.Value);  // Aucun quota consommé (illimité)
    }

    [Fact]
    public void UnlimitedQuotaLimit_ShouldAcceptMultipleTransactions()
    {
        // ARRANGE
        var quota = UnlimitedQuotaLimit.Instance;
        var transaction1 = Guid.NewGuid();
        var transaction2 = Guid.NewGuid();

        // ACT
        var result1 = quota.RecordUsage(transaction1, 1_000_000);
        var result2 = quota.RecordUsage(transaction2, 2_000_000);

        // ASSERT
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.Equal(0L, result1.Value);
        Assert.Equal(0L, result2.Value);
    }

    [Fact]
    public void UnlimitedQuotaLimit_ShouldHaveMaxValueLimit()
    {
        // ARRANGE
        var quota = UnlimitedQuotaLimit.Instance;

        // ACT & ASSERT
        Assert.Equal(long.MaxValue, quota.Limit);
        Assert.Equal(QuotaType.TokensPerMonth, quota.QuotaType);
        Assert.Equal(QuotaPeriod.Month, quota.Period);
    }

    [Fact]
    public async Task NullCache_GetAsync_ShouldAlwaysReturnNull()
    {
        // ARRANGE
        var cache = NullCache.Instance;

        // ACT
        var result = await cache.GetAsync("any-key");

        // ASSERT
        Assert.Null(result);  // Cache miss toujours
    }

    [Fact]
    public async Task NullCache_SetAsync_ShouldDoNothing()
    {
        // ARRANGE
        var cache = NullCache.Instance;
        var value = new byte[] { 1, 2, 3 };

        // ACT & ASSERT (pas d'exception)
        await cache.SetAsync("key", value, new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions());

        // Vérifier qu'aucune valeur n'est stockée
        var retrieved = await cache.GetAsync("key");
        Assert.Null(retrieved);
    }

    [Fact]
    public void NullCache_Get_ShouldAlwaysReturnNull()
    {
        // ARRANGE
        var cache = NullCache.Instance;

        // ACT
        var result = cache.Get("any-key");

        // ASSERT
        Assert.Null(result);
    }

    [Fact]
    public void NullCache_Set_ShouldDoNothing()
    {
        // ARRANGE
        var cache = NullCache.Instance;
        var value = new byte[] { 1, 2, 3 };

        // ACT & ASSERT (pas d'exception)
        cache.Set("key", value, new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions());

        // Vérifier qu'aucune valeur n'est stockée
        var retrieved = cache.Get("key");
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task NullCache_RemoveAsync_ShouldDoNothing()
    {
        // ARRANGE
        var cache = NullCache.Instance;

        // ACT & ASSERT (pas d'exception)
        await cache.RemoveAsync("any-key");
    }

    [Fact]
    public async Task NullCache_RefreshAsync_ShouldDoNothing()
    {
        // ARRANGE
        var cache = NullCache.Instance;

        // ACT & ASSERT (pas d'exception)
        await cache.RefreshAsync("any-key");
    }
}
