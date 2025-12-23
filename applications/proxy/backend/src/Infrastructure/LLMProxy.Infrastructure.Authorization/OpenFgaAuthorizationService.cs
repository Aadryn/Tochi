using LLMProxy.Infrastructure.Authorization.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenFga.Sdk.Client;
using OpenFga.Sdk.Client.Model;
using OpenFga.Sdk.Model;

namespace LLMProxy.Infrastructure.Authorization;

/// <summary>
/// Implémentation du service d'autorisation utilisant OpenFGA.
/// </summary>
/// <remarks>
/// <para>
/// OpenFGA est un système d'autorisation basé sur les relations (ReBAC) inspiré de
/// Google Zanzibar. Il permet de définir des autorisations sous forme de tuples
/// (utilisateur, relation, objet).
/// </para>
/// <para>
/// Voir ADR-055 pour les détails de l'architecture d'autorisation.
/// </para>
/// </remarks>
public sealed class OpenFgaAuthorizationService : IAuthorizationService, IDisposable
{
    private readonly OpenFgaClient _client;
    private readonly OpenFgaConfiguration _config;
    private readonly ILogger<OpenFgaAuthorizationService> _logger;
    private bool _disposed;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="OpenFgaAuthorizationService"/>.
    /// </summary>
    /// <param name="options">Options de configuration OpenFGA.</param>
    /// <param name="logger">Logger pour les traces.</param>
    public OpenFgaAuthorizationService(
        IOptions<OpenFgaConfiguration> options,
        ILogger<OpenFgaAuthorizationService> logger)
    {
        _config = options.Value;
        _logger = logger;

        _config.Validate();

        var clientConfig = new ClientConfiguration
        {
            ApiUrl = _config.ApiUrl,
            StoreId = _config.StoreId,
            AuthorizationModelId = _config.AuthorizationModelId,
            MaxRetry = _config.MaxRetries
        };

        _client = new OpenFgaClient(clientConfig);

        _logger.LogInformation(
            "Service OpenFGA initialisé - URL: {ApiUrl}, Store: {StoreId}",
            _config.ApiUrl,
            _config.StoreId);
    }

    /// <inheritdoc />
    public async Task<AuthorizationResult> CheckAsync(
        AuthorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_config.Enabled)
        {
            _logger.LogDebug("Service d'autorisation désactivé - autorisation accordée par défaut");
            return AuthorizationResult.Allowed;
        }

        try
        {
            var checkRequest = new ClientCheckRequest
            {
                User = request.GetFullUser(),
                Relation = request.Relation,
                Object = request.GetFullObject()
            };

            _logger.LogDebug(
                "Vérification autorisation: {User} {Relation} {Object}",
                checkRequest.User,
                checkRequest.Relation,
                checkRequest.Object);

            var response = await _client.Check(checkRequest, null, cancellationToken)
                .ConfigureAwait(false);

            if (response.Allowed == true)
            {
                _logger.LogDebug(
                    "Autorisation accordée: {User} {Relation} {Object}",
                    checkRequest.User,
                    checkRequest.Relation,
                    checkRequest.Object);

                return AuthorizationResult.Allowed;
            }

            _logger.LogDebug(
                "Autorisation refusée: {User} {Relation} {Object}",
                checkRequest.User,
                checkRequest.Relation,
                checkRequest.Object);

            return AuthorizationResult.NoRelation(
                request.Relation,
                request.ObjectType,
                request.ObjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erreur lors de la vérification d'autorisation: {User} {Relation} {Object}",
                request.GetFullUser(),
                request.Relation,
                request.GetFullObject());

            return HandleError(ex);
        }
    }

    /// <inheritdoc />
    public Task<AuthorizationResult> CheckAsync(
        string userId,
        string relation,
        string objectType,
        string objectId,
        CancellationToken cancellationToken = default)
    {
        var request = new AuthorizationRequest(userId, relation, objectType, objectId);
        return CheckAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddRelationAsync(
        string userId,
        string relation,
        string objectType,
        string objectId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(relation);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectId);

        if (!_config.Enabled)
        {
            _logger.LogDebug("Service d'autorisation désactivé - opération ignorée");
            return;
        }

        try
        {
            var fullUser = userId.Contains(':') ? userId : $"user:{userId}";
            var fullObject = $"{objectType}:{objectId}";

            var writeRequest = new ClientWriteRequest
            {
                Writes = new List<ClientTupleKey>
                {
                    new()
                    {
                        User = fullUser,
                        Relation = relation,
                        Object = fullObject
                    }
                }
            };

            await _client.Write(writeRequest, null, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "Relation créée: {User} {Relation} {Object}",
                fullUser,
                relation,
                fullObject);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erreur lors de la création de relation: {UserId} {Relation} {ObjectType}:{ObjectId}",
                userId,
                relation,
                objectType,
                objectId);

            throw;
        }
    }

    /// <inheritdoc />
    public async Task RemoveRelationAsync(
        string userId,
        string relation,
        string objectType,
        string objectId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(relation);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectId);

        if (!_config.Enabled)
        {
            _logger.LogDebug("Service d'autorisation désactivé - opération ignorée");
            return;
        }

        try
        {
            var fullUser = userId.Contains(':') ? userId : $"user:{userId}";
            var fullObject = $"{objectType}:{objectId}";

            var writeRequest = new ClientWriteRequest
            {
                Deletes = new List<ClientTupleKeyWithoutCondition>
                {
                    new()
                    {
                        User = fullUser,
                        Relation = relation,
                        Object = fullObject
                    }
                }
            };

            await _client.Write(writeRequest, null, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "Relation supprimée: {User} {Relation} {Object}",
                fullUser,
                relation,
                fullObject);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erreur lors de la suppression de relation: {UserId} {Relation} {ObjectType}:{ObjectId}",
                userId,
                relation,
                objectType,
                objectId);

            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListObjectsAsync(
        string userId,
        string relation,
        string objectType,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(relation);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);

        if (!_config.Enabled)
        {
            _logger.LogDebug("Service d'autorisation désactivé - liste vide retournée");
            return Array.Empty<string>();
        }

        try
        {
            var fullUser = userId.Contains(':') ? userId : $"user:{userId}";

            var listRequest = new ClientListObjectsRequest
            {
                User = fullUser,
                Relation = relation,
                Type = objectType
            };

            var response = await _client.ListObjects(listRequest, null, cancellationToken)
                .ConfigureAwait(false);

            var objects = response.Objects?
                .Select(o => o.Replace($"{objectType}:", string.Empty))
                .ToList()
                ?? new List<string>();

            _logger.LogDebug(
                "ListObjects: {User} {Relation} {Type} -> {Count} objets",
                fullUser,
                relation,
                objectType,
                objects.Count);

            return objects;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erreur lors de la liste des objets: {UserId} {Relation} {ObjectType}",
                userId,
                relation,
                objectType);

            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Gère les erreurs selon le mode de fallback configuré.
    /// </summary>
    private AuthorizationResult HandleError(Exception ex)
    {
        return _config.FallbackMode switch
        {
            FallbackMode.Allow => AuthorizationResult.Allowed,
            FallbackMode.Deny => AuthorizationResult.Error(ex.Message),
            _ => AuthorizationResult.Error(ex.Message)
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _client.Dispose();
        _disposed = true;
    }
}
