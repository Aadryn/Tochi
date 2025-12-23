namespace LLMProxy.Domain.LLM;

/// <summary>
/// Représente un vecteur d'embedding.
/// </summary>
public sealed record Embedding
{
    /// <summary>
    /// Index dans la liste des inputs.
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    /// Vecteur d'embedding.
    /// </summary>
    public required IReadOnlyList<float> Vector { get; init; }

    /// <summary>
    /// Texte original (optionnel).
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Dimension du vecteur.
    /// </summary>
    public int Dimensions => Vector.Count;
}
