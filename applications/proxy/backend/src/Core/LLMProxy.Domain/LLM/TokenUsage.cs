using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.LLM;

/// <summary>
/// Value Object représentant l'utilisation de tokens pour une requête LLM.
/// Contient les compteurs de tokens d'entrée et de sortie.
/// </summary>
public sealed class TokenUsage : ValueObject
{
    /// <summary>
    /// Nombre de tokens dans le prompt (entrée).
    /// </summary>
    public int InputTokens { get; }

    /// <summary>
    /// Nombre de tokens dans la réponse (sortie).
    /// </summary>
    public int OutputTokens { get; }

    /// <summary>
    /// Total des tokens utilisés (entrée + sortie).
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;

    /// <summary>
    /// Constructeur privé pour contrôler la création.
    /// </summary>
    private TokenUsage(int inputTokens, int outputTokens)
    {
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
    }

    /// <summary>
    /// Crée une instance de TokenUsage avec validation.
    /// </summary>
    /// <param name="inputTokens">Nombre de tokens d'entrée.</param>
    /// <param name="outputTokens">Nombre de tokens de sortie.</param>
    /// <returns>Result contenant TokenUsage ou une erreur.</returns>
    public static Result<TokenUsage> Create(int inputTokens, int outputTokens)
    {
        if (inputTokens < 0)
        {
            return Error.Validation.OutOfRange(nameof(inputTokens), 0, int.MaxValue);
        }

        if (outputTokens < 0)
        {
            return Error.Validation.OutOfRange(nameof(outputTokens), 0, int.MaxValue);
        }

        return new TokenUsage(inputTokens, outputTokens);
    }

    /// <summary>
    /// Crée une instance vide (aucun token utilisé).
    /// </summary>
    public static TokenUsage Empty => new(0, 0);

    /// <summary>
    /// Crée une instance à partir de valeurs connues valides.
    /// </summary>
    /// <param name="inputTokens">Nombre de tokens d'entrée.</param>
    /// <param name="outputTokens">Nombre de tokens de sortie.</param>
    /// <returns>Instance de TokenUsage.</returns>
    public static TokenUsage FromValid(int inputTokens, int outputTokens)
    {
        return new TokenUsage(Math.Max(0, inputTokens), Math.Max(0, outputTokens));
    }

    /// <summary>
    /// Additionne deux TokenUsage.
    /// </summary>
    public static TokenUsage operator +(TokenUsage a, TokenUsage b)
    {
        return new TokenUsage(
            a.InputTokens + b.InputTokens,
            a.OutputTokens + b.OutputTokens);
    }

    /// <summary>
    /// Retourne les composants utilisés pour l'égalité.
    /// </summary>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return InputTokens;
        yield return OutputTokens;
    }

    /// <summary>
    /// Représentation textuelle de l'utilisation.
    /// </summary>
    public override string ToString() => 
        $"Tokens: {InputTokens} in, {OutputTokens} out, {TotalTokens} total";
}
