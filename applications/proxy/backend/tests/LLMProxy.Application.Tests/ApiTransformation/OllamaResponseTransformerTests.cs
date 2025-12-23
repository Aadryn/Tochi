using FluentAssertions;
using LLMProxy.Application.ApiTransformation.Ollama;
using LLMProxy.Application.ApiTransformation.Ollama.Contracts;
using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.Tests.ApiTransformation;

/// <summary>
/// Tests unitaires pour OllamaResponseTransformer.
/// </summary>
public sealed class OllamaResponseTransformerTests
{
    private readonly OllamaResponseTransformer _sut = new();

    [Fact]
    public void SupportedFormat_ReturnsOllama()
    {
        // Act
        var format = _sut.SupportedFormat;

        // Assert
        format.Should().Be(ApiFormat.Ollama);
    }

    [Fact]
    public void TransformChatResponse_WithValidResponse_ReturnsOllamaResponse()
    {
        // Arrange
        var response = new LLMResponse
        {
            Id = "chat-123",
            Model = ModelIdentifier.FromValid("llama3.1"),
            Content = "Hello! How can I help you?",
            FinishReason = FinishReason.Stop,
            Usage = TokenUsage.FromValid(10, 20)
        };

        // Act
        var result = _sut.TransformChatResponse(response);

        // Assert
        result.Should().BeOfType<OllamaChatResponse>();
        var ollamaResponse = (OllamaChatResponse)result;
        ollamaResponse.Model.Should().Be("llama3.1");
        ollamaResponse.Message.Should().NotBeNull();
        ollamaResponse.Message!.Content.Should().Be("Hello! How can I help you?");
        ollamaResponse.Message.Role.Should().Be("assistant");
        ollamaResponse.Done.Should().BeTrue();
        ollamaResponse.PromptEvalCount.Should().Be(10);
        ollamaResponse.EvalCount.Should().Be(20);
    }

    [Fact]
    public void TransformChatResponse_WithNullResponse_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _sut.TransformChatResponse(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TransformStreamChunk_WithContent_ReturnsJsonString()
    {
        // Arrange
        var response = new LLMResponse
        {
            Id = "chunk-123",
            Model = ModelIdentifier.FromValid("llama3.1"),
            Content = "Hello",
            IsStreamChunk = true,
            StreamIndex = 0
        };

        // Act
        var result = _sut.TransformStreamChunk(response);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\"model\":\"llama3.1\"");
        result.Should().Contain("\"content\":\"Hello\"");
    }

    [Fact]
    public void TransformEmbeddingResponse_WithValidResponse_ReturnsOllamaResponse()
    {
        // Arrange
        var response = new EmbeddingResponse
        {
            Model = ModelIdentifier.FromValid("nomic-embed-text"),
            Embeddings = new List<Embedding>
            {
                new Embedding
                {
                    Index = 0,
                    Vector = new[] { 0.1f, 0.2f, 0.3f }
                },
                new Embedding
                {
                    Index = 1,
                    Vector = new[] { 0.4f, 0.5f, 0.6f }
                }
            },
            Usage = TokenUsage.FromValid(5, 0)
        };

        // Act
        var result = _sut.TransformEmbeddingResponse(response);

        // Assert
        result.Should().BeOfType<OllamaEmbeddingResponse>();
        var ollamaResponse = (OllamaEmbeddingResponse)result;
        ollamaResponse.Model.Should().Be("nomic-embed-text");
        ollamaResponse.Embeddings.Should().HaveCount(2);
        ollamaResponse.Embeddings![0].Should().BeEquivalentTo(new[] { 0.1f, 0.2f, 0.3f });
        ollamaResponse.PromptEvalCount.Should().Be(5);
    }

    [Fact]
    public void TransformModelsResponse_WithValidModels_ReturnsOllamaTagsResponse()
    {
        // Arrange
        var models = new List<LLMModel>
        {
            new LLMModel
            {
                Id = ModelIdentifier.FromValid("llama3.1"),
                Name = "Llama 3.1"
            },
            new LLMModel
            {
                Id = ModelIdentifier.FromValid("mistral"),
                Name = "Mistral"
            }
        };

        // Act
        var result = _sut.TransformModelsResponse(models);

        // Assert
        result.Should().BeOfType<OllamaTagsResponse>();
        var tagsResponse = (OllamaTagsResponse)result;
        tagsResponse.Models.Should().HaveCount(2);
        tagsResponse.Models!.Select(m => m.Name).Should().BeEquivalentTo("llama3.1", "mistral");
    }
}
