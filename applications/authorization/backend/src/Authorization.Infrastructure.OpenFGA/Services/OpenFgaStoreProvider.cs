using System.Collections.Concurrent;
using Authorization.Domain.ValueObjects;
using Authorization.Infrastructure.OpenFGA.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenFga.Sdk.Client;
using OpenFga.Sdk.Client.Model;
using OpenFga.Sdk.Model;

namespace Authorization.Infrastructure.OpenFGA.Services;

/// <summary>
/// Fournisseur de clients OpenFGA avec gestion multi-tenant (1 store par tenant).
/// </summary>
public sealed class OpenFgaStoreProvider : IOpenFgaStoreProvider, IDisposable
{
    private readonly ILogger<OpenFgaStoreProvider> _logger;
    private readonly OpenFgaOptions _options;
    private readonly ConcurrentDictionary<string, (OpenFgaClient Client, string StoreId, string ModelId)> _clients = new();
    private readonly OpenFgaClient _adminClient;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// Modèle d'autorisation par défaut.
    /// </summary>
    private static readonly string DefaultAuthorizationModel = """
        model
          schema 1.1

        type user

        type group
          relations
            define member: [user, group#member]

        type serviceaccount

        type scope
          relations
            define owner: [user, group#member, serviceaccount]
            define contributor: [user, group#member, serviceaccount] or owner
            define reader: [user, group#member, serviceaccount] or contributor

            define can_manage: owner
            define can_write: contributor
            define can_read: reader
        """;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="options">Options OpenFGA.</param>
    public OpenFgaStoreProvider(
        ILogger<OpenFgaStoreProvider> logger,
        IOptions<OpenFgaOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        var config = new ClientConfiguration
        {
            ApiUrl = _options.ApiUrl
        };

        _adminClient = new OpenFgaClient(config);
    }

    /// <inheritdoc />
    public async Task<OpenFgaClient> GetClientAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var tenantKey = tenantId.Value;

        if (_clients.TryGetValue(tenantKey, out var cached))
        {
            return cached.Client;
        }

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check après acquisition du lock
            if (_clients.TryGetValue(tenantKey, out cached))
            {
                return cached.Client;
            }

            var storeId = await GetOrCreateStoreAsync(tenantId, cancellationToken);
            var modelId = await GetOrCreateModelAsync(tenantId, storeId, cancellationToken);

            var config = new ClientConfiguration
            {
                ApiUrl = _options.ApiUrl,
                StoreId = storeId,
                AuthorizationModelId = modelId
            };

            var client = new OpenFgaClient(config);
            _clients[tenantKey] = (client, storeId, modelId);

            _logger.LogInformation(
                "OpenFGA client initialized for tenant {TenantId} with store {StoreId}",
                tenantId, storeId);

            return client;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<string> CreateStoreAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var storeName = tenantId.ToOpenFgaStoreName();

        _logger.LogInformation("Creating OpenFGA store: {StoreName}", storeName);

        var response = await _adminClient.CreateStore(new ClientCreateStoreRequest
        {
            Name = storeName
        }, cancellationToken: cancellationToken);

        var storeId = response.Id;

        _logger.LogInformation(
            "OpenFGA store created: {StoreName} -> {StoreId}",
            storeName, storeId);

        return storeId;
    }

    /// <inheritdoc />
    public async Task<string?> GetStoreIdAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var tenantKey = tenantId.Value;

        if (_clients.TryGetValue(tenantKey, out var cached))
        {
            return cached.StoreId;
        }

        var storeName = tenantId.ToOpenFgaStoreName();
        var request = new ClientListStoresRequest();
        var stores = await _adminClient.ListStores(request, cancellationToken: cancellationToken);

        return stores.Stores?.FirstOrDefault(s => s.Name == storeName)?.Id;
    }

    /// <inheritdoc />
    public async Task InitializeModelAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var storeId = await GetStoreIdAsync(tenantId, cancellationToken);
        if (storeId is null)
        {
            throw new InvalidOperationException($"Store not found for tenant {tenantId}");
        }

        await CreateAuthorizationModelAsync(storeId, cancellationToken);

        _logger.LogInformation(
            "Authorization model initialized for tenant {TenantId}",
            tenantId);
    }

    /// <inheritdoc />
    public async Task DeleteStoreAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var tenantKey = tenantId.Value;
        var storeId = await GetStoreIdAsync(tenantId, cancellationToken);

        if (storeId is null)
        {
            _logger.LogWarning("Store not found for tenant {TenantId}", tenantId);
            return;
        }

        var config = new ClientConfiguration
        {
            ApiUrl = _options.ApiUrl,
            StoreId = storeId
        };

        var client = new OpenFgaClient(config);
        await client.DeleteStore(cancellationToken: cancellationToken);

        _clients.TryRemove(tenantKey, out _);

        _logger.LogInformation(
            "OpenFGA store deleted for tenant {TenantId}: {StoreId}",
            tenantId, storeId);
    }

    private async Task<string> GetOrCreateStoreAsync(TenantId tenantId, CancellationToken cancellationToken)
    {
        var storeId = await GetStoreIdAsync(tenantId, cancellationToken);
        if (storeId is not null)
        {
            return storeId;
        }

        return await CreateStoreAsync(tenantId, cancellationToken);
    }

    private async Task<string> GetOrCreateModelAsync(TenantId tenantId, string storeId, CancellationToken cancellationToken)
    {
        var config = new ClientConfiguration
        {
            ApiUrl = _options.ApiUrl,
            StoreId = storeId
        };

        var client = new OpenFgaClient(config);

        try
        {
            var models = await client.ReadAuthorizationModels(cancellationToken: cancellationToken);
            var latestModel = models.AuthorizationModels?.OrderByDescending(m => m.Id).FirstOrDefault();

            if (latestModel is not null)
            {
                return latestModel.Id!;
            }
        }
        catch
        {
            // Store existe mais pas de modèle
        }

        return await CreateAuthorizationModelAsync(storeId, cancellationToken);
    }

    private async Task<string> CreateAuthorizationModelAsync(string storeId, CancellationToken cancellationToken)
    {
        var config = new ClientConfiguration
        {
            ApiUrl = _options.ApiUrl,
            StoreId = storeId
        };

        var client = new OpenFgaClient(config);

        // Utiliser le modèle depuis les options ou le modèle par défaut
        var modelDsl = DefaultAuthorizationModel;
        if (!string.IsNullOrEmpty(_options.AuthorizationModelPath) && File.Exists(_options.AuthorizationModelPath))
        {
            modelDsl = await File.ReadAllTextAsync(_options.AuthorizationModelPath, cancellationToken);
        }

        // Convertir DSL en JSON (simplification - en production utiliser openfga/language)
        var typeDefinitions = ParseDslToTypeDefinitions(modelDsl);

        var response = await client.WriteAuthorizationModel(new ClientWriteAuthorizationModelRequest
        {
            SchemaVersion = "1.1",
            TypeDefinitions = typeDefinitions
        }, cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Authorization model created for store {StoreId}: {ModelId}",
            storeId, response.AuthorizationModelId);

        return response.AuthorizationModelId;
    }

    /// <summary>
    /// Parse simplifié du DSL vers les définitions de types.
    /// En production, utiliser le package officiel @openfga/syntax-transformer.
    /// </summary>
    private static List<TypeDefinition> ParseDslToTypeDefinitions(string dsl)
    {
        // Implémentation simplifiée pour le modèle standard
        return new List<TypeDefinition>
        {
            new() { Type = "user" },
            new()
            {
                Type = "group",
                Relations = new Dictionary<string, Userset>
                {
                    ["member"] = new()
                    {
                        This = new object()
                    }
                },
                Metadata = new Metadata
                {
                    Relations = new Dictionary<string, RelationMetadata>
                    {
                        ["member"] = new()
                        {
                            DirectlyRelatedUserTypes = new List<RelationReference>
                            {
                                new() { Type = "user" },
                                new() { Type = "group", Relation = "member" }
                            }
                        }
                    }
                }
            },
            new() { Type = "serviceaccount" },
            new()
            {
                Type = "scope",
                Relations = new Dictionary<string, Userset>
                {
                    ["owner"] = new() { This = new object() },
                    ["contributor"] = new()
                    {
                        Union = new Usersets
                        {
                            Child = new List<Userset>
                            {
                                new() { This = new object() },
                                new() { ComputedUserset = new ObjectRelation { Relation = "owner" } }
                            }
                        }
                    },
                    ["reader"] = new()
                    {
                        Union = new Usersets
                        {
                            Child = new List<Userset>
                            {
                                new() { This = new object() },
                                new() { ComputedUserset = new ObjectRelation { Relation = "contributor" } }
                            }
                        }
                    },
                    ["can_manage"] = new() { ComputedUserset = new ObjectRelation { Relation = "owner" } },
                    ["can_write"] = new() { ComputedUserset = new ObjectRelation { Relation = "contributor" } },
                    ["can_read"] = new() { ComputedUserset = new ObjectRelation { Relation = "reader" } }
                },
                Metadata = new Metadata
                {
                    Relations = new Dictionary<string, RelationMetadata>
                    {
                        ["owner"] = new()
                        {
                            DirectlyRelatedUserTypes = new List<RelationReference>
                            {
                                new() { Type = "user" },
                                new() { Type = "group", Relation = "member" },
                                new() { Type = "serviceaccount" }
                            }
                        },
                        ["contributor"] = new()
                        {
                            DirectlyRelatedUserTypes = new List<RelationReference>
                            {
                                new() { Type = "user" },
                                new() { Type = "group", Relation = "member" },
                                new() { Type = "serviceaccount" }
                            }
                        },
                        ["reader"] = new()
                        {
                            DirectlyRelatedUserTypes = new List<RelationReference>
                            {
                                new() { Type = "user" },
                                new() { Type = "group", Relation = "member" },
                                new() { Type = "serviceaccount" }
                            }
                        }
                    }
                }
            }
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        _initLock.Dispose();
        _adminClient.Dispose();

        foreach (var (_, value) in _clients)
        {
            value.Client.Dispose();
        }

        _clients.Clear();
        _disposed = true;
    }
}
