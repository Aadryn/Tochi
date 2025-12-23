using FluentAssertions;
using LLMProxy.Application.ApiTransformation;
using LLMProxy.Domain.LLM;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace LLMProxy.Application.Tests.ApiTransformation;

/// <summary>
/// Tests unitaires pour ApiFormatDetector.
/// </summary>
public sealed class ApiFormatDetectorTests
{
    private readonly ApiFormatDetector _sut = new();

    #region DetectFromPath Tests

    [Theory]
    [InlineData("/v1/chat/completions")]
    [InlineData("/v1/embeddings")]
    [InlineData("/v1/models")]
    [InlineData("/V1/chat/completions")]
    public void DetectFromPath_WithOpenAIPath_ReturnsOpenAI(string path)
    {
        // Act
        var result = _sut.DetectFromPath(path);

        // Assert
        result.Should().Be(ApiFormat.OpenAI);
    }

    [Theory]
    [InlineData("/api/chat")]
    [InlineData("/api/generate")]
    [InlineData("/api/embeddings")]
    [InlineData("/api/embed")]
    [InlineData("/api/tags")]
    [InlineData("/api/show")]
    [InlineData("/API/chat")]
    public void DetectFromPath_WithOllamaPath_ReturnsOllama(string path)
    {
        // Act
        var result = _sut.DetectFromPath(path);

        // Assert
        result.Should().Be(ApiFormat.Ollama);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/unknown/endpoint")]
    [InlineData("")]
    public void DetectFromPath_WithUnknownPath_ReturnsOpenAIDefault(string path)
    {
        // Act
        var result = _sut.DetectFromPath(path);

        // Assert
        // Default is OpenAI as per implementation
        result.Should().Be(ApiFormat.OpenAI);
    }

    #endregion

    #region TryDetectFromHeader Tests

    [Theory]
    [InlineData("openai")]
    [InlineData("OpenAI")]
    [InlineData("OPENAI")]
    public void TryDetectFromHeader_WithOpenAIHeader_ReturnsTrueAndOpenAI(string headerValue)
    {
        // Arrange
        var request = CreateMockRequest("/unknown", headerValue);

        // Act
        var success = _sut.TryDetectFromHeader(request, out var format);

        // Assert
        success.Should().BeTrue();
        format.Should().Be(ApiFormat.OpenAI);
    }

    [Theory]
    [InlineData("ollama")]
    [InlineData("Ollama")]
    [InlineData("OLLAMA")]
    public void TryDetectFromHeader_WithOllamaHeader_ReturnsTrueAndOllama(string headerValue)
    {
        // Arrange
        var request = CreateMockRequest("/unknown", headerValue);

        // Act
        var success = _sut.TryDetectFromHeader(request, out var format);

        // Assert
        success.Should().BeTrue();
        format.Should().Be(ApiFormat.Ollama);
    }

    [Fact]
    public void TryDetectFromHeader_WithMissingHeader_ReturnsFalse()
    {
        // Arrange
        var request = CreateMockRequest("/unknown", null);

        // Act
        var success = _sut.TryDetectFromHeader(request, out var format);

        // Assert
        success.Should().BeFalse();
    }

    [Fact]
    public void TryDetectFromHeader_WithInvalidHeader_ReturnsFalse()
    {
        // Arrange
        var request = CreateMockRequest("/unknown", "invalid");

        // Act
        var success = _sut.TryDetectFromHeader(request, out var format);

        // Assert
        success.Should().BeFalse();
    }

    #endregion

    #region DetectFormat Tests

    [Fact]
    public void DetectFormat_WithOpenAIPath_ReturnsOpenAI()
    {
        // Arrange
        var request = CreateMockRequest("/v1/chat/completions", null);

        // Act
        var result = _sut.DetectFormat(request);

        // Assert
        result.Should().Be(ApiFormat.OpenAI);
    }

    [Fact]
    public void DetectFormat_WithOllamaPath_ReturnsOllama()
    {
        // Arrange
        var request = CreateMockRequest("/api/chat", null);

        // Act
        var result = _sut.DetectFormat(request);

        // Assert
        result.Should().Be(ApiFormat.Ollama);
    }

    [Fact]
    public void DetectFormat_WithHeaderOverridingUnknownPath_ReturnsHeaderFormat()
    {
        // Arrange - unknown path but header says ollama
        var request = CreateMockRequest("/unknown", "ollama");

        // Act
        var result = _sut.DetectFormat(request);

        // Assert
        // Header takes precedence when path doesn't match known patterns
        result.Should().Be(ApiFormat.Ollama);
    }

    [Fact]
    public void DetectFormat_WithUnknownPathAndNoHeader_ReturnsDefaultOpenAI()
    {
        // Arrange
        var request = CreateMockRequest("/unknown", null);

        // Act
        var result = _sut.DetectFormat(request);

        // Assert
        // Default is OpenAI
        result.Should().Be(ApiFormat.OpenAI);
    }

    [Fact]
    public void DetectFormat_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _sut.DetectFormat(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Helper Methods

    private static HttpRequest CreateMockRequest(string path, string? apiFormatHeader)
    {
        var request = Substitute.For<HttpRequest>();
        request.Path.Returns(new PathString(path));
        
        var headers = new HeaderDictionary();
        if (apiFormatHeader != null)
        {
            headers["X-Api-Format"] = apiFormatHeader;
        }
        request.Headers.Returns(headers);
        
        return request;
    }

    #endregion
}
