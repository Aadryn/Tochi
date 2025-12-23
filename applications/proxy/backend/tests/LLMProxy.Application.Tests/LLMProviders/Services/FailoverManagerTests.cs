using FluentAssertions;
using LLMProxy.Application.LLMProviders.Services.Failover;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LLMProxy.Application.Tests.LLMProviders.Services;

/// <summary>
/// Tests unitaires pour le gestionnaire de failover.
/// Vérifie le comportement du failover automatique entre providers.
/// </summary>
public sealed class FailoverManagerTests
{
    private readonly ILogger<FailoverManager> _logger;
    private readonly FailoverOptions _options;
    private readonly FailoverManager _sut;

    public FailoverManagerTests()
    {
        _logger = Substitute.For<ILogger<FailoverManager>>();
        _options = new FailoverOptions
        {
            FailuresBeforeBlacklist = 3,
            BlacklistDuration = TimeSpan.FromMinutes(5)
        };
        _sut = new FailoverManager(_logger, _options);
    }

    #region ExecuteWithFailoverAsync

    [Fact]
    public async Task ExecuteWithFailoverAsync_AvecListeVide_DoitRetournerEchec()
    {
        // Arrange
        var providers = Array.Empty<ILLMProviderClient>();
        Func<ILLMProviderClient, CancellationToken, Task<string>> operation = 
            (_, _) => Task.FromResult("success");

        // Act
        var result = await _sut.ExecuteWithFailoverAsync(providers, operation);

        // Assert
        result.Success.Should().BeFalse();
        result.Attempts.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteWithFailoverAsync_PremierProviderSucces_DoitRetournerSuccesImmediatement()
    {
        // Arrange
        var mockProvider = CreateMockProvider(ProviderType.OpenAI);
        var providers = new[] { mockProvider };
        Func<ILLMProviderClient, CancellationToken, Task<string>> operation = 
            (_, _) => Task.FromResult("success");

        // Act
        var result = await _sut.ExecuteWithFailoverAsync(providers, operation);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().Be("success");
        result.SuccessfulProvider.Should().Be(ProviderType.OpenAI);
        result.Attempts.Should().HaveCount(1);
        result.Attempts[0].Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteWithFailoverAsync_PremierProviderEchoue_DoitEssayerSecondProvider()
    {
        // Arrange
        var provider1 = CreateMockProvider(ProviderType.OpenAI);
        var provider2 = CreateMockProvider(ProviderType.Anthropic);
        var providers = new[] { provider1, provider2 };
        
        var callCount = 0;
        Func<ILLMProviderClient, CancellationToken, Task<string>> operation = (client, _) =>
        {
            callCount++;
            if (client.Type == ProviderType.OpenAI)
                throw new HttpRequestException("Provider unreachable");
            return Task.FromResult("success-from-anthropic");
        };

        // Act
        var result = await _sut.ExecuteWithFailoverAsync(providers, operation);

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().Be("success-from-anthropic");
        result.SuccessfulProvider.Should().Be(ProviderType.Anthropic);
        result.Attempts.Should().HaveCount(2);
        result.Attempts[0].Success.Should().BeFalse();
        result.Attempts[1].Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteWithFailoverAsync_TousProvidersEchouent_DoitRetournerEchec()
    {
        // Arrange
        var provider1 = CreateMockProvider(ProviderType.OpenAI);
        var provider2 = CreateMockProvider(ProviderType.Anthropic);
        var providers = new[] { provider1, provider2 };
        
        Func<ILLMProviderClient, CancellationToken, Task<string>> operation = 
            (_, _) => throw new Exception("All failed");

        // Act
        var result = await _sut.ExecuteWithFailoverAsync(providers, operation);

        // Assert
        result.Success.Should().BeFalse();
        result.Result.Should().BeNull();
        result.Attempts.Should().HaveCount(2);
        result.Attempts.Should().AllSatisfy(a => a.Success.Should().BeFalse());
    }

    [Fact]
    public async Task ExecuteWithFailoverAsync_IgnoreProvidersBlacklistes()
    {
        // Arrange
        var provider1 = CreateMockProvider(ProviderType.OpenAI);
        var provider2 = CreateMockProvider(ProviderType.Anthropic);
        var providers = new[] { provider1, provider2 };
        
        // Blacklister le premier provider
        _sut.BlacklistProvider(ProviderType.OpenAI, TimeSpan.FromMinutes(5), "Test blacklist");
        
        Func<ILLMProviderClient, CancellationToken, Task<string>> operation = 
            (_, _) => Task.FromResult("success");

        // Act
        var result = await _sut.ExecuteWithFailoverAsync(providers, operation);

        // Assert
        result.Success.Should().BeTrue();
        result.SuccessfulProvider.Should().Be(ProviderType.Anthropic);
        // Le premier provider blacklisté devrait être ignoré
        result.Attempts.Should().HaveCount(1);
    }

    #endregion

    #region Blacklist Management

    [Fact]
    public void IsBlacklisted_AvecNouveauProvider_DoitRetournerFaux()
    {
        // Act
        var result = _sut.IsBlacklisted(ProviderType.OpenAI);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void BlacklistProvider_AjouteALaBlacklist()
    {
        // Act
        _sut.BlacklistProvider(ProviderType.OpenAI, TimeSpan.FromMinutes(5), "Test reason");

        // Assert
        _sut.IsBlacklisted(ProviderType.OpenAI).Should().BeTrue();
    }

    [Fact]
    public void UnblacklistProvider_RetireDeLaBlacklist()
    {
        // Arrange
        _sut.BlacklistProvider(ProviderType.OpenAI, TimeSpan.FromMinutes(5), "Test reason");

        // Act
        _sut.UnblacklistProvider(ProviderType.OpenAI);

        // Assert
        _sut.IsBlacklisted(ProviderType.OpenAI).Should().BeFalse();
    }

    [Fact]
    public void IsBlacklisted_AvecBlacklistExpiree_DoitRetournerFaux()
    {
        // Arrange
        _sut.BlacklistProvider(ProviderType.OpenAI, TimeSpan.FromMilliseconds(1), "Test reason");
        
        // Attendre l'expiration
        Thread.Sleep(10);

        // Act
        var result = _sut.IsBlacklisted(ProviderType.OpenAI);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetBlacklistedProviders_RetourneTousLesBlacklistes()
    {
        // Arrange
        _sut.BlacklistProvider(ProviderType.OpenAI, TimeSpan.FromMinutes(5), "Reason 1");
        _sut.BlacklistProvider(ProviderType.Anthropic, TimeSpan.FromMinutes(5), "Reason 2");

        // Act
        var result = _sut.GetBlacklistedProviders();

        // Assert
        result.Should().HaveCount(2);
        result.Keys.Should().Contain(ProviderType.OpenAI);
        result.Keys.Should().Contain(ProviderType.Anthropic);
    }

    #endregion

    #region Helpers

    private static ILLMProviderClient CreateMockProvider(ProviderType type)
    {
        var mock = Substitute.For<ILLMProviderClient>();
        mock.Type.Returns(type);
        mock.Name.Returns(type.ToString());
        return mock;
    }

    #endregion
}
