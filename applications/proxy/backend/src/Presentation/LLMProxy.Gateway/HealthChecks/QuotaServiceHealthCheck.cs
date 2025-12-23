using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LLMProxy.Gateway.HealthChecks;

/// <summary>
/// Health check personnalisé pour vérifier la disponibilité du service de quotas.
/// Teste que Redis est accessible via le QuotaService.
/// Conforme à ADR-038 (Health Checks et Readiness Probes).
/// </summary>
public sealed class QuotaServiceHealthCheck : IHealthCheck
{
    private readonly IQuotaService _quotaService;
    private readonly ILogger<QuotaServiceHealthCheck> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="QuotaServiceHealthCheck"/>.
    /// </summary>
    /// <param name="quotaService">Service de quotas à vérifier.</param>
    /// <param name="logger">Logger pour journaliser les échecs.</param>
    public QuotaServiceHealthCheck(
        IQuotaService quotaService,
        ILogger<QuotaServiceHealthCheck> logger)
    {
        _quotaService = quotaService;
        _logger = logger;
    }

    /// <summary>
    /// Vérifie la santé du service de quotas en tentant d'accéder à Redis.
    /// </summary>
    /// <param name="context">Contexte du health check.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Résultat du health check (Healthy si Redis accessible, Degraded sinon).</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Tenter de récupérer un quota fictif pour tester Redis
            var testUserId = Guid.Empty; // Utilisateur fictif
            var testType = Domain.Entities.QuotaType.RequestsPerMinute;

            var usage = await _quotaService.GetUsageAsync(testUserId, testType, cancellationToken);

            // Si aucune exception, Redis est accessible
            return HealthCheckResult.Healthy("Quota service accessible (Redis OK)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quota service health check failed");
            
            // Redis inaccessible → Degraded (app fonctionne sans quotas temps réel)
            return HealthCheckResult.Degraded(
                "Quota service unavailable (Redis down or unreachable)",
                exception: ex);
        }
    }
}
