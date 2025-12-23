namespace LLMProxy.Domain.Entities;

/// <summary>
/// Méthode de routage pour la sélection de fournisseur LLM
/// </summary>
public enum RoutingMethod
{
    Path = 0,
    Header = 1,
    Subdomain = 2,
    UserConfig = 3
}
