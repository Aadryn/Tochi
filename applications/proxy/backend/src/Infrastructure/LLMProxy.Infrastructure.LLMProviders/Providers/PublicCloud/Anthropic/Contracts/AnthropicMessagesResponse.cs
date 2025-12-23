namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Anthropic.Contracts;

internal sealed record AnthropicMessagesResponse
{
    public required string Id { get; init; }
    public string? Model { get; init; }
    public string? StopReason { get; init; }
    public List<AnthropicContentBlock>? Content { get; init; }
    public AnthropicUsage? Usage { get; init; }
}
