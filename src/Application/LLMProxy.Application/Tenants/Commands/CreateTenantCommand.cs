using LLMProxy.Application.Common;

namespace LLMProxy.Application.Tenants.Commands;

/// <summary>
/// Commande pour créer un nouveau tenant
/// </summary>
public record CreateTenantCommand : ICommand<TenantDto>
{
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public TenantSettingsDto? Settings { get; init; }
}
