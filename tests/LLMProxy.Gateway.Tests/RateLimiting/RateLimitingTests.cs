using NFluent;
using LLMProxy.Gateway.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using Xunit;

namespace LLMProxy.Gateway.Tests.RateLimiting;

/// <summary>
/// Tests pour la configuration et fonctionnement du Rate Limiting.
/// Conforme à ADR-041 (Rate Limiting et Throttling).
/// </summary>
public sealed class RateLimitingTests
{
    [Fact]
    public void RateLimitingOptions_Should_BeValid_WithDefaultConfiguration()
    {
        // Arrange
        var options = new RateLimitingOptions
        {
            Global = new FixedWindowOptions 
            { 
                PermitLimit = 10000, 
                Window = TimeSpan.FromMinutes(1) 
            },
            PerTenant = new FixedWindowOptions 
            { 
                PermitLimit = 1000, 
                Window = TimeSpan.FromHours(1) 
            },
            PerUser = new SlidingWindowOptions 
            { 
                PermitLimit = 100, 
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6
            },
            PerIp = new FixedWindowOptions 
            { 
                PermitLimit = 50, 
                Window = TimeSpan.FromMinutes(1) 
            },
            Concurrency = new ConcurrencyOptions 
            { 
                PermitLimit = 500, 
                QueueLimit = 0 
            }
        };

        // Assert
        Check.That(options.Global.PermitLimit).IsEqualTo(10000);
        Check.That(options.Global.Window).IsEqualTo(TimeSpan.FromMinutes(1));
        Check.That(options.PerTenant.PermitLimit).IsEqualTo(1000);
        Check.That(options.PerTenant.Window).IsEqualTo(TimeSpan.FromHours(1));
        Check.That(options.PerUser.PermitLimit).IsEqualTo(100);
        Check.That(options.PerUser.Window).IsEqualTo(TimeSpan.FromMinutes(1));
        Check.That(options.PerUser.SegmentsPerWindow).IsEqualTo(6);
        Check.That(options.PerIp.PermitLimit).IsEqualTo(50);
        Check.That(options.Concurrency.PermitLimit).IsEqualTo(500);
    }

    [Fact]
    public async Task FixedWindowLimiter_Should_AllowRequestsUnderLimit()
    {
        // Arrange
        var options = new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        };

        using var limiter = new FixedWindowRateLimiter(options);

        // Act - Acquérir 10 leases (sous la limite)
        var results = new List<bool>();
        for (int i = 0; i < 10; i++)
        {
            using var lease = await limiter.AcquireAsync();
            results.Add(lease.IsAcquired);
        }

        // Assert
        Check.That(results).ContainsOnlyElementsThatMatch(r => r == true);
    }

    [Fact]
    public async Task FixedWindowLimiter_Should_RejectRequestsOverLimit()
    {
        // Arrange
        var options = new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        };

        using var limiter = new FixedWindowRateLimiter(options);

        // Act - Tenter 10 requêtes (limite = 5)
        var results = new List<bool>();
        for (int i = 0; i < 10; i++)
        {
            using var lease = await limiter.AcquireAsync();
            results.Add(lease.IsAcquired);
        }

        // Assert
        Check.That(results.Take(5)).ContainsOnlyElementsThatMatch(r => r == true);
        Check.That(results.Skip(5)).ContainsOnlyElementsThatMatch(r => r == false);
    }

    [Fact]
    public async Task SlidingWindowLimiter_Should_SmoothTraffic()
    {
        // Arrange
        var options = new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromSeconds(10),
            SegmentsPerWindow = 5, // Fenêtre divisée en 5 segments de 2s
            QueueLimit = 0
        };

        using var limiter = new SlidingWindowRateLimiter(options);

        // Act - Acquérir plusieurs leases rapidement
        var results = new List<bool>();
        for (int i = 0; i < 10; i++)
        {
            using var lease = await limiter.AcquireAsync();
            results.Add(lease.IsAcquired);
        }

        // Assert
        Check.That(results.Take(10)).ContainsOnlyElementsThatMatch(r => r == true);
        
        // Vérifier que la 11ème requête est rejetée
        using var overLimitLease = await limiter.AcquireAsync();
        Check.That(overLimitLease.IsAcquired).IsFalse();
    }

    [Fact]
    public async Task TokenBucketLimiter_Should_AllowBursts()
    {
        // Arrange
        var options = new TokenBucketRateLimiterOptions
        {
            TokenLimit = 20,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            TokensPerPeriod = 10,
            AutoReplenishment = false, // Pas d'auto-replenishment pour test déterministe
            QueueLimit = 0
        };

        using var limiter = new TokenBucketRateLimiter(options);

        // Act - Consommer 20 tokens d'un coup (burst)
        var results = new List<bool>();
        for (int i = 0; i < 20; i++)
        {
            using var lease = await limiter.AcquireAsync(permitCount: 1);
            results.Add(lease.IsAcquired);
        }

        // Assert
        Check.That(results).ContainsOnlyElementsThatMatch(r => r == true);

        // Vérifier rejet après épuisement du bucket
        using var overLimitLease = await limiter.AcquireAsync();
        Check.That(overLimitLease.IsAcquired).IsFalse();
    }

    [Fact]
    public async Task ConcurrencyLimiter_Should_LimitSimultaneousRequests()
    {
        // Arrange
        var options = new ConcurrencyLimiterOptions
        {
            PermitLimit = 3,
            QueueLimit = 0
        };

        using var limiter = new ConcurrencyLimiter(options);

        // Act - Acquérir 3 leases simultanés (sans dispose)
        var lease1 = await limiter.AcquireAsync();
        var lease2 = await limiter.AcquireAsync();
        var lease3 = await limiter.AcquireAsync();

        // Tenter 4ème lease
        using var lease4 = await limiter.AcquireAsync();

        // Assert
        Check.That(lease1.IsAcquired).IsTrue();
        Check.That(lease2.IsAcquired).IsTrue();
        Check.That(lease3.IsAcquired).IsTrue();
        Check.That(lease4.IsAcquired).IsFalse();

        // Cleanup
        lease1.Dispose();
        lease2.Dispose();
        lease3.Dispose();

        // Vérifier qu'un nouveau lease peut être acquis après libération
        using var lease5 = await limiter.AcquireAsync();
        Check.That(lease5.IsAcquired).IsTrue();
    }

    [Fact]
    public async Task RateLimiter_Should_ProvideRetryAfterMetadata()
    {
        // Arrange
        var options = new FixedWindowRateLimiterOptions
        {
            PermitLimit = 1,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        };

        using var limiter = new FixedWindowRateLimiter(options);

        // Act - Consommer le seul permit
        using var lease1 = await limiter.AcquireAsync();
        
        // Tenter une 2ème requête (doit être rejetée)
        using var lease2 = await limiter.AcquireAsync();

        // Assert
        Check.That(lease2.IsAcquired).IsFalse();
        
        // Vérifier métadonnée RetryAfter
        var hasRetryAfter = lease2.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter);
        Check.That(hasRetryAfter).IsTrue();
        Check.That(retryAfter.TotalSeconds).IsStrictlyGreaterThan(0);
        Check.That(retryAfter.TotalSeconds).IsLessOrEqualThan(60.0);
    }
}
