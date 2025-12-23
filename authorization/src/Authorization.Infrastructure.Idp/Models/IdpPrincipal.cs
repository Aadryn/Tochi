// <copyright file="IdpPrincipal.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Authorization.Infrastructure.Idp.Models;

/// <summary>
/// Représente un principal générique provenant d'un Identity Provider.
/// Un principal peut être un utilisateur, un groupe ou un service account.
/// </summary>
/// <param name="ObjectId">Identifiant unique dans l'IDP (GUID).</param>
/// <param name="PrincipalType">Type du principal : User, Group, ServiceAccount.</param>
/// <param name="ExternalId">Identifiant externe (email, UPN, name).</param>
/// <param name="DisplayName">Nom d'affichage lisible.</param>
public record IdpPrincipal(
    Guid ObjectId,
    string PrincipalType,
    string ExternalId,
    string DisplayName);
