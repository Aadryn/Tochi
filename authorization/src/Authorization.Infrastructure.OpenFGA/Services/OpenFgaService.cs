using Authorization.Domain.ValueObjects;
using Authorization.Infrastructure.OpenFGA.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenFga.Sdk.Client;
using OpenFga.Sdk.Client.Model;
using OpenFga.Sdk.Model;

namespace Authorization.Infrastructure.OpenFGA.Services;

/// <summary>
/// Implémentation du service OpenFGA.
/// </summary>
public sealed class OpenFgaService : IOpenFgaService
{
    private readonly ILogger<OpenFgaService> _logger;
    private readonly OpenFgaOptions _options;
    private readonly IOpenFgaStoreProvider _storeProvider;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="options">Options OpenFGA.</param>
    /// <param name="storeProvider">Fournisseur de stores par tenant.</param>
    public OpenFgaService(
        ILogger<OpenFgaService> logger,
        IOptions<OpenFgaOptions> options,
        IOpenFgaStoreProvider storeProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _storeProvider = storeProvider ?? throw new ArgumentNullException(nameof(storeProvider));
    }

    /// <inheritdoc />
    public async Task<bool> CheckAsync(
        TenantId tenantId,
        PrincipalId principalId,
        PrincipalType principalType,
        string relation,
        string objectType,
        string objectId,
        CancellationToken cancellationToken = default)
    {
        var client = await _storeProvider.GetClientAsync(tenantId, cancellationToken);
        var user = principalId.ToOpenFgaFormat(principalType);
        var @object = $"{objectType}:{objectId}";

        _logger.LogDebug(
            "OpenFGA Check: user={User}, relation={Relation}, object={Object}, tenant={TenantId}",
            user, relation, @object, tenantId);

        try
        {
            var response = await client.Check(new ClientCheckRequest
            {
                User = user,
                Relation = relation,
                Object = @object
            }, cancellationToken: cancellationToken);

            _logger.LogDebug(
                "OpenFGA Check result: allowed={Allowed}",
                response.Allowed);

            return response.Allowed ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "OpenFGA Check failed: user={User}, relation={Relation}, object={Object}",
                user, relation, @object);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task WriteAsync(
        TenantId tenantId,
        string user,
        string relation,
        string objectType,
        string objectId,
        CancellationToken cancellationToken = default)
    {
        var client = await _storeProvider.GetClientAsync(tenantId, cancellationToken);
        var @object = $"{objectType}:{objectId}";

        _logger.LogDebug(
            "OpenFGA Write: user={User}, relation={Relation}, object={Object}, tenant={TenantId}",
            user, relation, @object, tenantId);

        try
        {
            await client.Write(new ClientWriteRequest
            {
                Writes = new List<ClientTupleKey>
                {
                    new()
                    {
                        User = user,
                        Relation = relation,
                        Object = @object
                    }
                }
            }, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "OpenFGA tuple written: user={User}, relation={Relation}, object={Object}",
                user, relation, @object);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "OpenFGA Write failed: user={User}, relation={Relation}, object={Object}",
                user, relation, @object);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        TenantId tenantId,
        string user,
        string relation,
        string objectType,
        string objectId,
        CancellationToken cancellationToken = default)
    {
        var client = await _storeProvider.GetClientAsync(tenantId, cancellationToken);
        var @object = $"{objectType}:{objectId}";

        _logger.LogDebug(
            "OpenFGA Delete: user={User}, relation={Relation}, object={Object}, tenant={TenantId}",
            user, relation, @object, tenantId);

        try
        {
            await client.Write(new ClientWriteRequest
            {
                Deletes = new List<ClientTupleKeyWithoutCondition>
                {
                    new()
                    {
                        User = user,
                        Relation = relation,
                        Object = @object
                    }
                }
            }, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "OpenFGA tuple deleted: user={User}, relation={Relation}, object={Object}",
                user, relation, @object);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "OpenFGA Delete failed: user={User}, relation={Relation}, object={Object}",
                user, relation, @object);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListObjectsAsync(
        TenantId tenantId,
        PrincipalId principalId,
        PrincipalType principalType,
        string relation,
        string objectType,
        CancellationToken cancellationToken = default)
    {
        var client = await _storeProvider.GetClientAsync(tenantId, cancellationToken);
        var user = principalId.ToOpenFgaFormat(principalType);

        _logger.LogDebug(
            "OpenFGA ListObjects: user={User}, relation={Relation}, type={ObjectType}",
            user, relation, objectType);

        try
        {
            var response = await client.ListObjects(new ClientListObjectsRequest
            {
                User = user,
                Relation = relation,
                Type = objectType
            }, cancellationToken: cancellationToken);

            return response.Objects ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "OpenFGA ListObjects failed: user={User}, relation={Relation}, type={ObjectType}",
                user, relation, objectType);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListUsersAsync(
        TenantId tenantId,
        string relation,
        string objectType,
        string objectId,
        string userType,
        CancellationToken cancellationToken = default)
    {
        var client = await _storeProvider.GetClientAsync(tenantId, cancellationToken);
        var @object = $"{objectType}:{objectId}";

        _logger.LogDebug(
            "OpenFGA ListUsers: relation={Relation}, object={Object}, userType={UserType}",
            relation, @object, userType);

        try
        {
            var response = await client.ListUsers(new ClientListUsersRequest
            {
                Object = new FgaObject { Type = objectType, Id = objectId },
                Relation = relation,
                UserFilters = new List<UserTypeFilter>
                {
                    new() { Type = userType }
                }
            }, cancellationToken: cancellationToken);

            return response.Users?
                .Select(u => u.Object != null ? $"{u.Object.Type}:{u.Object.Id}" : u.Userset?.Type ?? "")
                .Where(u => !string.IsNullOrEmpty(u))
                .ToList() ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "OpenFGA ListUsers failed: relation={Relation}, object={Object}",
                relation, @object);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string> CreateStoreAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _storeProvider.CreateStoreAsync(tenantId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string?> GetStoreIdAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _storeProvider.GetStoreIdAsync(tenantId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task InitializeModelAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        await _storeProvider.InitializeModelAsync(tenantId, cancellationToken);
    }
}
