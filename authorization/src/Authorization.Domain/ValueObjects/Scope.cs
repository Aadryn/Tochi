using Authorization.Domain.Abstractions;

namespace Authorization.Domain.ValueObjects;

/// <summary>
/// Représente un segment d'un scope (type de ressource et valeur optionnelle).
/// </summary>
/// <param name="Type">Type du segment (domain, organizations, tenants, etc.).</param>
/// <param name="Value">Valeur du segment (identifiant de la ressource, null pour les collections).</param>
public readonly record struct ScopeSegment(string Type, string? Value)
{
    /// <inheritdoc />
    public override string ToString() =>
        Value is not null ? $"{Type}/{Value}" : Type;
}

/// <summary>
/// Représente un scope au format URL REST avec support de hiérarchie.
/// </summary>
/// <remarks>
/// <para>
/// Le scope définit le périmètre sur lequel une permission est accordée.
/// Le format suit une structure URL REST permettant une hiérarchie naturelle.
/// </para>
/// <example>
/// Exemples de scopes valides :
/// <code>
/// api.llmproxy.com                                          // Racine
/// api.llmproxy.com/organizations/org-123                    // Organisation
/// api.llmproxy.com/organizations/org-123/tenants/tenant-456 // Tenant
/// </code>
/// </example>
/// </remarks>
public sealed class Scope : ValueObject
{
    private static readonly char[] Separator = ['/'];

    /// <summary>
    /// Chemin complet du scope au format URL REST.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Segments composant le scope.
    /// </summary>
    public IReadOnlyList<ScopeSegment> Segments { get; }

    /// <summary>
    /// Domaine racine du scope.
    /// </summary>
    public string Domain => Segments.Count > 0 ? Segments[0].Value ?? Segments[0].Type : string.Empty;

    /// <summary>
    /// Profondeur du scope (nombre de segments).
    /// </summary>
    public int Depth => Segments.Count;

    private Scope(string path, IReadOnlyList<ScopeSegment> segments)
    {
        Path = path;
        Segments = segments;
    }

    /// <summary>
    /// Parse une chaîne URL en scope.
    /// </summary>
    /// <param name="path">Chemin URL du scope.</param>
    /// <returns>Instance de Scope.</returns>
    /// <exception cref="ArgumentException">Si le chemin est invalide.</exception>
    public static Scope Parse(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Le chemin du scope ne peut pas être vide.", nameof(path));
        }

        // Nettoyer le chemin
        var cleanPath = path.Trim().TrimEnd('/');
        
        // Valider le format (pas d'espaces, pas de schéma HTTP)
        if (cleanPath.Contains(' '))
        {
            throw new ArgumentException("Le chemin du scope ne peut pas contenir d'espaces.", nameof(path));
        }

        if (cleanPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            cleanPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Le chemin du scope ne doit pas inclure de schéma HTTP.", nameof(path));
        }

        var parts = cleanPath.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            throw new ArgumentException("Le chemin du scope doit contenir au moins un segment.", nameof(path));
        }

        var segments = new List<ScopeSegment>();

        // Premier segment = domaine
        segments.Add(new ScopeSegment("domain", parts[0]));

        // Segments suivants = paires type/valeur
        for (var i = 1; i < parts.Length; i += 2)
        {
            var type = parts[i];
            var value = i + 1 < parts.Length ? parts[i + 1] : null;
            segments.Add(new ScopeSegment(type, value));
        }

        return new Scope(cleanPath, segments.AsReadOnly());
    }

    /// <summary>
    /// Tente de parser une chaîne en Scope.
    /// </summary>
    /// <param name="path">Chemin à parser.</param>
    /// <param name="scope">Scope résultant si succès.</param>
    /// <returns>True si le parsing a réussi.</returns>
    public static bool TryParse(string? path, out Scope? scope)
    {
        scope = null;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            scope = Parse(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Crée un scope racine à partir d'un domaine.
    /// </summary>
    /// <param name="domain">Nom de domaine (ex: api.llmproxy.com).</param>
    /// <returns>Scope racine.</returns>
    public static Scope Root(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            throw new ArgumentException("Le domaine ne peut pas être vide.", nameof(domain));
        }

        return new Scope(domain, new[] { new ScopeSegment("domain", domain) });
    }

    /// <summary>
    /// Crée un scope enfant en ajoutant un type de ressource et son identifiant.
    /// </summary>
    /// <param name="resourceType">Type de ressource (ex: organizations, tenants).</param>
    /// <param name="resourceId">Identifiant de la ressource.</param>
    /// <returns>Nouveau scope enfant.</returns>
    public Scope Child(string resourceType, string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceType))
        {
            throw new ArgumentException("Le type de ressource ne peut pas être vide.", nameof(resourceType));
        }

        if (string.IsNullOrWhiteSpace(resourceId))
        {
            throw new ArgumentException("L'identifiant de ressource ne peut pas être vide.", nameof(resourceId));
        }

        var newPath = $"{Path}/{resourceType}/{resourceId}";
        var newSegments = Segments
            .Append(new ScopeSegment(resourceType, resourceId))
            .ToList()
            .AsReadOnly();

        return new Scope(newPath, newSegments);
    }

    /// <summary>
    /// Retourne le scope parent (un niveau au-dessus).
    /// </summary>
    /// <returns>Scope parent, ou null si c'est la racine.</returns>
    public Scope? GetParent()
    {
        if (Segments.Count <= 1)
        {
            return null;
        }

        var parentSegments = Segments.Take(Segments.Count - 1).ToList();
        var parentPath = BuildPath(parentSegments);

        return new Scope(parentPath, parentSegments.AsReadOnly());
    }

    /// <summary>
    /// Vérifie si ce scope est un descendant du scope spécifié.
    /// </summary>
    /// <param name="ancestor">Scope ancêtre potentiel.</param>
    /// <returns>True si ce scope est un descendant.</returns>
    public bool IsDescendantOf(Scope ancestor)
    {
        if (ancestor.Path.Length >= Path.Length)
        {
            return false;
        }

        return Path.StartsWith(ancestor.Path, StringComparison.OrdinalIgnoreCase) &&
               (Path.Length == ancestor.Path.Length ||
                Path[ancestor.Path.Length] == '/');
    }

    /// <summary>
    /// Vérifie si ce scope contient (est ancêtre de) un autre scope.
    /// </summary>
    /// <param name="descendant">Scope descendant potentiel.</param>
    /// <returns>True si ce scope contient le descendant.</returns>
    public bool Contains(Scope descendant)
    {
        return descendant.IsDescendantOf(this);
    }

    /// <summary>
    /// Retourne la hiérarchie complète des scopes (du plus spécifique au plus général).
    /// </summary>
    /// <returns>Énumération des scopes de la hiérarchie.</returns>
    public IEnumerable<Scope> GetHierarchy()
    {
        var current = this;
        while (current is not null)
        {
            yield return current;
            current = current.GetParent();
        }
    }

    /// <summary>
    /// Retourne la représentation OpenFGA du scope.
    /// </summary>
    /// <returns>Chaîne formatée pour OpenFGA (ex: scope:api.llmproxy.com/organizations/org-123).</returns>
    public string ToOpenFgaFormat() => $"scope:{Path}";

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Path.ToLowerInvariant();
    }

    /// <inheritdoc />
    public override string ToString() => Path;

    private static string BuildPath(IEnumerable<ScopeSegment> segments)
    {
        var parts = new List<string>();
        var isFirst = true;

        foreach (var segment in segments)
        {
            if (isFirst)
            {
                // Premier segment = domaine (valeur uniquement)
                parts.Add(segment.Value ?? segment.Type);
                isFirst = false;
            }
            else
            {
                // Segments suivants = type/valeur
                parts.Add(segment.Type);
                if (segment.Value is not null)
                {
                    parts.Add(segment.Value);
                }
            }
        }

        return string.Join("/", parts);
    }
}
