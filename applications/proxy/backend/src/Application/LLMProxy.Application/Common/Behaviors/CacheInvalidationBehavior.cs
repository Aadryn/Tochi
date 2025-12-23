using LLMProxy.Domain.Interfaces;
using LLMProxy.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Common.Behaviors;

/// <summary>
/// Behavior pour invalider le cache après les Commands qui modifient des données.
/// Conforme à ADR-042 (Distributed Cache Strategy).
/// </summary>
/// <remarks>
/// Ce behavior intercepte les Commands implémentant <see cref="ICacheInvalidator"/>
/// et supprime les entrées de cache correspondantes après succès de la Command.
/// </remarks>
/// <typeparam name="TRequest">Type de la Command (doit implémenter ICacheInvalidator)</typeparam>
/// <typeparam name="TResponse">Type de la réponse</typeparam>
public sealed class CacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheInvalidator
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidationBehavior<TRequest, TResponse>> _logger;

    public CacheInvalidationBehavior(
        ICacheService cacheService,
        ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Exécute la Command puis invalide le cache selon les patterns définis.
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Exécuter la Command (modification en DB)
        var response = await next();

        // Invalider le cache après succès de la Command
        var cacheKeys = request.GetCacheKeysToInvalidate();

        if (cacheKeys.Any())
        {
            foreach (var cacheKey in cacheKeys)
            {
                // Si contient wildcard (*), utiliser pattern matching
                if (cacheKey.Contains('*'))
                {
                    await _cacheService.RemoveByPatternAsync(cacheKey, cancellationToken);
                    
                    _logger.LogDebug(
                        "Cache invalidé par pattern '{Pattern}' après {CommandType}",
                        cacheKey,
                        typeof(TRequest).Name);
                }
                else
                {
                    await _cacheService.RemoveAsync(cacheKey, cancellationToken);
                    
                    _logger.LogDebug(
                        "Cache invalidé pour clé '{CacheKey}' après {CommandType}",
                        cacheKey,
                        typeof(TRequest).Name);
                }
            }
        }

        return response;
    }
}
