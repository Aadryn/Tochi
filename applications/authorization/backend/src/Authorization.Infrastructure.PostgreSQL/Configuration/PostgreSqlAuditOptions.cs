namespace Authorization.Infrastructure.PostgreSQL.Configuration;

/// <summary>
/// Options de configuration pour la base de données PostgreSQL d'audit.
/// </summary>
public class PostgreSqlAuditOptions
{
    /// <summary>
    /// Nom de la section de configuration.
    /// </summary>
    public const string SectionName = "Authorization:Audit:PostgreSql";

    /// <summary>
    /// Chaîne de connexion à la base de données.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Nom du schéma à utiliser.
    /// </summary>
    public string Schema { get; set; } = "authorization";

    /// <summary>
    /// Nombre de jours de rétention des logs.
    /// </summary>
    public int RetentionDays { get; set; } = 90;

    /// <summary>
    /// Indique si l'écriture des logs est asynchrone.
    /// </summary>
    public bool AsyncWrite { get; set; } = true;

    /// <summary>
    /// Taille du batch pour l'écriture asynchrone.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Intervalle en secondes pour le flush du batch.
    /// </summary>
    public int FlushIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Timeout de commande en secondes.
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Nombre maximum de tentatives en cas d'erreur.
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Délai de base entre les tentatives en millisecondes.
    /// </summary>
    public int RetryDelayMs { get; set; } = 100;
}
