using System.Reflection;
using LLMProxy.Infrastructure.Authorization.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Authorization;

/// <summary>
/// Comportement MediatR pour la vérification d'autorisation ReBAC.
/// </summary>
/// <remarks>
/// <para>
/// Ce comportement s'exécute avant le handler pour les requêtes marquées avec
/// <see cref="RequirePermissionAttribute"/> et implémentant <see cref="IAuthorizedRequest"/>.
/// </para>
/// <para>
/// Si l'utilisateur n'a pas les permissions requises, une <see cref="UnauthorizedException"/>
/// est levée, empêchant l'exécution du handler.
/// </para>
/// </remarks>
/// <typeparam name="TRequest">Type de la requête MediatR.</typeparam>
/// <typeparam name="TResponse">Type de la réponse MediatR.</typeparam>
public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="AuthorizationBehavior{TRequest, TResponse}"/>.
    /// </summary>
    /// <param name="authorizationService">Service d'autorisation ReBAC.</param>
    /// <param name="logger">Logger pour les traces.</param>
    public AuthorizationBehavior(
        IAuthorizationService authorizationService,
        ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    {
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Récupérer les attributs RequirePermission sur la requête
        var permissionAttributes = typeof(TRequest)
            .GetCustomAttributes<RequirePermissionAttribute>(inherit: true)
            .ToList();

        // Si aucun attribut de permission, passer directement au handler
        if (permissionAttributes.Count == 0)
        {
            return await next();
        }

        // Vérifier que la requête implémente IAuthorizedRequest
        if (request is not IAuthorizedRequest authorizedRequest)
        {
            _logger.LogWarning(
                "La requête {RequestType} a des attributs RequirePermission mais n'implémente pas IAuthorizedRequest",
                typeof(TRequest).Name);

            throw new InvalidOperationException(
                $"La requête {typeof(TRequest).Name} doit implémenter {nameof(IAuthorizedRequest)} " +
                $"pour utiliser {nameof(RequirePermissionAttribute)}");
        }

        var userId = authorizedRequest.GetUserId();
        var objectId = authorizedRequest.GetObjectId();

        _logger.LogDebug(
            "Vérification des autorisations pour {RequestType}: UserId={UserId}, ObjectId={ObjectId}, Permissions={PermissionCount}",
            typeof(TRequest).Name,
            userId,
            objectId,
            permissionAttributes.Count);

        // Vérifier chaque permission requise
        foreach (var permission in permissionAttributes)
        {
            var authRequest = new AuthorizationRequest(
                userId,
                permission.Relation,
                permission.ObjectType,
                objectId);

            var result = await _authorizationService.CheckAsync(authRequest, cancellationToken);

            if (result.IsDenied)
            {
                _logger.LogWarning(
                    "Autorisation refusée pour {RequestType}: {UserId} n'a pas la permission {Relation} sur {ObjectType}:{ObjectId}. Raison: {Reason}",
                    typeof(TRequest).Name,
                    userId,
                    permission.Relation,
                    permission.ObjectType,
                    objectId,
                    result.Reason);

                throw UnauthorizedException.ForPermission(
                    userId,
                    permission.Relation,
                    permission.ObjectType,
                    objectId);
            }

            _logger.LogDebug(
                "Autorisation accordée: {UserId} {Relation} {ObjectType}:{ObjectId}",
                userId,
                permission.Relation,
                permission.ObjectType,
                objectId);
        }

        _logger.LogDebug(
            "Toutes les autorisations vérifiées pour {RequestType}",
            typeof(TRequest).Name);

        // Toutes les autorisations sont validées, exécuter le handler
        return await next();
    }
}
