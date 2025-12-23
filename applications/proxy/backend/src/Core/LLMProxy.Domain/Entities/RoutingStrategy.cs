using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Stratégie de routage pour la sélection de fournisseur
/// </summary>
public class RoutingStrategy : ValueObject
{
    public RoutingMethod Method { get; private set; }
    public string? PathPattern { get; private set; }
    public string? HeaderName { get; private set; }
    public string? Subdomain { get; private set; }

    private RoutingStrategy() { }

    public RoutingStrategy(
        RoutingMethod method,
        string? pathPattern = null,
        string? headerName = null,
        string? subdomain = null)
    {
        Method = method;
        PathPattern = pathPattern;
        HeaderName = headerName;
        Subdomain = subdomain;
    }

    public static RoutingStrategy ByPath(string pathPattern) =>
        new(RoutingMethod.Path, pathPattern: pathPattern);

    public static RoutingStrategy ByHeader(string headerName) =>
        new(RoutingMethod.Header, headerName: headerName);

    public static RoutingStrategy BySubdomain(string subdomain) =>
        new(RoutingMethod.Subdomain, subdomain: subdomain);

    public static RoutingStrategy ByUser() =>
        new(RoutingMethod.UserConfig);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Method;
        yield return PathPattern;
        yield return HeaderName;
        yield return Subdomain;
    }
}
