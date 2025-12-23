using Authorization.Domain.Abstractions;
using Authorization.Domain.Events;
using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Entities;

/// <summary>
/// Représente un principal (utilisateur, groupe ou service account) synchronisé depuis l'IDP.
/// </summary>
/// <remarks>
/// <para>
/// Les principals sont des entités externes gérées par l'Identity Provider (Azure AD, Okta, Keycloak).
/// Cette classe représente leur projection dans le système d'autorisation pour la correspondance
/// avec les assignations de rôles.
/// </para>
/// <para>
/// Le principal ne contient que les informations minimales nécessaires pour l'autorisation.
/// Les données complètes de l'utilisateur restent dans l'IDP.
/// </para>
/// </remarks>
public sealed class Principal : Entity<PrincipalId>
{
    /// <summary>
    /// Type de principal (User, Group, ServiceAccount).
    /// </summary>
    public PrincipalType Type { get; private set; }

    /// <summary>
    /// Nom d'affichage du principal.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Adresse email (pour les utilisateurs).
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Identifiant du tenant auquel appartient ce principal.
    /// </summary>
    public TenantId TenantId { get; private set; }

    /// <summary>
    /// Indique si le principal est actif dans l'IDP.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Date de la dernière synchronisation avec l'IDP.
    /// </summary>
    public DateTimeOffset LastSyncedAt { get; private set; }

    /// <summary>
    /// Données supplémentaires provenant de l'IDP (claims, attributs).
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; private set; }

    private Principal()
    {
        // Constructeur pour EF Core
        DisplayName = string.Empty;
        Metadata = new Dictionary<string, string>();
    }

    private Principal(
        PrincipalId id,
        PrincipalType type,
        string displayName,
        string? email,
        TenantId tenantId,
        bool isActive,
        IReadOnlyDictionary<string, string>? metadata)
    {
        Id = id;
        Type = type;
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        Email = email;
        TenantId = tenantId;
        IsActive = isActive;
        LastSyncedAt = DateTimeOffset.UtcNow;
        Metadata = metadata ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Crée un nouveau principal lors de la synchronisation depuis l'IDP.
    /// </summary>
    /// <param name="objectId">ObjectId de l'IDP (GUID).</param>
    /// <param name="type">Type de principal.</param>
    /// <param name="displayName">Nom d'affichage.</param>
    /// <param name="email">Adresse email (optionnelle).</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="isActive">État actif/inactif.</param>
    /// <param name="metadata">Métadonnées additionnelles.</param>
    /// <returns>Nouvelle instance de Principal.</returns>
    public static Principal Create(
        Guid objectId,
        PrincipalType type,
        string displayName,
        string? email,
        TenantId tenantId,
        bool isActive = true,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        var principal = new Principal(
            PrincipalId.Create(objectId),
            type,
            displayName,
            email,
            tenantId,
            isActive,
            metadata);

        principal.AddDomainEvent(new PrincipalSyncedEvent(
            principal.Id,
            principal.Type,
            principal.TenantId,
            PrincipalSyncAction.Created));

        return principal;
    }

    /// <summary>
    /// Met à jour les informations du principal depuis l'IDP.
    /// </summary>
    /// <param name="displayName">Nouveau nom d'affichage.</param>
    /// <param name="email">Nouvelle adresse email.</param>
    /// <param name="isActive">Nouvel état actif/inactif.</param>
    /// <param name="metadata">Nouvelles métadonnées.</param>
    public void UpdateFromIdp(
        string displayName,
        string? email,
        bool isActive,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        var wasActive = IsActive;

        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        Email = email;
        IsActive = isActive;
        Metadata = metadata ?? Metadata;
        LastSyncedAt = DateTimeOffset.UtcNow;

        var action = wasActive switch
        {
            true when !isActive => PrincipalSyncAction.Deactivated,
            false when isActive => PrincipalSyncAction.Reactivated,
            _ => PrincipalSyncAction.Updated
        };

        AddDomainEvent(new PrincipalSyncedEvent(Id, Type, TenantId, action));
    }

    /// <summary>
    /// Marque le principal comme supprimé de l'IDP.
    /// </summary>
    public void MarkAsDeleted()
    {
        IsActive = false;
        LastSyncedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new PrincipalSyncedEvent(
            Id, Type, TenantId, PrincipalSyncAction.Deleted));
    }

    /// <summary>
    /// Retourne la représentation OpenFGA du principal.
    /// </summary>
    /// <returns>Chaîne au format "{type}:{id}".</returns>
    public string ToOpenFgaFormat() => Id.ToOpenFgaFormat(Type);
}
