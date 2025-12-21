using LLMProxy.Application.Common;

namespace LLMProxy.Application.Tenants.Commands;

/// <summary>
/// Commande pour activer un tenant
/// </summary>
public record ActivateTenantCommand : ICommand
{
    public Guid TenantId { get; init; }
}
