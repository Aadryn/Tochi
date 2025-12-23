using System.Diagnostics;
using Authorization.API.Contracts.Requests;
using Authorization.API.Contracts.Responses;
using Authorization.API.Extensions;
using Authorization.Application.Services.Authorization;
using Authorization.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.API.Controllers;

/// <summary>
/// Controller pour les opérations de vérification de permissions.
/// </summary>
[ApiController]
[Route("api/v1/permissions")]
[Produces("application/json")]
[Authorize]
public sealed class PermissionsController : ControllerBase
{
    private readonly IRbacAuthorizationService _authorizationService;
    private readonly ILogger<PermissionsController> _logger;

    /// <summary>
    /// Initialise le controller.
    /// </summary>
    /// <param name="authorizationService">Service d'autorisation.</param>
    /// <param name="logger">Logger.</param>
    public PermissionsController(
        IRbacAuthorizationService authorizationService,
        ILogger<PermissionsController> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    /// <summary>
    /// Vérifie si le principal courant (ou spécifié) a une permission.
    /// </summary>
    /// <param name="request">Requête de vérification.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Résultat de la vérification.</returns>
    /// <response code="200">Vérification effectuée.</response>
    /// <response code="400">Requête invalide.</response>
    /// <response code="401">Non authentifié.</response>
    [HttpPost("check")]
    [ProducesResponseType(typeof(PermissionCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PermissionCheckResponse>> CheckPermission(
        [FromBody] CheckPermissionRequest request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        // Extraire le tenant depuis le scope ou les claims
        var tenantId = User.GetTenantId() ?? ExtractTenantFromScope(request.Scope);

        // Déterminer le principal à vérifier
        var principalId = request.PrincipalId.HasValue
            ? PrincipalId.Create(request.PrincipalId.Value)
            : User.GetPrincipalId();

        var principalType = !string.IsNullOrEmpty(request.PrincipalType)
            ? ParsePrincipalType(request.PrincipalType)
            : User.GetPrincipalType();

        // Parser la permission
        var permission = Permission.Parse(request.Permission);
        var scope = Scope.Parse(request.Scope);

        _logger.LogDebug(
            "Checking permission {Permission} for {PrincipalType}:{PrincipalId} on {Scope}",
            permission,
            principalType,
            principalId,
            scope.Path);

        var allowed = await _authorizationService.CheckPermissionAsync(
            tenantId,
            principalId,
            principalType,
            permission,
            scope,
            cancellationToken);

        stopwatch.Stop();

        return Ok(new PermissionCheckResponse
        {
            Allowed = allowed,
            PrincipalId = principalId.Value.ToString(),
            PrincipalType = principalType.ToString().ToLowerInvariant(),
            Permission = request.Permission,
            Scope = request.Scope,
            GrantingRole = null, // TODO: Implémenter avec résultat enrichi
            GrantingScope = null,
            FromCache = false, // TODO: Implémenter avec résultat enrichi
            DurationMs = stopwatch.Elapsed.TotalMilliseconds
        });
    }

    /// <summary>
    /// Vérifie plusieurs permissions en une seule requête.
    /// </summary>
    /// <param name="requests">Liste des permissions à vérifier.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des résultats de vérification.</returns>
    /// <response code="200">Vérifications effectuées.</response>
    [HttpPost("check/batch")]
    [ProducesResponseType(typeof(IEnumerable<PermissionCheckResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PermissionCheckResponse>>> CheckPermissionsBatch(
        [FromBody] IEnumerable<CheckPermissionRequest> requests,
        CancellationToken cancellationToken)
    {
        var results = new List<PermissionCheckResponse>();

        foreach (var request in requests)
        {
            var result = await CheckPermission(request, cancellationToken);
            if (result.Value != null)
            {
                results.Add(result.Value);
            }
        }

        return Ok(results);
    }

    private static TenantId ExtractTenantFromScope(string scopePath)
    {
        // Le scope contient le tenant : api.llmproxy.com/organizations/org-123/tenants/tenant-456
        var parts = scopePath.Split('/');
        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (parts[i].Equals("tenants", StringComparison.OrdinalIgnoreCase))
            {
                return TenantId.Parse(parts[i + 1]);
            }
        }

        throw new ArgumentException(
            $"Unable to extract tenant from scope: {scopePath}",
            nameof(scopePath));
    }

    private static PrincipalType ParsePrincipalType(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "user" => PrincipalType.User,
            "group" => PrincipalType.Group,
            "serviceaccount" or "service_account" or "service-account" => PrincipalType.ServiceAccount,
            _ => throw new ArgumentException($"Unknown principal type: {type}", nameof(type))
        };
    }
}
