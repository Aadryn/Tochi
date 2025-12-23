// <copyright file="ExpirationCleanupOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Authorization.Application.Jobs;

/// <summary>
/// Options de configuration du job de nettoyage des assignations expirées.
/// </summary>
/// <remarks>
/// <para>
/// Ce job s'exécute périodiquement pour supprimer les tuples OpenFGA
/// dont la date d'expiration est dépassée.
/// </para>
/// <para>
/// Configuration dans appsettings.json :
/// </para>
/// <code>
/// {
///   "ExpirationCleanup": {
///     "IntervalMinutes": 5,
///     "BatchSize": 100,
///     "Enabled": true
///   }
/// }
/// </code>
/// </remarks>
public sealed class ExpirationCleanupOptions
{
    /// <summary>
    /// Nom de la section de configuration.
    /// </summary>
    public const string SectionName = "ExpirationCleanup";

    /// <summary>
    /// Intervalle entre deux exécutions du cleanup (en minutes).
    /// </summary>
    /// <value>Par défaut : 5 minutes.</value>
    public int IntervalMinutes { get; set; } = 5;

    /// <summary>
    /// Obtient l'intervalle sous forme de TimeSpan.
    /// </summary>
    public TimeSpan Interval => TimeSpan.FromMinutes(IntervalMinutes);

    /// <summary>
    /// Nombre maximum d'assignations à traiter par batch.
    /// </summary>
    /// <value>Par défaut : 100.</value>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Indique si le job de cleanup est activé.
    /// </summary>
    /// <value>Par défaut : true.</value>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Délai initial avant la première exécution (en secondes).
    /// </summary>
    /// <value>Par défaut : 30 secondes.</value>
    /// <remarks>
    /// Permet à l'application de démarrer complètement avant
    /// de lancer le cleanup.
    /// </remarks>
    public int InitialDelaySeconds { get; set; } = 30;

    /// <summary>
    /// Obtient le délai initial sous forme de TimeSpan.
    /// </summary>
    public TimeSpan InitialDelay => TimeSpan.FromSeconds(InitialDelaySeconds);
}
