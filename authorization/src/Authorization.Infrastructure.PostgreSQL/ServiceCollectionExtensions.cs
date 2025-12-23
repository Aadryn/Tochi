using Authorization.Infrastructure.PostgreSQL.Configuration;
using Authorization.Infrastructure.PostgreSQL.Data;
using Authorization.Infrastructure.PostgreSQL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Infrastructure.PostgreSQL;

/// <summary>
/// Extensions pour l'enregistrement des services PostgreSQL d'audit.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ajoute les services PostgreSQL d'audit au conteneur DI.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configuration">Configuration.</param>
    /// <returns>Collection de services pour chaînage.</returns>
    public static IServiceCollection AddPostgreSqlAudit(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = new PostgreSqlAuditOptions();
        configuration.GetSection(PostgreSqlAuditOptions.SectionName).Bind(options);

        services.Configure<PostgreSqlAuditOptions>(
            configuration.GetSection(PostgreSqlAuditOptions.SectionName));

        services.AddDbContext<AuthorizationAuditDbContext>(dbOptions =>
        {
            dbOptions.UseNpgsql(options.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(options.CommandTimeout);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: options.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromMilliseconds(options.RetryDelayMs * Math.Pow(2, options.MaxRetryCount)),
                    errorCodesToAdd: null);
            });
        });

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        return services;
    }

    /// <summary>
    /// Ajoute les services PostgreSQL d'audit avec configuration personnalisée.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="connectionString">Chaîne de connexion.</param>
    /// <param name="configure">Action de configuration optionnelle.</param>
    /// <returns>Collection de services pour chaînage.</returns>
    public static IServiceCollection AddPostgreSqlAudit(
        this IServiceCollection services,
        string connectionString,
        Action<PostgreSqlAuditOptions>? configure = null)
    {
        var options = new PostgreSqlAuditOptions { ConnectionString = connectionString };
        configure?.Invoke(options);

        services.Configure<PostgreSqlAuditOptions>(opt =>
        {
            opt.ConnectionString = options.ConnectionString;
            opt.Schema = options.Schema;
            opt.RetentionDays = options.RetentionDays;
            opt.AsyncWrite = options.AsyncWrite;
            opt.BatchSize = options.BatchSize;
            opt.FlushIntervalSeconds = options.FlushIntervalSeconds;
            opt.CommandTimeout = options.CommandTimeout;
            opt.MaxRetryCount = options.MaxRetryCount;
            opt.RetryDelayMs = options.RetryDelayMs;
        });

        services.AddDbContext<AuthorizationAuditDbContext>(dbOptions =>
        {
            dbOptions.UseNpgsql(options.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(options.CommandTimeout);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: options.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromMilliseconds(options.RetryDelayMs * Math.Pow(2, options.MaxRetryCount)),
                    errorCodesToAdd: null);
            });
        });

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        return services;
    }
}
