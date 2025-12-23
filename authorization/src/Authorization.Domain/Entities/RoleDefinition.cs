using Authorization.Domain.Abstractions;
using Authorization.Domain.Events;
using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Entities;

/// <summary>
/// Représente une définition de rôle avec ses permissions associées.
/// </summary>
/// <remarks>
/// <para>
/// Les rôles définissent un ensemble de permissions qui peuvent être assignées à des principals.
/// Il existe deux types de rôles :
/// - Rôles de base (Owner, Contributor, Reader) : prédéfinis et non modifiables
/// - Rôles personnalisés : créés par les utilisateurs avec des permissions spécifiques
/// </para>
/// <para>
/// Les rôles sont scopés à un tenant et peuvent avoir une hiérarchie implicite
/// (Owner > Contributor > Reader).
/// </para>
/// </remarks>
public sealed class RoleDefinition : Entity<RoleId>
{
    private readonly List<Permission> _permissions = [];

    /// <summary>
    /// Nom d'affichage du rôle.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Description du rôle.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Tenant auquel appartient ce rôle.
    /// </summary>
    public TenantId TenantId { get; private set; }

    /// <summary>
    /// Indique si c'est un rôle de base (non modifiable).
    /// </summary>
    public bool IsBuiltIn { get; private set; }

    /// <summary>
    /// Ordre de priorité pour la délégation hiérarchique.
    /// Plus le niveau est élevé, plus le rôle a de privilèges.
    /// </summary>
    public int HierarchyLevel { get; private set; }

    /// <summary>
    /// Permissions accordées par ce rôle.
    /// </summary>
    public IReadOnlyList<Permission> Permissions => _permissions.AsReadOnly();

    /// <summary>
    /// Indique si ce rôle peut être délégué.
    /// </summary>
    public bool IsDelegatable { get; private set; }

    /// <summary>
    /// Date de création du rôle.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Date de dernière modification.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; private set; }

    private RoleDefinition()
    {
        // Constructeur pour EF Core
        Name = string.Empty;
    }

    private RoleDefinition(
        RoleId id,
        string name,
        string? description,
        TenantId tenantId,
        bool isBuiltIn,
        int hierarchyLevel,
        bool isDelegatable,
        IEnumerable<Permission>? permissions)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        TenantId = tenantId;
        IsBuiltIn = isBuiltIn;
        HierarchyLevel = hierarchyLevel;
        IsDelegatable = isDelegatable;
        CreatedAt = DateTimeOffset.UtcNow;

        if (permissions is not null)
        {
            _permissions.AddRange(permissions);
        }
    }

    /// <summary>
    /// Crée un rôle personnalisé.
    /// </summary>
    /// <param name="name">Nom du rôle.</param>
    /// <param name="description">Description du rôle.</param>
    /// <param name="tenantId">Tenant auquel appartient le rôle.</param>
    /// <param name="permissions">Permissions initiales.</param>
    /// <param name="hierarchyLevel">Niveau de hiérarchie (défaut: 50).</param>
    /// <returns>Nouvelle définition de rôle.</returns>
    public static RoleDefinition CreateCustom(
        string name,
        string? description,
        TenantId tenantId,
        IEnumerable<Permission>? permissions = null,
        int hierarchyLevel = 50)
    {
        var role = new RoleDefinition(
            RoleId.NewCustom(),
            name,
            description,
            tenantId,
            isBuiltIn: false,
            hierarchyLevel,
            isDelegatable: true,
            permissions);

        role.AddDomainEvent(new RoleDefinitionCreatedEvent(
            role.Id, role.Name, role.TenantId, role.IsBuiltIn));

        return role;
    }

    /// <summary>
    /// Crée le rôle Owner (rôle de base avec toutes les permissions).
    /// </summary>
    /// <param name="tenantId">Tenant.</param>
    /// <returns>Définition du rôle Owner.</returns>
    public static RoleDefinition CreateOwner(TenantId tenantId)
    {
        var permissions = new[]
        {
            Permission.Create("*", Permission.Actions.Manage)
        };

        return new RoleDefinition(
            RoleId.Owner,
            "Owner",
            "Toutes les permissions, incluant la gestion des rôles et la délégation.",
            tenantId,
            isBuiltIn: true,
            hierarchyLevel: 100,
            isDelegatable: true,
            permissions);
    }

    /// <summary>
    /// Crée le rôle Contributor (lecture et écriture).
    /// </summary>
    /// <param name="tenantId">Tenant.</param>
    /// <returns>Définition du rôle Contributor.</returns>
    public static RoleDefinition CreateContributor(TenantId tenantId)
    {
        var permissions = new[]
        {
            Permission.Create(Permission.Resources.Prompts, Permission.Actions.Read),
            Permission.Create(Permission.Resources.Prompts, Permission.Actions.Create),
            Permission.Create(Permission.Resources.Prompts, Permission.Actions.Update),
            Permission.Create(Permission.Resources.Prompts, Permission.Actions.Delete),
            Permission.Create(Permission.Resources.Prompts, Permission.Actions.List),
            Permission.Create(Permission.Resources.Models, Permission.Actions.Read),
            Permission.Create(Permission.Resources.Models, Permission.Actions.List),
            Permission.Create(Permission.Resources.Configurations, Permission.Actions.Read),
        };

        return new RoleDefinition(
            RoleId.Contributor,
            "Contributor",
            "Lecture et écriture sur les ressources principales.",
            tenantId,
            isBuiltIn: true,
            hierarchyLevel: 75,
            isDelegatable: true,
            permissions);
    }

    /// <summary>
    /// Crée le rôle Reader (lecture seule).
    /// </summary>
    /// <param name="tenantId">Tenant.</param>
    /// <returns>Définition du rôle Reader.</returns>
    public static RoleDefinition CreateReader(TenantId tenantId)
    {
        var permissions = new[]
        {
            Permission.Create(Permission.Resources.Prompts, Permission.Actions.Read),
            Permission.Create(Permission.Resources.Prompts, Permission.Actions.List),
            Permission.Create(Permission.Resources.Models, Permission.Actions.Read),
            Permission.Create(Permission.Resources.Models, Permission.Actions.List),
        };

        return new RoleDefinition(
            RoleId.Reader,
            "Reader",
            "Lecture seule sur les ressources.",
            tenantId,
            isBuiltIn: true,
            hierarchyLevel: 25,
            isDelegatable: false,
            permissions);
    }

    /// <summary>
    /// Ajoute une permission au rôle (rôles personnalisés uniquement).
    /// </summary>
    /// <param name="permission">Permission à ajouter.</param>
    /// <exception cref="InvalidOperationException">Si le rôle est un rôle de base.</exception>
    public void AddPermission(Permission permission)
    {
        if (IsBuiltIn)
        {
            throw new InvalidOperationException("Les rôles de base ne peuvent pas être modifiés.");
        }

        if (_permissions.Contains(permission))
        {
            return; // Permission déjà présente
        }

        _permissions.Add(permission);
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new RoleDefinitionUpdatedEvent(Id, TenantId));
    }

    /// <summary>
    /// Retire une permission du rôle (rôles personnalisés uniquement).
    /// </summary>
    /// <param name="permission">Permission à retirer.</param>
    /// <exception cref="InvalidOperationException">Si le rôle est un rôle de base.</exception>
    public void RemovePermission(Permission permission)
    {
        if (IsBuiltIn)
        {
            throw new InvalidOperationException("Les rôles de base ne peuvent pas être modifiés.");
        }

        if (_permissions.Remove(permission))
        {
            UpdatedAt = DateTimeOffset.UtcNow;
            AddDomainEvent(new RoleDefinitionUpdatedEvent(Id, TenantId));
        }
    }

    /// <summary>
    /// Met à jour les informations du rôle (rôles personnalisés uniquement).
    /// </summary>
    /// <param name="name">Nouveau nom.</param>
    /// <param name="description">Nouvelle description.</param>
    /// <exception cref="InvalidOperationException">Si le rôle est un rôle de base.</exception>
    public void Update(string name, string? description)
    {
        if (IsBuiltIn)
        {
            throw new InvalidOperationException("Les rôles de base ne peuvent pas être modifiés.");
        }

        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new RoleDefinitionUpdatedEvent(Id, TenantId));
    }

    /// <summary>
    /// Vérifie si ce rôle accorde une permission spécifique.
    /// </summary>
    /// <param name="permission">Permission à vérifier.</param>
    /// <returns>True si le rôle accorde cette permission.</returns>
    public bool HasPermission(Permission permission)
    {
        return _permissions.Any(p => p.Implies(permission));
    }

    /// <summary>
    /// Vérifie si ce rôle peut déléguer un autre rôle (délégation hiérarchique).
    /// </summary>
    /// <param name="targetRole">Rôle cible de la délégation.</param>
    /// <returns>True si la délégation est autorisée.</returns>
    public bool CanDelegate(RoleDefinition targetRole)
    {
        // Un rôle ne peut déléguer qu'un rôle de niveau inférieur ou égal
        return HierarchyLevel >= targetRole.HierarchyLevel && targetRole.IsDelegatable;
    }
}
