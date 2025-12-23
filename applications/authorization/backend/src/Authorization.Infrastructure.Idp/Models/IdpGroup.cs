// <copyright file="IdpGroup.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Authorization.Infrastructure.Idp.Models;

/// <summary>
/// Représente un groupe provenant d'un Identity Provider.
/// Hérite des propriétés de base de <see cref="IdpPrincipal"/>.
/// </summary>
/// <param name="ObjectId">Identifiant unique du groupe dans l'IDP.</param>
/// <param name="Name">Nom du groupe.</param>
/// <param name="Description">Description optionnelle du groupe.</param>
public record IdpGroup(
    Guid ObjectId,
    string Name,
    string? Description)
    : IdpPrincipal(ObjectId, "Group", Name, Name);
