using Authorization.API.Contracts.Requests;
using Authorization.API.Contracts.Responses;
using Authorization.API.Extensions;
using Authorization.Application.Services.Authorization;
using Authorization.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.API.Controllers;

/// <summary>
/// Controller pour la gestion des assignations de rôles.
/// </summary>
[ApiController]
[Route("api/v1/assignments")]
[Produces("application/json")]
[Authorize]
public sealed class RoleAssignmentsController : ControllerBase
{
    private readonly IRbacAuthorizationService _authorizationService;
    private readonly ILogger<RoleAssignmentsController> _logger;

    /// <summary>
    /// Initialise le controller.
    /// </summary>
    /// <param name="authorizationService">Service d'autorisation.</param>
    /// <param name="logger">Logger.</param>
    public RoleAssignmentsController(
        IRbacAuthorizationService authorizationService,
        ILogger<RoleAssignmentsController> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    /// <summary>
    /// Liste les assignations de rôles pour un principal.
    /// </summary>
    /// <param name="principalId">ID du principal (optionnel, défaut: appelant).</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des assignations.</returns>
    /// <response code="200">Liste retournée.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AssignmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AssignmentResponse>>> ListAssignments(
        [FromQuery] Guid? principalId,
        CancellationToken cancellationToken)
    {
        var tenantId = User.GetTenantId()
            ?? throw new InvalidOperationException("Tenant ID required");

        var targetPrincipalId = principalId.HasValue
            ? PrincipalId.Create(principalId.Value)
            : User.GetPrincipalId();

        var assignments = await _authorizationService.ListRoleAssignmentsAsync(
            tenantId,
            targetPrincipalId,
            cancellationToken);

        var responses = assignments.Select(a => new AssignmentResponse
        {
            Id = a.Id.Value,
            PrincipalId = targetPrincipalId.Value.ToString(), // Le principal est passé en paramètre
            PrincipalType = "user", // TODO: Récupérer depuis le repository
            RoleId = a.RoleId.Value,
            RoleName = a.RoleName,
            Scope = a.Scope.Path,
            CreatedAt = a.AssignedAt,
            CreatedBy = a.AssignedBy?.Value.ToString() ?? "system",
            ExpiresAt = a.ExpiresAt
        });

        return Ok(responses);
    }

    /// <summary>
    /// Crée une nouvelle assignation de rôle.
    /// </summary>
    /// <param name="request">Détails de l'assignation.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Assignation créée.</returns>
    /// <response code="201">Assignation créée.</response>
    /// <response code="400">Requête invalide.</response>
    /// <response code="403">Non autorisé à créer cette assignation.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AssignmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AssignmentResponse>> CreateAssignment(
        [FromBody] CreateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = User.GetTenantId()
            ?? throw new InvalidOperationException("Tenant ID required");

        var actorId = User.GetPrincipalId();
        var actorType = User.GetPrincipalType();

        var targetPrincipalId = PrincipalId.Create(request.PrincipalId);
        var targetPrincipalType = ParsePrincipalType(request.PrincipalType);
        var roleId = RoleId.Create(request.RoleId);
        var scope = Scope.Parse(request.Scope);

        _logger.LogInformation(
            "Creating assignment: {Role} for {PrincipalType}:{PrincipalId} on {Scope} by {Actor}",
            roleId,
            targetPrincipalType,
            targetPrincipalId,
            scope.Path,
            actorId);

        try
        {
            var assignmentId = await _authorizationService.AssignRoleAsync(
                tenantId,
                actorId,
                actorType,
                targetPrincipalId,
                targetPrincipalType,
                roleId,
                scope,
                request.ExpiresAt?.DateTime,
                cancellationToken);

            var response = new AssignmentResponse
            {
                Id = assignmentId.Value,
                PrincipalId = request.PrincipalId.ToString(),
                PrincipalType = request.PrincipalType,
                RoleId = request.RoleId,
                RoleName = request.RoleId, // TODO: Résoudre le nom
                Scope = request.Scope,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = actorId.Value.ToString(),
                ExpiresAt = request.ExpiresAt
            };

            return CreatedAtAction(
                nameof(GetAssignment),
                new { id = assignmentId.Value },
                response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(
                "Assignment denied: {Reason}. Actor: {Actor}, Target: {Target}, Role: {Role}",
                ex.Message,
                actorId,
                targetPrincipalId,
                roleId);

            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden,
                title: "Assignation refusée");
        }
    }

    /// <summary>
    /// Récupère une assignation par ID.
    /// </summary>
    /// <param name="id">ID de l'assignation.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Assignation.</returns>
    /// <response code="200">Assignation trouvée.</response>
    /// <response code="404">Assignation non trouvée.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignmentResponse>> GetAssignment(
        Guid id,
        CancellationToken cancellationToken)
    {
        // TODO: Implémenter GetAssignmentById dans IAuthorizationService
        await Task.CompletedTask;
        return NotFound();
    }

    /// <summary>
    /// Révoque une assignation de rôle.
    /// </summary>
    /// <param name="id">ID de l'assignation à révoquer.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>NoContent si révoqué.</returns>
    /// <response code="204">Assignation révoquée.</response>
    /// <response code="403">Non autorisé à révoquer cette assignation.</response>
    /// <response code="404">Assignation non trouvée.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeAssignment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tenantId = User.GetTenantId()
            ?? throw new InvalidOperationException("Tenant ID required");

        var actorId = User.GetPrincipalId();
        var actorType = User.GetPrincipalType();
        var assignmentId = RoleAssignmentId.Create(id);

        try
        {
            await _authorizationService.RevokeRoleAsync(
                tenantId,
                actorId,
                actorType,
                assignmentId,
                cancellationToken);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden,
                title: "Révocation refusée");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
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
