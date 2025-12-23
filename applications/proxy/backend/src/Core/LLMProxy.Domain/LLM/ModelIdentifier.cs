using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.LLM;

/// <summary>
/// Value Object représentant un identifiant de modèle LLM.
/// Encapsule le nom du modèle avec validation.
/// </summary>
/// <example>
/// <code>
/// var model = ModelIdentifier.Create("gpt-4o");
/// var model2 = ModelIdentifier.Create("claude-3-opus-20240229");
/// </code>
/// </example>
public sealed class ModelIdentifier : ValueObject
{
    /// <summary>
    /// Longueur maximale autorisée pour un identifiant de modèle.
    /// </summary>
    public const int MaxLength = 256;

    /// <summary>
    /// Valeur de l'identifiant de modèle.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Constructeur privé pour contrôler la création via factory.
    /// </summary>
    private ModelIdentifier(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Crée un nouvel identifiant de modèle avec validation.
    /// </summary>
    /// <param name="value">Nom du modèle.</param>
    /// <returns>Result contenant le ModelIdentifier ou une erreur.</returns>
    public static Result<ModelIdentifier> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation.Required(nameof(value));
        }

        if (value.Length > MaxLength)
        {
            return Error.Validation.TooLong(nameof(value), MaxLength);
        }

        return new ModelIdentifier(value.Trim());
    }

    /// <summary>
    /// Crée un identifiant de modèle sans validation (pour les cas internes).
    /// </summary>
    /// <param name="value">Nom du modèle (doit être valide).</param>
    /// <returns>Instance de ModelIdentifier.</returns>
    /// <exception cref="ArgumentException">Si la valeur est nulle ou vide.</exception>
    public static ModelIdentifier FromValid(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        return new ModelIdentifier(value);
    }

    /// <summary>
    /// Retourne les composants utilisés pour l'égalité.
    /// </summary>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    /// <summary>
    /// Conversion implicite vers string.
    /// </summary>
    public static implicit operator string(ModelIdentifier identifier) => identifier.Value;

    /// <summary>
    /// Représentation textuelle de l'identifiant.
    /// </summary>
    public override string ToString() => Value;
}
