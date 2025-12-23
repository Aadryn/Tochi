namespace Authorization.Infrastructure.Redis.Configuration;

/// <summary>
/// Options de configuration pour le cache Redis.
/// </summary>
public class RedisCacheOptions
{
    /// <summary>
    /// Nom de la section de configuration.
    /// </summary>
    public const string SectionName = "Authorization:Cache:Redis";

    /// <summary>
    /// Chaîne de connexion Redis.
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Nom de l'instance (préfixe des clés).
    /// </summary>
    public string InstanceName { get; set; } = "llmproxy:authorization:";

    /// <summary>
    /// Indique si le cache est activé.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// TTL par défaut en secondes pour les résultats de vérification de permission.
    /// </summary>
    public int PermissionCheckTtlSeconds { get; set; } = 300;

    /// <summary>
    /// TTL par défaut en secondes pour les informations de principal.
    /// </summary>
    public int PrincipalTtlSeconds { get; set; } = 600;

    /// <summary>
    /// TTL par défaut en secondes pour les définitions de rôles.
    /// </summary>
    public int RoleDefinitionTtlSeconds { get; set; } = 3600;

    /// <summary>
    /// TTL par défaut en secondes pour les assignations de rôles.
    /// </summary>
    public int RoleAssignmentTtlSeconds { get; set; } = 300;

    /// <summary>
    /// Timeout de connexion en millisecondes.
    /// </summary>
    public int ConnectTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Timeout de synchronisation en millisecondes.
    /// </summary>
    public int SyncTimeoutMs { get; set; } = 1000;

    /// <summary>
    /// Nombre de tentatives de connexion.
    /// </summary>
    public int ConnectRetry { get; set; } = 3;

    /// <summary>
    /// Indique si les connexions échouées doivent être loguées.
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;
}
