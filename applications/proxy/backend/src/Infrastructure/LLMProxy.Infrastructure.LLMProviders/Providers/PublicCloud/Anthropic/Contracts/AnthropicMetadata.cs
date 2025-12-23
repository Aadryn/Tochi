namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Anthropic.Contracts;

internal sealed record AnthropicMetadata
{
    public string? UserId { get; init; }
}
