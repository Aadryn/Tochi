using FluentAssertions;
using LLMProxy.Application.Common.NullObjects;
using Xunit;

namespace LLMProxy.Application.Tests.Common.NullObjects;

/// <summary>
/// Tests unitaires pour NoCacheService.
/// Valide le pattern Null Object pour ICacheService.
/// </summary>
public sealed class NoCacheServiceTests
{
    [Fact]
    public void Instance_Should_ReturnSingletonInstance()
    {
        // Arrange & Act
        var instance1 = NoCacheService.Instance;
        var instance2 = NoCacheService.Instance;

        // Assert
        instance1.Should().BeSameAs(instance2, "NoCacheService doit être un singleton");
        instance1.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAsync_Should_AlwaysReturnNull()
    {
        // Arrange
        var cache = NoCacheService.Instance;

        // Act
        var result = await cache.GetAsync<string>("test-key");

        // Assert
        result.Should().BeNull("NoCacheService retourne toujours null (cache miss)");
    }

    [Fact]
    public async Task GetAsync_WithDifferentTypes_Should_AlwaysReturnNull()
    {
        // Arrange
        var cache = NoCacheService.Instance;

        // Act
        var stringResult = await cache.GetAsync<string>("key1");
        var listResult = await cache.GetAsync<List<int>>("key2");
        var objectResult = await cache.GetAsync<TestObject>("key3");

        // Assert
        stringResult.Should().BeNull();
        listResult.Should().BeNull();
        objectResult.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_Should_DoNothing_WithoutException()
    {
        // Arrange
        var cache = NoCacheService.Instance;
        var value = "test-value";

        // Act
        Func<Task> action = async () => await cache.SetAsync("test-key", value);

        // Assert
        await action.Should().NotThrowAsync("SetAsync ne doit jamais lever d'exception");
    }

    [Fact]
    public async Task SetAsync_WithExpiration_Should_DoNothing_WithoutException()
    {
        // Arrange
        var cache = NoCacheService.Instance;
        var value = new TestObject { Id = 1, Name = "Test" };

        // Act
        Func<Task> action = async () => 
            await cache.SetAsync("test-key", value, TimeSpan.FromMinutes(10));

        // Assert
        await action.Should().NotThrowAsync("SetAsync avec expiration fonctionne sans erreur");
    }

    [Fact]
    public async Task ExistsAsync_Should_AlwaysReturnFalse()
    {
        // Arrange
        var cache = NoCacheService.Instance;

        // Act
        var exists = await cache.ExistsAsync("test-key");

        // Assert
        exists.Should().BeFalse("NoCacheService n'a jamais de clés (cache désactivé)");
    }

    [Fact]
    public async Task ExistsAsync_AfterSet_Should_StillReturnFalse()
    {
        // Arrange
        var cache = NoCacheService.Instance;
        await cache.SetAsync("test-key", "test-value");

        // Act
        var exists = await cache.ExistsAsync("test-key");

        // Assert
        exists.Should().BeFalse("SetAsync ne stocke rien, ExistsAsync reste false");
    }

    [Fact]
    public async Task RemoveAsync_Should_DoNothing_WithoutException()
    {
        // Arrange
        var cache = NoCacheService.Instance;

        // Act
        Func<Task> action = async () => await cache.RemoveAsync("test-key");

        // Assert
        await action.Should().NotThrowAsync("RemoveAsync ne doit jamais lever d'exception");
    }

    [Fact]
    public async Task RemoveByPatternAsync_Should_DoNothing_WithoutException()
    {
        // Arrange
        var cache = NoCacheService.Instance;

        // Act
        Func<Task> action = async () => await cache.RemoveByPatternAsync("user:*");

        // Assert
        await action.Should().NotThrowAsync("RemoveByPatternAsync fonctionne sans erreur");
    }

    [Theory]
    [InlineData("/api/chat", "{\"message\":\"hello\"}", false, "exact:/api/chat:")]
    [InlineData("/api/embeddings", "{\"input\":\"test\"}", true, "semantic:/api/embeddings:")]
    [InlineData("/api/completion", "", false, "exact:/api/completion:")]
    public void GenerateCacheKey_Should_ReturnValidKey(
        string endpoint,
        string requestBody,
        bool semantic,
        string expectedPrefix)
    {
        // Arrange
        var cache = NoCacheService.Instance;

        // Act
        var key = cache.GenerateCacheKey(endpoint, requestBody, semantic);

        // Assert
        key.Should().NotBeNullOrEmpty("GenerateCacheKey retourne toujours une clé valide");
        key.Should().StartWith(expectedPrefix, "Le préfixe doit correspondre au type (exact/semantic)");
        key.Should().Contain(endpoint, "La clé doit contenir l'endpoint");
    }

    [Fact]
    public void GenerateCacheKey_WithSameInput_Should_ReturnSameKey()
    {
        // Arrange
        var cache = NoCacheService.Instance;
        var endpoint = "/api/test";
        var requestBody = "{\"data\":\"value\"}";

        // Act
        var key1 = cache.GenerateCacheKey(endpoint, requestBody, semantic: false);
        var key2 = cache.GenerateCacheKey(endpoint, requestBody, semantic: false);

        // Assert
        key1.Should().Be(key2, "Les mêmes entrées produisent la même clé");
    }

    [Fact]
    public void GenerateCacheKey_WithDifferentSemanticFlag_Should_ReturnDifferentKeys()
    {
        // Arrange
        var cache = NoCacheService.Instance;
        var endpoint = "/api/test";
        var requestBody = "{\"data\":\"value\"}";

        // Act
        var exactKey = cache.GenerateCacheKey(endpoint, requestBody, semantic: false);
        var semanticKey = cache.GenerateCacheKey(endpoint, requestBody, semantic: true);

        // Assert
        exactKey.Should().NotBe(semanticKey, "exact et semantic génèrent des clés différentes");
        exactKey.Should().StartWith("exact:");
        semanticKey.Should().StartWith("semantic:");
    }

    [Fact]
    public async Task CompleteWorkflow_Should_WorkWithoutException()
    {
        // Arrange
        var cache = NoCacheService.Instance;
        var key = "workflow-key";
        var value = new TestObject { Id = 42, Name = "Workflow Test" };

        // Act & Assert
        Func<Task> action = async () =>
        {
            // Set
            await cache.SetAsync(key, value, TimeSpan.FromMinutes(5));

            // Check existence
            var exists = await cache.ExistsAsync(key);
            exists.Should().BeFalse();

            // Try to get
            var retrieved = await cache.GetAsync<TestObject>(key);
            retrieved.Should().BeNull();

            // Remove
            await cache.RemoveAsync(key);

            // Remove by pattern
            await cache.RemoveByPatternAsync("workflow-*");

            // Generate key
            var cacheKey = cache.GenerateCacheKey("/api/test", "{}", false);
            cacheKey.Should().NotBeNullOrEmpty();
        };

        await action.Should().NotThrowAsync("Le workflow complet fonctionne sans erreur");
    }

    [Fact]
    public async Task Cancellation_Should_BeRespected()
    {
        // Arrange
        var cache = NoCacheService.Instance;
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        // NoCacheService retourne immédiatement, donc le token annulé ne pose pas de problème
        Func<Task> getAction = async () => 
            await cache.GetAsync<string>("key", cts.Token);

        Func<Task> setAction = async () => 
            await cache.SetAsync("key", "value", cancellationToken: cts.Token);

        Func<Task> existsAction = async () => 
            await cache.ExistsAsync("key", cts.Token);

        // Assert - ne doit PAS lever d'exception car les opérations sont instantanées
        await getAction.Should().NotThrowAsync();
        await setAction.Should().NotThrowAsync();
        await existsAction.Should().NotThrowAsync();
    }

    /// <summary>
    /// Classe de test pour valider le fonctionnement avec des objets complexes.
    /// </summary>
    private sealed class TestObject
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
