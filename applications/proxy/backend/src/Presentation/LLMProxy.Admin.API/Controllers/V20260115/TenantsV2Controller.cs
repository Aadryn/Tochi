using LLMProxy.Application.Tenants.Commands;
using LLMProxy.Application.Tenants.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LLMProxy.Admin.API.Controllers.V20260115;

/// <summary>
/// Tenants API v2 - Version 2026-01-15
/// Version détectée automatiquement depuis le namespace
/// </summary>
/// <remarks>
/// Cette version ajoute :
/// - Pagination native (GET /api/v2026-01-15/tenants?page=1&amp;pageSize=20)
/// - Réponses enrichies avec métadonnées (total, hasMore, etc.)
/// - Codes HTTP plus explicites (201 Created avec Location header)
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/tenants")]
[Authorize(Policy = "AdminOnly")]
[EnableRateLimiting("per-ip")] // ADR-041: Protection DDoS
public class TenantsV2Controller : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TenantsV2Controller> _logger;

    /// <summary>
    /// Initialise une nouvelle instance du contrôleur Tenants v2.0
    /// </summary>
    /// <param name="mediator">Médiateur pour l'envoi des commandes et requêtes CQRS</param>
    /// <param name="logger">Logger pour la traçabilité des opérations</param>
    public TenantsV2Controller(IMediator mediator, ILogger<TenantsV2Controller> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Récupère un tenant par son identifiant (v2.0 - Réponse enrichie)
    /// </summary>
    /// <param name="id">Identifiant unique du tenant</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones</param>
    /// <returns>Tenant avec métadonnées supplémentaires</returns>
    /// <response code="200">Tenant trouvé</response>
    /// <response code="404">Tenant introuvable</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetTenantByIdQuery { TenantId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new 
            { 
                error = result.Error,
                requestId = HttpContext.TraceIdentifier,
                timestamp = DateTime.UtcNow
            });
        }

        // V2: Réponse enrichie avec métadonnées
        return Ok(new 
        {
            data = result.Value,
            requestId = HttpContext.TraceIdentifier,
            timestamp = DateTime.UtcNow,
            version = "2.0"
        });
    }

    /// <summary>
    /// Récupère tous les tenants avec pagination (v2.0 - NOUVELLE fonctionnalité)
    /// </summary>
    /// <param name="page">Numéro de page (commence à 1)</param>
    /// <param name="pageSize">Nombre d'éléments par page (max 100)</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste paginée de tenants avec métadonnées</returns>
    /// <response code="200">Liste paginée de tenants</response>
    /// <response code="400">Paramètres de pagination invalides</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Validation des paramètres de pagination
        if (page < 1)
        {
            return BadRequest(new { error = "Le numéro de page doit être >= 1" });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "La taille de page doit être entre 1 et 100" });
        }

        // Récupération de tous les tenants (V2 utilise la même query pour l'instant)
        var query = new GetAllTenantsQuery();
        var result = await _mediator.Send(query, cancellationToken);

        var allTenants = result.Value.ToList();
        var totalCount = allTenants.Count;
        
        // Pagination manuelle (en attendant query paginée)
        var pagedTenants = allTenants
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // V2: Réponse paginée avec métadonnées complètes
        return Ok(new 
        {
            data = pagedTenants,
            pagination = new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                hasNext = page * pageSize < totalCount,
                hasPrevious = page > 1
            },
            requestId = HttpContext.TraceIdentifier,
            timestamp = DateTime.UtcNow,
            version = "2.0"
        });
    }

    /// <summary>
    /// Crée un nouveau tenant (v2.0 - Header Location amélioré)
    /// </summary>
    /// <param name="command">Commande de création du tenant</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Tenant créé avec Location header</returns>
    /// <response code="201">Tenant créé avec succès</response>
    /// <response code="400">Données invalides</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTenantCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new 
            { 
                error = result.Error,
                requestId = HttpContext.TraceIdentifier,
                timestamp = DateTime.UtcNow
            });
        }

        var createdTenant = result.Value;
        var tenantId = createdTenant.Id;

        // V2: CreatedAtAction avec Location header explicite
        return CreatedAtAction(
            nameof(GetById),
            new { id = tenantId },
            new 
            {
                data = new { id = tenantId },
                requestId = HttpContext.TraceIdentifier,
                timestamp = DateTime.UtcNow,
                version = "2026-01-15"
            });
    }
}
