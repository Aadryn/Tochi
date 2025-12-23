using FluentAssertions;
using LLMProxy.Application.ApiTransformation.OpenAI;
using LLMProxy.Application.ApiTransformation.OpenAI.Contracts;
using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.Tests.ApiTransformation;

/// <summary>
/// Tests unitaires pour OpenAIRequestTransformer.
/// </summary>
public sealed class OpenAIRequestTransformerTests
{
    private readonly OpenAIRequestTransformer _sut = new();

    [Fact]
    public void SupportedFormat_ReturnsOpenAI()
    {
        // Act
        var format = _sut.SupportedFormat;

        // Assert
        format.Should().Be(ApiFormat.OpenAI);
    }

    [Fact]
    public void TransformChatRequest_WithValidRequest_ReturnsCanonicalRequest()
    {
        // Arrange
        var request = new OpenAIChatRequest
        {
            Model = "gpt-4",
            Messages = new[]
            {
                new OpenAIMessage { Role = "system", Content = "You are a helpful assistant." },
                new OpenAIMessage { Role = "user", Content = "Hello!" }
            }
        };

        // Act
        var result = _sut.TransformChatRequest(request);

        // Assert
        result.Model.Value.Should().Be("gpt-4");
        result.Messages.Should().HaveCount(2);
        result.Messages[0].Role.Should().Be(MessageRole.System);
        result.Messages[0].Content.Should().Be("You are a helpful assistant.");
        result.Messages[1].Role.Should().Be(MessageRole.User);
        result.Messages[1].Content.Should().Be("Hello!");
    }

    [Theory]
    [InlineData("system", MessageRole.System)]
    [InlineData("user", MessageRole.User)]
    [InlineData("assistant", MessageRole.Assistant)]
    [InlineData("tool", MessageRole.Tool)]
    [InlineData("unknown", MessageRole.User)]
    public void TransformChatRequest_MapsRolesCorrectly(string openAIRole, MessageRole expectedRole)
    {
        // Arrange
        var request = new OpenAIChatRequest
        {
            Model = "gpt-4",
            Messages = new[]
            {
                new OpenAIMessage { Role = openAIRole, Content = "Test" }
            }
        };

        // Act
        var result = _sut.TransformChatRequest(request);

        // Assert
        result.Messages[0].Role.Should().Be(expectedRole);
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
    public void TransformEmbeddingRequest_WithStringInput_ReturnsCanonicalRequest()
    {
        // Arrange
        var request = new OpenAIEmbeddingRequest
        {
            Model = "text-embedding-ada-002",
            Input = "Hello world"
        };

        // Act
        var result = _sut.TransformEmbeddingRequest(request);

        // Assert
        result.Model.Value.Should().Be("text-embedding-ada-002");
        result.Inputs.Should().HaveCount(1);
        result.Inputs[0].Should().Be("Hello world");
    }

    [Fact]
    public void TransformEmbeddingRequest_WithArrayInput_ReturnsCanonicalRequest()
    {
        // Arrange
        var request = new OpenAIEmbeddingRequest
        {
            Model = "text-embedding-ada-002",
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
