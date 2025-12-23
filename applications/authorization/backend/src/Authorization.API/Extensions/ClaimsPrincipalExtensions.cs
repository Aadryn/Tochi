using System.Security.Claims;
using Authorization.Domain.ValueObjects;

namespace Authorization.API.Extensions;

/// <summary>
/// Extensions pour extraire des informations du ClaimsPrincipal.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Claim standard pour l'object ID (Azure AD / Entra ID).
    /// </summary>
    private const string ObjectIdClaim = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    /// <summary>
    /// Claim standard pour le subject (OpenID Connect).
    /// </summary>
    private const string SubjectClaim = "sub";

    /// <summary>
    /// Claim Keycloak pour l'ID utilisateur.
    /// </summary>
    private const string KeycloakUserIdClaim = "preferred_username";

    /// <summary>
    /// Extrait l'identifiant du principal depuis les claims.
    /// Cherche dans l'ordre : oid, sub, preferred_username.
    /// </summary>
    /// <param name="principal">Principal authentifié.</param>
    /// <returns>Identifiant du principal.</returns>
    /// <exception cref="InvalidOperationException">Si aucun identifiant trouvé.</exception>
    public static PrincipalId GetPrincipalId(this ClaimsPrincipal principal)
    {
        var objectId = principal.FindFirstValue(ObjectIdClaim)
            ?? principal.FindFirstValue(SubjectClaim)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(objectId))
        {
            throw new InvalidOperationException(
                "Unable to determine principal ID from claims. " +
                $"Expected one of: {ObjectIdClaim}, {SubjectClaim}, {ClaimTypes.NameIdentifier}");
        }

        if (Guid.TryParse(objectId, out var guid))
        {
            return PrincipalId.Create(guid);
        }

        // Pour les identifiants non-GUID (ex: email), utiliser un hash
        return PrincipalId.Parse(objectId);
    }

    /// <summary>
    /// Extrait le type de principal depuis les claims.
    /// </summary>
    /// <param name="principal">Principal authentifié.</param>
    /// <returns>Type de principal (User, ServiceAccount, Group).</returns>
    public static PrincipalType GetPrincipalType(this ClaimsPrincipal principal)
    {
        // Vérifier si c'est un service account (client credentials flow)
        var azp = principal.FindFirstValue("azp"); // Authorized party
        var clientId = principal.FindFirstValue("client_id");

        // Si pas de subject mais un client_id, c'est un service account
        var sub = principal.FindFirstValue(SubjectClaim);
        if (sub == clientId && !string.IsNullOrEmpty(clientId))
        {
            return PrincipalType.ServiceAccount;
        }

        // Par défaut, c'est un utilisateur
        return PrincipalType.User;
    }

    /// <summary>
    /// Extrait le tenant ID depuis les claims.
    /// </summary>
    /// <param name="principal">Principal authentifié.</param>
    /// <returns>Tenant ID ou null si non trouvé.</returns>
    public static TenantId? GetTenantId(this ClaimsPrincipal principal)
    {
        // Claim personnalisé pour le tenant
        var tenantClaim = principal.FindFirstValue("tenant_id")
            ?? principal.FindFirstValue("tid"); // Azure AD tenant ID

        if (string.IsNullOrEmpty(tenantClaim))
        {
            return null;
        }

        return TenantId.Parse(tenantClaim);
    }

    /// <summary>
    /// Extrait l'email de l'utilisateur.
    /// </summary>
    /// <param name="principal">Principal authentifié.</param>
    /// <returns>Email ou null si non trouvé.</returns>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Email)
            ?? principal.FindFirstValue("email");
    }

    /// <summary>
    /// Extrait le nom d'affichage de l'utilisateur.
    /// </summary>
    /// <param name="principal">Principal authentifié.</param>
    /// <returns>Nom d'affichage ou null si non trouvé.</returns>
    public static string? GetDisplayName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Name)
            ?? principal.FindFirstValue("name")
            ?? principal.FindFirstValue("preferred_username");
    }

    /// <summary>
    /// Extrait les groupes de l'utilisateur depuis les claims.
    /// </summary>
    /// <param name="principal">Principal authentifié.</param>
    /// <returns>Liste des IDs de groupe.</returns>
    public static IReadOnlyList<string> GetGroups(this ClaimsPrincipal principal)
    {
        return principal
            .FindAll("groups")
            .Select(c => c.Value)
            .ToList();
    }

    /// <summary>
    /// Vérifie si le principal a un claim spécifique.
    /// </summary>
    /// <param name="principal">Principal authentifié.</param>
    /// <param name="claimType">Type de claim.</param>
    /// <param name="claimValue">Valeur attendue.</param>
    /// <returns>True si le claim existe avec la valeur.</returns>
    public static bool HasClaim(
        this ClaimsPrincipal principal,
        string claimType,
        string claimValue)
    {
        return principal.HasClaim(c =>
            c.Type == claimType &&
            c.Value.Equals(claimValue, StringComparison.OrdinalIgnoreCase));
    }
}
