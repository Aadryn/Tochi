using FluentAssertions;
using LLMProxy.Application.ApiTransformation.OpenAI;
using LLMProxy.Application.ApiTransformation.OpenAI.Contracts;
using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.Tests.ApiTransformation;

/// <summary>
/// Tests unitaires pour OpenAIResponseTransformer.
/// </summary>
public sealed class OpenAIResponseTransformerTests
{
    private readonly OpenAIResponseTransformer _sut = new();

    [Fact]
    public void SupportedFormat_ReturnsOpenAI()
    {
        // Act
        var format = _sut.SupportedFormat;

        // Assert
        format.Should().Be(ApiFormat.OpenAI);
    }

    [Fact]
    public void TransformChatResponse_WithValidResponse_ReturnsOpenAIResponse()
    {
        // Arrange
        var response = new LLMResponse
        {
            Id = "chat-123",
            Model = ModelIdentifier.FromValid("gpt-4"),
            Content = "Hello! How can I help you?",
            FinishReason = FinishReason.Stop,
            Usage = TokenUsage.FromValid(10, 20)
        };

        // Act
        var result = _sut.TransformChatResponse(response);

        // Assert
        result.Should().BeOfType<OpenAIChatResponse>();
        var openAIResponse = (OpenAIChatResponse)result;
        openAIResponse.Model.Should().Be("gpt-4");
        openAIResponse.Choices.Should().HaveCount(1);
        openAIResponse.Choices![0].Message.Should().NotBeNull();
        openAIResponse.Choices[0].Message!.Content.Should().Be("Hello! How can I help you?");
        openAIResponse.Choices[0].Message.Role.Should().Be("assistant");
        openAIResponse.Choices[0].FinishReason.Should().Be("stop");
        openAIResponse.Usage.Should().NotBeNull();
        openAIResponse.Usage!.PromptTokens.Should().Be(10);
        openAIResponse.Usage.CompletionTokens.Should().Be(20);
        openAIResponse.Usage.TotalTokens.Should().Be(30);
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
            Model = ModelIdentifier.FromValid("gpt-4"),
            Content = "Hello",
            IsStreamChunk = true,
            StreamIndex = 0
        };

        // Act
        var result = _sut.TransformStreamChunk(response);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\"model\":\"gpt-4\"");
        result.Should().Contain("\"content\":\"Hello\"");
    }

    [Fact]
    public void TransformEmbeddingResponse_WithValidResponse_ReturnsOpenAIResponse()
    {
        // Arrange
        var response = new EmbeddingResponse
        {
            Model = ModelIdentifier.FromValid("text-embedding-ada-002"),
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
        result.Should().BeOfType<OpenAIEmbeddingResponse>();
        var embeddingResponse = (OpenAIEmbeddingResponse)result;
        embeddingResponse.Model.Should().Be("text-embedding-ada-002");
        embeddingResponse.Data.Should().HaveCount(2);
        embeddingResponse.Data![0].Index.Should().Be(0);
        embeddingResponse.Data[0].Embedding.Should().BeEquivalentTo(new[] { 0.1f, 0.2f, 0.3f });
        embeddingResponse.Data[1].Index.Should().Be(1);
        embeddingResponse.Usage.Should().NotBeNull();
        embeddingResponse.Usage!.PromptTokens.Should().Be(5);
    }

    [Fact]
    public void TransformModelsResponse_WithValidModels_ReturnsOpenAIResponse()
    {
        // Arrange
        var models = new List<LLMModel>
        {
            new LLMModel
            {
                Id = ModelIdentifier.FromValid("gpt-4"),
                Name = "GPT-4",
                OwnedBy = "openai"
            },
            new LLMModel
            {
                Id = ModelIdentifier.FromValid("gpt-3.5-turbo"),
                Name = "GPT-3.5 Turbo",
                OwnedBy = "openai"
            }
        };

        // Act
        var result = _sut.TransformModelsResponse(models);

        // Assert
        result.Should().BeOfType<OpenAIModelsResponse>();
        var modelsResponse = (OpenAIModelsResponse)result;
        modelsResponse.Data.Should().HaveCount(2);
        modelsResponse.Data!.Select(m => m.Id).Should().BeEquivalentTo("gpt-4", "gpt-3.5-turbo");
    }
}
