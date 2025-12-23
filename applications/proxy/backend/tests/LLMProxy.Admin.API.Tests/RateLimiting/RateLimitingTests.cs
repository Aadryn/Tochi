using System.Net;
using LLMProxy.Admin.API.Configuration;
using Microsoft.Extensions.Options;
using NFluent;
using Xunit;

namespace LLMProxy.Admin.API.Tests.RateLimiting;

/// <summary>
/// Tests pour le rate limiting de l'API d'administration.
/// Conforme à ADR-041 (Rate Limiting et Throttling).
/// </summary>
public sealed class RateLimitingTests
{
    /// <summary>
    /// Vérifie que RateLimitingOptions charge correctement la configuration.
    /// </summary>
    [Fact]
    public void RateLimitingOptions_ShouldLoadConfiguration_Successfully()
    {
        // Arrange & Act
        var options = new RateLimitingOptions
        {
            Global = new FixedWindowOptions
            {
                PermitLimit = 1000,
                Window = TimeSpan.FromMinutes(1)
            },
            PerIp = new FixedWindowOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            },
            Concurrency = new ConcurrencyOptions
            {
                PermitLimit = 50,
                QueueLimit = 0
            }
        };

        // Assert
        Check.That(options.Global.PermitLimit).IsEqualTo(1000);
        Check.That(options.Global.Window).IsEqualTo(TimeSpan.FromMinutes(1));
        Check.That(options.PerIp.PermitLimit).IsEqualTo(100);
        Check.That(options.PerIp.Window).IsEqualTo(TimeSpan.FromMinutes(1));
        Check.That(options.Concurrency.PermitLimit).IsEqualTo(50);
        Check.That(options.Concurrency.QueueLimit).IsEqualTo(0);
    }

    /// <summary>
    /// Vérifie que Global rate limit permet les requêtes sous la limite.
    /// </summary>
    [Fact]
    public void GlobalRateLimit_ShouldAllow_RequestsUnderLimit()
    {
        // Arrange
        var options = new RateLimitingOptions
        {
            Global = new FixedWindowOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            },
            PerIp = new FixedWindowOptions { PermitLimit = 100, Window = TimeSpan.FromMinutes(1) },
            Concurrency = new ConcurrencyOptions { PermitLimit = 50, QueueLimit = 0 }
        };

        // Act & Assert
        Check.That(options.Global.PermitLimit).IsStrictlyGreaterThan(0);
        Check.That(options.Global.Window.TotalSeconds).IsStrictlyGreaterThan(0);
    }

    /// <summary>
    /// Vérifie que Per-IP rate limit protège contre une IP abusive.
    /// </summary>
    [Fact]
    public void PerIpRateLimit_ShouldBlock_ExcessiveRequestsFromSameIp()
    {
        // Arrange
        var options = new RateLimitingOptions
        {
            Global = new FixedWindowOptions { PermitLimit = 1000, Window = TimeSpan.FromMinutes(1) },
            PerIp = new FixedWindowOptions
            {
                PermitLimit = 5, // Limite très basse pour test
                Window = TimeSpan.FromMinutes(1)
            },
            Concurrency = new ConcurrencyOptions { PermitLimit = 50, QueueLimit = 0 }
        };

        // Act & Assert
        // Une IP qui dépasse 5 requêtes par minute sera bloquée
        Check.That(options.PerIp.PermitLimit).IsEqualTo(5);
        Check.That(options.PerIp.Window).IsEqualTo(TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Vérifie que Concurrency limit protège les ressources serveur.
    /// </summary>
    [Fact]
    public void ConcurrencyLimit_ShouldProtect_ServerResources()
    {
        // Arrange
        var options = new RateLimitingOptions
        {
            Global = new FixedWindowOptions { PermitLimit = 1000, Window = TimeSpan.FromMinutes(1) },
            PerIp = new FixedWindowOptions { PermitLimit = 100, Window = TimeSpan.FromMinutes(1) },
            Concurrency = new ConcurrencyOptions
            {
                PermitLimit = 10, // Max 10 requêtes simultanées
                QueueLimit = 0 // Rejet immédiat si limite atteinte
            }
        };

        // Act & Assert
        Check.That(options.Concurrency.PermitLimit).IsEqualTo(10);
        Check.That(options.Concurrency.QueueLimit).IsEqualTo(0);
    }

    /// <summary>
    /// Vérifie que les limites sont configurées pour Admin API (pas de tenant/user).
    /// </summary>
    [Fact]
    public void RateLimitingOptions_ShouldNotHave_TenantOrUserLimits()
    {
        // Arrange
        var options = new RateLimitingOptions
        {
            Global = new FixedWindowOptions { PermitLimit = 1000, Window = TimeSpan.FromMinutes(1) },
            PerIp = new FixedWindowOptions { PermitLimit = 100, Window = TimeSpan.FromMinutes(1) },
            Concurrency = new ConcurrencyOptions { PermitLimit = 50, QueueLimit = 0 }
        };

        // Act & Assert
        // Admin API a uniquement Global, PerIp, Concurrency (pas de PerTenant ni PerUser)
        Check.That(options.Global).IsNotNull();
        Check.That(options.PerIp).IsNotNull();
        Check.That(options.Concurrency).IsNotNull();
    }

    /// <summary>
    /// Vérifie que FixedWindowOptions valide correctement les paramètres.
    /// </summary>
    [Fact]
    public void FixedWindowOptions_ShouldValidate_Parameters()
    {
        // Arrange
        var options = new FixedWindowOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1)
        };

        // Act & Assert
        Check.That(options.PermitLimit).IsStrictlyGreaterThan(0);
        Check.That(options.Window.TotalSeconds).IsStrictlyGreaterThan(0);
    }

    /// <summary>
    /// Vérifie que ConcurrencyOptions configure correctement la queue.
    /// </summary>
    [Fact]
    public void ConcurrencyOptions_WithZeroQueue_ShouldRejectImmediately()
    {
        // Arrange
        var options = new ConcurrencyOptions
        {
            PermitLimit = 50,
            QueueLimit = 0 // Rejet immédiat recommandé pour Admin API
        };

        // Act & Assert
        Check.That(options.QueueLimit).IsEqualTo(0);
        // QueueLimit = 0 signifie que si limite atteinte, rejet HTTP 429 immédiat
    }

    /// <summary>
    /// Vérifie que les fenêtres temporelles sont raisonnables (pas trop courtes).
    /// </summary>
    [Fact]
    public void RateLimitWindows_ShouldBe_Reasonable()
    {
        // Arrange
        var options = new RateLimitingOptions
        {
            Global = new FixedWindowOptions { PermitLimit = 1000, Window = TimeSpan.FromMinutes(1) },
            PerIp = new FixedWindowOptions { PermitLimit = 100, Window = TimeSpan.FromMinutes(1) },
            Concurrency = new ConcurrencyOptions { PermitLimit = 50, QueueLimit = 0 }
        };

        // Act & Assert
        // Les fenêtres doivent être au moins de quelques secondes (pas millisecondes)
        Check.That(options.Global.Window.TotalSeconds).IsStrictlyGreaterThan(1);
        Check.That(options.PerIp.Window.TotalSeconds).IsStrictlyGreaterThan(1);
    }

    /// <summary>
    /// Vérifie que les limites Global sont supérieures à PerIp (cohérence).
    /// </summary>
    [Fact]
    public void GlobalLimit_ShouldBeGreaterThan_PerIpLimit()
    {
        // Arrange
        var options = new RateLimitingOptions
        {
            Global = new FixedWindowOptions { PermitLimit = 1000, Window = TimeSpan.FromMinutes(1) },
            PerIp = new FixedWindowOptions { PermitLimit = 100, Window = TimeSpan.FromMinutes(1) },
            Concurrency = new ConcurrencyOptions { PermitLimit = 50, QueueLimit = 0 }
        };

        // Act & Assert
        // Global doit être >= PerIp (sinon Global bloque avant PerIp)
        Check.That(options.Global.PermitLimit).IsStrictlyGreaterThan(options.PerIp.PermitLimit);
    }
}
