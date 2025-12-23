namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Anthropic.Contracts;

internal sealed record AnthropicUsage
{
    public int InputTokens { get; init; }
    public int OutputTokens { get; init; }
}
