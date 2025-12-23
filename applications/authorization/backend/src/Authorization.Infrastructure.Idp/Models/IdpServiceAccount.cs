// <copyright file="IdpServiceAccount.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Authorization.Infrastructure.Idp.Models;

/// <summary>
/// Représente un service account (application/machine identity) provenant d'un Identity Provider.
/// Utilisé pour les authentifications machine-to-machine (M2M).
/// </summary>
/// <param name="ObjectId">Identifiant unique du service account dans l'IDP.</param>
/// <param name="ClientId">Client ID de l'application (OAuth2 client_id).</param>
/// <param name="DisplayName">Nom d'affichage du service account.</param>
public record IdpServiceAccount(
    Guid ObjectId,
    string ClientId,
    string DisplayName)
    : IdpPrincipal(ObjectId, "ServiceAccount", ClientId, DisplayName);
