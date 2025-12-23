using FluentAssertions;
using LLMProxy.Application.ApiTransformation.Ollama;
using LLMProxy.Application.ApiTransformation.Ollama.Contracts;
using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.Tests.ApiTransformation;

/// <summary>
/// Tests unitaires pour OllamaRequestTransformer.
/// </summary>
public sealed class OllamaRequestTransformerTests
{
    private readonly OllamaRequestTransformer _sut = new();

    [Fact]
    public void SupportedFormat_ReturnsOllama()
    {
        // Act
        var format = _sut.SupportedFormat;

        // Assert
        format.Should().Be(ApiFormat.Ollama);
    }

    [Fact]
    public void TransformChatRequest_WithValidRequest_ReturnsCanonicalRequest()
    {
        // Arrange
        var request = new OllamaChatRequest
        {
            Model = "llama3.1",
            Messages = new[]
            {
                new OllamaMessage { Role = "system", Content = "You are a helpful assistant." },
                new OllamaMessage { Role = "user", Content = "Hello!" }
            }
        };

        // Act
        var result = _sut.TransformChatRequest(request);

        // Assert
        result.Model.Value.Should().Be("llama3.1");
        result.Messages.Should().HaveCount(2);
        result.Messages[0].Role.Should().Be(MessageRole.System);
        result.Messages[0].Content.Should().Be("You are a helpful assistant.");
        result.Messages[1].Role.Should().Be(MessageRole.User);
        result.Messages[1].Content.Should().Be("Hello!");
    }

    [Fact]
    public void TransformChatRequest_WithOptions_MapsOptionsCorrectly()
    {
        // Arrange
        var request = new OllamaChatRequest
        {
            Model = "llama3.1",
            Messages = new[]
            {
                new OllamaMessage { Role = "user", Content = "Hello!" }
            },
            Options = new OllamaOptions
            {
                Temperature = 0.8m,
                NumPredict = 500,
                TopK = 40,
                TopP = 0.9m
            }
        };

        // Act
        var result = _sut.TransformChatRequest(request);

        // Assert
        result.Temperature.Should().Be(0.8m);
        result.MaxTokens.Should().Be(500);
        result.TopP.Should().Be(0.9m);
    }

    [Fact]
    public void TransformChatRequest_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _sut.TransformChatRequest(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TransformChatRequest_WithWrongType_ThrowsArgumentException()
    {
        // Act
        var act = () => _sut.TransformChatRequest("invalid");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TransformEmbeddingRequest_WithSingleInput_ReturnsCanonicalRequest()
    {
        // Arrange
        var request = new OllamaEmbeddingRequest
        {
            Model = "nomic-embed-text",
            Input = "Hello world"
        };

        // Act
        var result = _sut.TransformEmbeddingRequest(request);

        // Assert
        result.Model.Value.Should().Be("nomic-embed-text");
        result.Inputs.Should().HaveCount(1);
        result.Inputs[0].Should().Be("Hello world");
    }

    [Fact]
    public void TransformEmbeddingRequest_WithMultipleInputs_ReturnsCanonicalRequest()
    {
        // Arrange
        var request = new OllamaEmbeddingRequest
        {
            Model = "nomic-embed-text",
            Input = new[] { "Hello", "World" }
        };

        // Act
        var result = _sut.TransformEmbeddingRequest(request);

        // Assert
        result.Inputs.Should().HaveCount(2);
        result.Inputs[0].Should().Be("Hello");
        result.Inputs[1].Should().Be("World");
    }
}
