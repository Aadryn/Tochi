using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.ValueObjects;

/// <summary>
/// Value Object représentant une adresse email avec normalisation automatique.
/// </summary>
/// <remarks>
/// Garantit que l'email est toujours stocké en minuscules (invariant culture)
/// pour assurer la cohérence des comparaisons.
/// Conforme à ADR-024 (Value Objects) et ADR-003 (DRY).
/// </remarks>
public sealed class Email : ValueObject
{
    /// <summary>
    /// Obtient la valeur normalisée de l'email (toujours en minuscules).
    /// </summary>
    public string Value { get; }

    private Email(string value)
    {
        Value = value.ToLowerInvariant();
    }

    /// <summary>
    /// Crée une instance d'Email après validation.
    /// </summary>
    /// <param name="email">Adresse email à valider et normaliser.</param>
    /// <returns>Résultat contenant l'Email créé ou une erreur.</returns>
    public static Result<Email> Create(string email)
    {
        try
        {
            Guard.AgainstNullOrWhiteSpace(email, nameof(email), "Email ne peut pas être vide.");
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Email>(ex.Message);
        }

        if (email.Length > 255)
        {
            return Result.Failure<Email>("Email ne peut pas dépasser 255 caractères.");
        }

        // Validation basique format email
        if (!email.Contains('@') || !email.Contains('.'))
        {
            return Result.Failure<Email>("Format d'email invalide.");
        }

        return Result.Success(new Email(email));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;
}
