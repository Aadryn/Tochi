namespace LLMProxy.Gateway.Middleware;

/// <summary>
/// Réponse d'erreur structurée retournée au client
/// </summary>
public class ErrorResponse
{
    public ErrorDetail Error { get; set; } = null!;
}
