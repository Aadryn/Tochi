using FluentAssertions;
using LLMProxy.Application.ApiTransformation;
using LLMProxy.Application.ApiTransformation.Ollama;
using LLMProxy.Application.ApiTransformation.OpenAI;
using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.Tests.ApiTransformation;

/// <summary>
/// Tests unitaires pour TransformerFactory.
/// </summary>
public sealed class TransformerFactoryTests
{
    private readonly TransformerFactory _sut = new();

    [Theory]
    [InlineData(ApiFormat.OpenAI)]
    [InlineData(ApiFormat.Ollama)]
    public void IsFormatSupported_WithSupportedFormat_ReturnsTrue(ApiFormat format)
    {
        // Act
        var result = _sut.IsFormatSupported(format);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetRequestTransformer_WithOpenAI_ReturnsOpenAITransformer()
    {
        // Act
        var transformer = _sut.GetRequestTransformer(ApiFormat.OpenAI);

        // Assert
        transformer.Should().BeOfType<OpenAIRequestTransformer>();
        transformer.SupportedFormat.Should().Be(ApiFormat.OpenAI);
    }

    [Fact]
    public void GetRequestTransformer_WithOllama_ReturnsOllamaTransformer()
    {
        // Act
        var transformer = _sut.GetRequestTransformer(ApiFormat.Ollama);

        // Assert
        transformer.Should().BeOfType<OllamaRequestTransformer>();
        transformer.SupportedFormat.Should().Be(ApiFormat.Ollama);
    }

    [Fact]
    public void GetResponseTransformer_WithOpenAI_ReturnsOpenAITransformer()
    {
        // Act
        var transformer = _sut.GetResponseTransformer(ApiFormat.OpenAI);

        // Assert
        transformer.Should().BeOfType<OpenAIResponseTransformer>();
        transformer.SupportedFormat.Should().Be(ApiFormat.OpenAI);
    }

    [Fact]
    public void GetResponseTransformer_WithOllama_ReturnsOllamaTransformer()
    {
        // Act
        var transformer = _sut.GetResponseTransformer(ApiFormat.Ollama);

        // Assert
        transformer.Should().BeOfType<OllamaResponseTransformer>();
        transformer.SupportedFormat.Should().Be(ApiFormat.Ollama);
    }

    [Theory]
    [InlineData((ApiFormat)99)]
    [InlineData((ApiFormat)(-1))]
    public void GetRequestTransformer_WithUnsupportedFormat_ThrowsNotSupportedException(ApiFormat format)
    {
        // Act
        var act = () => _sut.GetRequestTransformer(format);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    [Theory]
    [InlineData((ApiFormat)99)]
    [InlineData((ApiFormat)(-1))]
    public void GetResponseTransformer_WithUnsupportedFormat_ThrowsNotSupportedException(ApiFormat format)
    {
        // Act
        var act = () => _sut.GetResponseTransformer(format);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    [Theory]
    [InlineData((ApiFormat)99)]
    [InlineData((ApiFormat)(-1))]
    public void IsFormatSupported_WithUnsupportedFormat_ReturnsFalse(ApiFormat format)
    {
        // Act
        var result = _sut.IsFormatSupported(format);

        // Assert
        result.Should().BeFalse();
    }
}
