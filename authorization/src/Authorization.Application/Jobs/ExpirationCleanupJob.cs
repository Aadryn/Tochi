// <copyright file="ExpirationCleanupJob.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Authorization.Domain.Entities;
using Authorization.Domain.Interfaces;
using Authorization.Domain.ValueObjects;
using Authorization.Infrastructure.OpenFGA.Services;
using Authorization.Infrastructure.Redis.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Authorization.Application.Jobs;

/// <summary>
/// BackgroundService pour purger les assignations de rôles expirées.
/// </summary>
/// <remarks>
/// <para>
/// Ce job s'exécute périodiquement (par défaut toutes les 5 minutes) pour :
/// </para>
/// <list type="number">
/// <item>Lire les assignations expirées depuis PostgreSQL</item>
/// <item>Supprimer les tuples correspondants dans OpenFGA</item>
/// <item>Invalider les entrées de cache Redis</item>
/// <item>Supprimer les entrées d'expiration</item>
/// <item>Logger les actions pour audit</item>
/// </list>
/// <para>
/// Chaque assignation est traitée individuellement avec gestion d'erreurs
/// pour garantir qu'une erreur sur une assignation n'empêche pas le
/// traitement des autres.
/// </para>
/// </remarks>
public sealed class ExpirationCleanupJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ExpirationCleanupOptions _options;
    private readonly ILogger<ExpirationCleanupJob> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ExpirationCleanupJob"/>.
    /// </summary>
    /// <param name="scopeFactory">Factory pour créer des scopes DI.</param>
    /// <param name="options">Options de configuration du job.</param>
    /// <param name="logger">Logger.</param>
    public ExpirationCleanupJob(
        IServiceScopeFactory scopeFactory,
        IOptions<ExpirationCleanupOptions> options,
        ILogger<ExpirationCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Job de cleanup des expirations désactivé");
            return;
        }

        _logger.LogInformation(
            "Job de cleanup des expirations démarré - Intervalle: {Interval}",
            _options.Interval);

        // Délai initial avant la première exécution
        await Task.Delay(_options.InitialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredAssignmentsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Arrêt normal demandé
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du cleanup des expirations");
            }

            await Task.Delay(_options.Interval, stoppingToken);
        }

        _logger.LogInformation("Job de cleanup des expirations arrêté");
    }

    /// <summary>
    /// Nettoie les assignations expirées.
    /// </summary>
    private async Task CleanupExpiredAssignmentsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var expirationStore = scope.ServiceProvider.GetRequiredService<IExpirationStore>();
        var openFgaService = scope.ServiceProvider.GetRequiredService<IOpenFgaService>();
        var permissionCache = scope.ServiceProvider.GetService<IPermissionCacheService>();

        var now = DateTimeOffset.UtcNow;
        var expiredAssignments = await expirationStore.GetExpiredAsync(
            now,
            _options.BatchSize,
            cancellationToken);

        if (expiredAssignments.Count == 0)
        {
            _logger.LogDebug("Aucune assignation expirée à nettoyer");
            return;
        }

        _logger.LogInformation(
            "Trouvé {Count} assignation(s) expirée(s) à nettoyer",
            expiredAssignments.Count);

        var successCount = 0;
        var errorCount = 0;

        foreach (var assignment in expiredAssignments)
        {
            try
            {
                await CleanupSingleAssignmentAsync(
                    assignment,
                    expirationStore,
                    openFgaService,
                    permissionCache,
                    cancellationToken);

                successCount++;
            }
            catch (Exception ex)
            {
                errorCount++;
                _logger.LogError(
                    ex,
                    "Échec du nettoyage de l'assignation {Id} ({Role} sur {Scope})",
                    assignment.Id,
                    assignment.Role,
                    assignment.Scope);
            }
        }

        _logger.LogInformation(
            "Cleanup terminé: {Success} succès, {Errors} erreurs",
            successCount,
            errorCount);
    }

    /// <summary>
    /// Nettoie une assignation unique.
    /// </summary>
    private async Task CleanupSingleAssignmentAsync(
        AssignmentExpiration assignment,
        IExpirationStore expirationStore,
        IOpenFgaService openFgaService,
        IPermissionCacheService? permissionCache,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(assignment.TenantId);
        var user = FormatUser(assignment);

        // 1. Supprimer le tuple dans OpenFGA
        await openFgaService.DeleteAsync(
            tenantId,
            user,
            assignment.Role,
            "scope",
            assignment.Scope,
            cancellationToken);

        _logger.LogDebug(
            "Tuple OpenFGA supprimé: {User} {Role} scope:{Scope}",
            user,
            assignment.Role,
            assignment.Scope);

        // 2. Invalider le cache si disponible
        if (permissionCache is not null)
        {
            var principalId = PrincipalId.Create(assignment.PrincipalId);
            await permissionCache.InvalidatePrincipalAsync(tenantId, principalId, cancellationToken);

            _logger.LogDebug(
                "Cache invalidé pour {PrincipalType}:{PrincipalId}",
                assignment.PrincipalType,
                assignment.PrincipalId);
        }

        // 3. Supprimer l'entrée d'expiration
        await expirationStore.DeleteAsync(assignment.Id, cancellationToken);

        _logger.LogInformation(
            "Assignation expirée nettoyée: {Role} sur {Scope} pour {PrincipalType}:{PrincipalId}",
            assignment.Role,
            assignment.Scope,
            assignment.PrincipalType,
            assignment.PrincipalId);
    }

    /// <summary>
    /// Formate l'identifiant utilisateur au format OpenFGA.
    /// </summary>
    private static string FormatUser(AssignmentExpiration assignment)
    {
        return assignment.PrincipalType.ToLowerInvariant() switch
        {
            "user" => $"user:{assignment.PrincipalId}",
            "group" => $"group:{assignment.PrincipalId}#member",
            "serviceaccount" => $"serviceaccount:{assignment.PrincipalId}",
            _ => throw new InvalidOperationException(
                $"Type de principal non supporté: {assignment.PrincipalType}")
        };
    }
}
