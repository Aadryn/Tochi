namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Anthropic.Contracts;

internal sealed record AnthropicMessage
{
    public required string Role { get; init; }
    public required string Content { get; init; }
}
