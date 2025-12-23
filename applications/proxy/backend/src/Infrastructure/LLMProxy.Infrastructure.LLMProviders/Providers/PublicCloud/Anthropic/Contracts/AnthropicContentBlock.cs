namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Anthropic.Contracts;

internal sealed record AnthropicContentBlock
{
    public required string Type { get; init; }
    public string? Text { get; init; }
}
