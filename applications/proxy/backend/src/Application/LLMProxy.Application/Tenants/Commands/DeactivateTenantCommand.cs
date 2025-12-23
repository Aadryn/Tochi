using LLMProxy.Application.Common;

namespace LLMProxy.Application.Tenants.Commands;

/// <summary>
/// Commande pour désactiver un tenant
/// </summary>
public record DeactivateTenantCommand : ICommand
{
    public Guid TenantId { get; init; }
}
