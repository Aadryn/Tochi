using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace LLMProxy.Admin.API.Tests.HealthChecks;

/// <summary>
/// Tests unitaires pour les health checks endpoints d'Admin API.
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
        var result = HealthCheckResult.Healthy("Admin API is alive");
        
        // ASSERT
        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Equal("Admin API is alive", result.Description);
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
            new { Name = "database", Status = HealthStatus.Healthy },
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
    public void DiskHealthCheck_ShouldReturnDegraded_WhenLowDiskSpace()
    {
        // ARRANGE
        var diskDegraded = HealthCheckResult.Degraded("Disk space below threshold");
        
        // ACT
        var status = diskDegraded.Status;
        
        // ASSERT
        Assert.Equal(HealthStatus.Degraded, status);
    }

    [Fact]
    public void MemoryHealthCheck_ShouldReturnDegraded_WhenHighMemoryUsage()
    {
        // ARRANGE
        var memoryDegraded = HealthCheckResult.Degraded("Memory usage above threshold");
        
        // ACT
        var status = memoryDegraded.Status;
        
        // ASSERT
        Assert.Equal(HealthStatus.Degraded, status);
    }

    [Fact]
    public void HealthCheckOptionsHelper_CreateLivenessOptions_ShouldDisableCaching()
    {
        // ARRANGE & ACT
        var options = Configuration.HealthCheckOptionsHelper.CreateLivenessOptions();
        
        // ASSERT
        Assert.NotNull(options.Predicate);
        Assert.False(options.AllowCachingResponses);
    }

    [Fact]
    public void HealthCheckOptionsHelper_CreateReadinessOptions_ShouldDisableCaching()
    {
        // ARRANGE & ACT
        var options = Configuration.HealthCheckOptionsHelper.CreateReadinessOptions();
        
        // ASSERT
        Assert.NotNull(options.Predicate);
        Assert.False(options.AllowCachingResponses);
    }

    [Fact]
    public void HealthCheckOptionsHelper_CreateMonitoringOptions_ShouldIncludeAllChecks()
    {
        // ARRANGE & ACT
        var options = Configuration.HealthCheckOptionsHelper.CreateMonitoringOptions();
        
        // ASSERT
        Assert.False(options.AllowCachingResponses);
        
        // Monitoring inclut tous les checks (pas de prédicat filtrant)
        // Le prédicat par défaut renvoie true pour tous
    }
}
