namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Anthropic.Contracts;

internal sealed record AnthropicStreamMessage
{
    public string? Id { get; init; }
}
