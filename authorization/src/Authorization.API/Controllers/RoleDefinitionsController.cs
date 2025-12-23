using Authorization.API.Contracts.Responses;
using Authorization.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.API.Controllers;

/// <summary>
/// Controller pour la gestion des définitions de rôles.
/// </summary>
[ApiController]
[Route("api/v1/roles")]
[Produces("application/json")]
[Authorize]
public sealed class RoleDefinitionsController : ControllerBase
{
    private readonly ILogger<RoleDefinitionsController> _logger;

    // Rôles prédéfinis (cache statique)
    private static readonly IReadOnlyList<RoleResponse> BuiltInRoles = new[]
    {
        new RoleResponse
        {
            Id = RoleId.Owner.Value,
            Name = "Owner",
            Description = "Toutes les permissions, incluant la gestion des rôles et la délégation.",
            IsBuiltIn = true,
            Permissions = new[] { "*:manage" },
            AssignableScopes = new[] { "/" }
        },
        new RoleResponse
        {
            Id = RoleId.Contributor.Value,
            Name = "Contributor",
            Description = "Permissions de lecture et écriture, sans gestion des accès.",
            IsBuiltIn = true,
            Permissions = new[] { "*:read", "*:write" },
            AssignableScopes = new[] { "/" }
        },
        new RoleResponse
        {
            Id = RoleId.Reader.Value,
            Name = "Reader",
            Description = "Permissions de lecture seule.",
            IsBuiltIn = true,
            Permissions = new[] { "*:read" },
            AssignableScopes = new[] { "/" }
        }
    };

    /// <summary>
    /// Initialise le controller.
    /// </summary>
    /// <param name="logger">Logger.</param>
    public RoleDefinitionsController(ILogger<RoleDefinitionsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Liste tous les rôles disponibles.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des rôles.</returns>
    /// <response code="200">Liste retournée.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoleResponse>>> ListRoles(
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        // TODO: Ajouter les rôles personnalisés depuis le repository
        return Ok(BuiltInRoles);
    }

    /// <summary>
    /// Récupère un rôle par ID.
    /// </summary>
    /// <param name="id">Identifiant du rôle.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Définition du rôle.</returns>
    /// <response code="200">Rôle trouvé.</response>
    /// <response code="404">Rôle non trouvé.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> GetRole(
        string id,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var role = BuiltInRoles.FirstOrDefault(r =>
            r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        if (role == null)
        {
            return NotFound();
        }

        return Ok(role);
    }

    /// <summary>
    /// Récupère les permissions d'un rôle.
    /// </summary>
    /// <param name="id">Identifiant du rôle.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des permissions.</returns>
    /// <response code="200">Permissions retournées.</response>
    /// <response code="404">Rôle non trouvé.</response>
    [HttpGet("{id}/permissions")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<string>>> GetRolePermissions(
        string id,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var role = BuiltInRoles.FirstOrDefault(r =>
            r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        if (role == null)
        {
            return NotFound();
        }

        return Ok(role.Permissions);
    }
}
