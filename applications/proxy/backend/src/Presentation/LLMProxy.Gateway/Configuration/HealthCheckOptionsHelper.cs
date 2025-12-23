using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

namespace LLMProxy.Gateway.Configuration;

/// <summary>
/// Utilitaires pour configurer les options des health checks.
/// Centralise la configuration pour éviter la duplication et garantir la cohérence.
/// Conforme à ADR-038 (Health Checks et Readiness Probes).
/// </summary>
public static class HealthCheckOptionsHelper
{
    /// <summary>
    /// Crée les options pour le endpoint liveness probe.
    /// Utilisé par Kubernetes pour vérifier que le processus est vivant.
    /// Retourne 200 OK si le processus fonctionne (même si dépendances en panne).
    /// </summary>
    /// <returns>Options configurées pour liveness probe.</returns>
    public static HealthCheckOptions CreateLivenessOptions()
    {
        return new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            AllowCachingResponses = false,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK, // Liveness tolérant aux dégradations
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        };
    }

    /// <summary>
    /// Crée les options pour le endpoint readiness probe.
    /// Utilisé par Kubernetes et load balancers pour savoir si l'instance peut recevoir du trafic.
    /// Retourne 200 OK uniquement si toutes les dépendances critiques sont disponibles.
    /// Tolère les dégradations mineures (ex: Redis down mais app fonctionnelle).
    /// </summary>
    /// <returns>Options configurées pour readiness probe.</returns>
    public static HealthCheckOptions CreateReadinessOptions()
    {
        return new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            AllowCachingResponses = false,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK, // Readiness tolérant aux dégradations mineures
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        };
    }

    /// <summary>
    /// Crée les options pour le endpoint de monitoring complet.
    /// Retourne le détail de tous les checks avec statuts individuels.
    /// Utilisé pour monitoring, dashboards et debugging.
    /// </summary>
    /// <returns>Options configurées pour monitoring complet.</returns>
    public static HealthCheckOptions CreateMonitoringOptions()
    {
        return new HealthCheckOptions
        {
            AllowCachingResponses = false,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        };
    }
}
