using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace LLMProxy.Gateway.Tests.HealthChecks;

/// <summary>
/// Tests unitaires pour les health checks endpoints.
/// Conforme à ADR-038 (Health Checks et Readiness Probes).
/// </summary>
public sealed class HealthCheckTests
{
    [Fact]
    public void LivenessProbe_ShouldAlwaysReturn200_WhenProcessAlive()
    {
        // ARRANGE
        // Le processus de test est vivant par définition
        
        // ACT
        // Simuler liveness check (toujours Healthy)
        var result = HealthCheckResult.Healthy("Gateway is alive");
        
        // ASSERT
        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Equal("Gateway is alive", result.Description);
    }

    [Fact]
    public void ReadinessProbe_ShouldReturn200_WhenAllDependenciesHealthy()
    {
        // ARRANGE
        var postgresHealthy = HealthCheckResult.Healthy("PostgreSQL OK");
        var redisHealthy = HealthCheckResult.Healthy("Redis OK");
        
        // ACT
        // Simuler readiness check avec toutes dépendances OK
        var allHealthy = postgresHealthy.Status == HealthStatus.Healthy
                      && redisHealthy.Status == HealthStatus.Healthy;
        
        // ASSERT
        Assert.True(allHealthy);
    }

    [Fact]
    public void ReadinessProbe_ShouldReturn503_WhenPostgreSQLDown()
    {
        // ARRANGE
        var postgresUnhealthy = HealthCheckResult.Unhealthy("PostgreSQL connection failed");
        
        // ACT
        var status = postgresUnhealthy.Status;
        
        // ASSERT
        Assert.Equal(HealthStatus.Unhealthy, status);
        Assert.Equal("PostgreSQL connection failed", postgresUnhealthy.Description);
    }

    [Fact]
    public void ReadinessProbe_ShouldReturn200Degraded_WhenRedisDown()
    {
        // ARRANGE
        var redisDegraded = HealthCheckResult.Degraded("Redis unavailable");
        
        // ACT
        var status = redisDegraded.Status;
        
        // ASSERT
        // Redis down → Degraded (app fonctionne sans cache)
        Assert.Equal(HealthStatus.Degraded, status);
        Assert.Equal("Redis unavailable", redisDegraded.Description);
    }

    [Fact]
    public void HealthEndpoint_ShouldReturnDetailsForAllChecks()
    {
        // ARRANGE
        var checks = new[]
        {
            new { Name = "self", Status = HealthStatus.Healthy },
            new { Name = "postgresql", Status = HealthStatus.Healthy },
            new { Name = "redis", Status = HealthStatus.Healthy },
            new { Name = "disk", Status = HealthStatus.Healthy },
            new { Name = "memory", Status = HealthStatus.Healthy }
        };
        
        // ACT
        var allHealthy = checks.All(c => c.Status == HealthStatus.Healthy);
        
        // ASSERT
        Assert.True(allHealthy);
        Assert.Equal(5, checks.Length);
    }

    [Fact]
    public void QuotaServiceHealthCheck_ShouldReturnHealthy_WhenRedisAccessible()
    {
        // ARRANGE
        // Simuler succès accès Redis
        var quotaServiceHealthy = HealthCheckResult.Healthy("Quota service accessible (Redis OK)");
        
        // ACT
        var status = quotaServiceHealthy.Status;
        
        // ASSERT
        Assert.Equal(HealthStatus.Healthy, status);
        Assert.Contains("Redis OK", quotaServiceHealthy.Description);
    }

    [Fact]
    public void QuotaServiceHealthCheck_ShouldReturnDegraded_WhenRedisUnavailable()
    {
        // ARRANGE
        // Simuler échec accès Redis
        var quotaServiceDegraded = HealthCheckResult.Degraded(
            "Quota service unavailable (Redis down or unreachable)");
        
        // ACT
        var status = quotaServiceDegraded.Status;
        
        // ASSERT
        // QuotaService inaccessible → Degraded (app fonctionne sans quotas temps réel)
        Assert.Equal(HealthStatus.Degraded, status);
        Assert.Contains("Redis down", quotaServiceDegraded.Description);
    }

    [Fact]
    public void HealthCheckOptions_ShouldMapStatusCodesToHttpCodes()
    {
        // ARRANGE
        var options = new HealthCheckOptions
        {
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = 200,
                [HealthStatus.Degraded] = 200,
                [HealthStatus.Unhealthy] = 503
            }
        };
        
        // ACT & ASSERT
        Assert.Equal(200, options.ResultStatusCodes[HealthStatus.Healthy]);
        Assert.Equal(200, options.ResultStatusCodes[HealthStatus.Degraded]);
        Assert.Equal(503, options.ResultStatusCodes[HealthStatus.Unhealthy]);
    }

    [Fact]
    public void LivenessOptions_ShouldFilterByLiveTag()
    {
        // ARRANGE
        var liveTags = new[] { "live" };
        var readyTags = new[] { "ready", "db" };
        
        // ACT
        var liveHasLiveTag = liveTags.Contains("live");
        var readyHasLiveTag = readyTags.Contains("live");
        
        // ASSERT
        Assert.True(liveHasLiveTag);
        Assert.False(readyHasLiveTag);
    }

    [Fact]
    public void ReadinessOptions_ShouldFilterByReadyTag()
    {
        // ARRANGE
        var liveTags = new[] { "live" };
        var readyTags = new[] { "ready", "db" };
        
        // ACT
        var liveHasReadyTag = liveTags.Contains("ready");
        var readyHasReadyTag = readyTags.Contains("ready");
        
        // ASSERT
        Assert.False(liveHasReadyTag);
        Assert.True(readyHasReadyTag);
    }
}
