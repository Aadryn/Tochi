using Authorization.Domain.Abstractions;
using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Events;

/// <summary>
/// Événement émis lors de la création d'une définition de rôle.
/// </summary>
/// <param name="RoleId">Identifiant du rôle créé.</param>
/// <param name="Name">Nom du rôle.</param>
/// <param name="TenantId">Tenant du rôle.</param>
/// <param name="IsBuiltIn">Indique si c'est un rôle de base.</param>
public sealed record RoleDefinitionCreatedEvent(
    RoleId RoleId,
    string Name,
    TenantId TenantId,
    bool IsBuiltIn) : IDomainEvent;

/// <summary>
/// Événement émis lors de la mise à jour d'une définition de rôle.
/// </summary>
/// <param name="RoleId">Identifiant du rôle.</param>
/// <param name="TenantId">Tenant du rôle.</param>
public sealed record RoleDefinitionUpdatedEvent(
    RoleId RoleId,
    TenantId TenantId) : IDomainEvent;

/// <summary>
/// Événement émis lors de la suppression d'une définition de rôle.
/// </summary>
/// <param name="RoleId">Identifiant du rôle supprimé.</param>
/// <param name="TenantId">Tenant du rôle.</param>
public sealed record RoleDefinitionDeletedEvent(
    RoleId RoleId,
    TenantId TenantId) : IDomainEvent;
