namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Anthropic.Contracts;

internal sealed record AnthropicStreamEvent
{
    public required string Type { get; init; }
    public AnthropicStreamMessage? Message { get; init; }
    public AnthropicStreamDelta? Delta { get; init; }
}
