namespace LLMProxy.Gateway.Middleware;

/// <summary>
/// Détails d'une erreur dans la réponse structurée
/// </summary>
public class ErrorDetail
{
    public string Message { get; set; } = null!;
    public string Type { get; set; } = null!;
    public int StatusCode { get; set; }
    public string? RequestId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
    public string? StackTrace { get; set; }
}
