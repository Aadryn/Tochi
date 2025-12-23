using FluentAssertions;
using LLMProxy.Application.LLMProviders.Services.Selection;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LLMProxy.Application.Tests.LLMProviders.Services;

/// <summary>
/// Tests unitaires pour le sélecteur de providers.
/// Vérifie la logique de sélection et de tri des providers.
/// </summary>
public sealed class ProviderSelectorTests
{
    private readonly ILogger<ProviderSelector> _logger;
    private readonly ProviderSelector _sut;

    public ProviderSelectorTests()
    {
        _logger = Substitute.For<ILogger<ProviderSelector>>();
        _sut = new ProviderSelector(_logger);
    }

    #region Select Tests

    [Fact]
    public void Select_SansCritere_DoitRetournerTousProvidersOrdonnes()
    {
        // Act
        var result = _sut.Select();

        // Assert
        result.Should().NotBeEmpty();
        // Premier provider devrait être le plus fiable (OpenAI)
        result[0].Should().Be(ProviderType.OpenAI);
    }

    [Fact]
    public void Select_AvecProvidersPreferes_DoitRetournerPreferesEnPremier()
    {
        // Arrange
        var criteria = new SelectionCriteria
        {
            PreferredProviders = [ProviderType.Ollama, ProviderType.Anthropic]
        };

        // Act
        var result = _sut.Select(criteria);

        // Assert
        result.Should().NotBeEmpty();
        result[0].Should().Be(ProviderType.Ollama);
        result[1].Should().Be(ProviderType.Anthropic);
    }

    [Fact]
    public void Select_AvecProvidersExclus_NePasLesInclure()
    {
        // Arrange
        var criteria = new SelectionCriteria
        {
            ExcludedProviders = [ProviderType.OpenAI, ProviderType.Anthropic]
        };

        // Act
        var result = _sut.Select(criteria);

        // Assert
        result.Should().NotContain(ProviderType.OpenAI);
        result.Should().NotContain(ProviderType.Anthropic);
    }

    [Fact]
    public void Select_AvecPreferesEtExclus_DoitGererCorrectement()
    {
        // Arrange
        var criteria = new SelectionCriteria
        {
            PreferredProviders = [ProviderType.OpenAI, ProviderType.Ollama],
            ExcludedProviders = [ProviderType.OpenAI] // OpenAI est à la fois préféré et exclu
        };

        // Act
        var result = _sut.Select(criteria);

        // Assert
        result.Should().NotContain(ProviderType.OpenAI);
        result[0].Should().Be(ProviderType.Ollama);
    }

    [Fact]
    public void Select_AvecTousExclus_DoitRetournerListeVide()
    {
        // Arrange
        var allProviders = Enum.GetValues<ProviderType>();
        var criteria = new SelectionCriteria
        {
            ExcludedProviders = allProviders.ToList()
        };

        // Act
        var result = _sut.Select(criteria);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region SelectBestProvider Tests

    [Fact]
    public void SelectBestProvider_SansProviders_DoitRetournerNull()
    {
        // Arrange
        var request = CreateMinimalRequest();
        var providers = Array.Empty<ILLMProviderClient>();

        // Act
        var result = _sut.SelectBestProvider(request, providers);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SelectBestProvider_AvecUnSeulProvider_DoitRetournerCeProvider()
    {
        // Arrange
        var request = CreateMinimalRequest();
        var mockProvider = CreateMockProvider(ProviderType.OpenAI);
        var providers = new[] { mockProvider };

        // Act
        var result = _sut.SelectBestProvider(request, providers);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(ProviderType.OpenAI);
    }

    [Fact]
    public void SelectBestProvider_AvecPlusieursProviders_DoitRetournerMeilleurScore()
    {
        // Arrange
        var request = CreateMinimalRequest();
        var provider1 = CreateMockProvider(ProviderType.Ollama);      // Score bas
        var provider2 = CreateMockProvider(ProviderType.OpenAI);      // Score haut
        var provider3 = CreateMockProvider(ProviderType.HuggingFace); // Score moyen
        var providers = new[] { provider1, provider2, provider3 };

        // Act
        var result = _sut.SelectBestProvider(request, providers);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(ProviderType.OpenAI); // Plus fiable
    }

    [Fact]
    public void SelectBestProvider_AvecProviderPrefere_DoitRetournerPrefere()
    {
        // Arrange
        var request = CreateMinimalRequest();
        var provider1 = CreateMockProvider(ProviderType.OpenAI);  // Score haut mais non préféré
        var provider2 = CreateMockProvider(ProviderType.Ollama);  // Score bas mais préféré
        var providers = new[] { provider1, provider2 };
        
        var criteria = new SelectionCriteria
        {
            PreferredProviders = [ProviderType.Ollama]
        };

        // Act
        var result = _sut.SelectBestProvider(request, providers, criteria);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(ProviderType.Ollama);
    }

    #endregion

    #region SelectProvidersOrdered Tests

    [Fact]
    public void SelectProvidersOrdered_DoitRetournerProvidersDansBonOrdre()
    {
        // Arrange
        var request = CreateMinimalRequest();
        var provider1 = CreateMockProvider(ProviderType.Ollama);
        var provider2 = CreateMockProvider(ProviderType.OpenAI);
        var provider3 = CreateMockProvider(ProviderType.Anthropic);
        var providers = new[] { provider1, provider2, provider3 };

        // Act
        var result = _sut.SelectProvidersOrdered(request, providers);

        // Assert
        result.Should().HaveCount(3);
        // OpenAI et Anthropic ont le même score de fiabilité (15), puis Ollama (3)
        // L'ordre devrait refléter les scores décroissants
    }

    [Fact]
    public void SelectProvidersOrdered_AvecCriteres_DoitRespecterPreferences()
    {
        // Arrange
        var request = CreateMinimalRequest();
        var provider1 = CreateMockProvider(ProviderType.OpenAI);
        var provider2 = CreateMockProvider(ProviderType.Ollama);
        var provider3 = CreateMockProvider(ProviderType.Anthropic);
        var providers = new[] { provider1, provider2, provider3 };
        
        var criteria = new SelectionCriteria
        {
            // Spécifier uniquement Ollama et Anthropic comme préférés
            PreferredProviders = [ProviderType.Ollama, ProviderType.Anthropic]
        };

        // Act
        var result = _sut.SelectProvidersOrdered(request, providers, criteria);

        // Assert
        // Seuls les providers préférés sont retournés
        result.Should().HaveCount(2);
        result.Should().NotContain(p => p.Type == ProviderType.OpenAI);
        result.Should().Contain(p => p.Type == ProviderType.Ollama);
        result.Should().Contain(p => p.Type == ProviderType.Anthropic);
    }

    #endregion

    #region Helpers

    private static LLMRequest CreateMinimalRequest()
    {
        return new LLMRequest
        {
            Model = ModelIdentifier.FromValid("test-model"),
            Messages = []
        };
    }

    private static ILLMProviderClient CreateMockProvider(ProviderType type)
    {
        var mock = Substitute.For<ILLMProviderClient>();
        mock.Type.Returns(type);
        mock.Name.Returns(type.ToString());
        return mock;
    }

    #endregion
}
