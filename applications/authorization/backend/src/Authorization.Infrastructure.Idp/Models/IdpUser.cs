// <copyright file="IdpUser.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Authorization.Infrastructure.Idp.Models;

/// <summary>
/// Représente un utilisateur provenant d'un Identity Provider.
/// Hérite des propriétés de base de <see cref="IdpPrincipal"/>.
/// </summary>
/// <param name="ObjectId">Identifiant unique de l'utilisateur dans l'IDP.</param>
/// <param name="Email">Adresse email de l'utilisateur.</param>
/// <param name="DisplayName">Nom d'affichage de l'utilisateur.</param>
/// <param name="UserPrincipalName">Nom principal utilisateur (UPN) si applicable.</param>
/// <param name="IsEnabled">Indique si le compte utilisateur est actif.</param>
public record IdpUser(
    Guid ObjectId,
    string Email,
    string DisplayName,
    string? UserPrincipalName,
    bool IsEnabled)
    : IdpPrincipal(ObjectId, "User", Email, DisplayName);
