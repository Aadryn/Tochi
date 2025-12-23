using LLMProxy.Application.ApiKeys.Commands;
using LLMProxy.Application.ApiKeys.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LLMProxy.Admin.API.Controllers.V20251222;

/// <summary>
/// API Keys Management - Version 2025-12-22
/// Version détectée automatiquement depuis le namespace
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "TenantAdmin")]
[EnableRateLimiting("per-ip")] // ADR-041: Protection DDoS
public class ApiKeysController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ApiKeysController> _logger;

    public ApiKeysController(IMediator mediator, ILogger<ApiKeysController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get API keys for a user
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var query = new GetApiKeysByUserIdQuery { UserId = userId };
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result.Value);
    }

    /// <summary>
    /// Get API keys for a tenant
    /// </summary>
    [HttpGet("tenant/{tenantId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTenantId(Guid tenantId, CancellationToken cancellationToken)
    {
        var query = new GetApiKeysByTenantIdQuery { TenantId = tenantId };
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result.Value);
    }

    /// <summary>
    /// Create new API key
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateApiKeyCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetByUserId), new { userId = command.UserId }, result.Value);
    }

    /// <summary>
    /// Revoke API key
    /// </summary>
    [HttpPost("{id:guid}/revoke")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
    {
        var command = new RevokeApiKeyCommand { ApiKeyId = id };
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result.Error);
        }

        return Ok();
    }

    /// <summary>
    /// Delete API key permanently
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteApiKeyCommand { ApiKeyId = id };
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result.Error);
        }

        return NoContent();
    }
}
