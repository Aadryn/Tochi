using LLMProxy.Domain.Common;
using System.Text.RegularExpressions;

namespace LLMProxy.Domain.ValueObjects;

/// <summary>
/// Value Object représentant un slug (identifiant URL-friendly) avec normalisation automatique.
/// </summary>
/// <remarks>
/// Garantit que le slug est toujours stocké en minuscules (invariant culture)
/// et respecte le format alphanumérique avec tirets uniquement.
/// Conforme à ADR-024 (Value Objects) et ADR-003 (DRY).
/// </remarks>
public sealed class Slug : ValueObject
{
    private static readonly Regex SlugPattern = new(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    /// <summary>
    /// Obtient la valeur normalisée du slug (toujours en minuscules).
    /// </summary>
    public string Value { get; }

    private Slug(string value)
    {
        Value = value.ToLowerInvariant();
    }

    /// <summary>
    /// Crée une instance de Slug après validation.
    /// </summary>
    /// <param name="slug">Slug à valider et normaliser.</param>
    /// <returns>Résultat contenant le Slug créé ou une erreur.</returns>
    public static Result<Slug> Create(string slug)
    {
        try
        {
            Guard.AgainstNullOrWhiteSpace(slug, nameof(slug), "Slug ne peut pas être vide.");
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Slug>(ex.Message);
        }

        if (slug.Length > 100)
        {
            return Result.Failure<Slug>("Slug ne peut pas dépasser 100 caractères.");
        }

        var normalized = slug.ToLowerInvariant();

        if (!SlugPattern.IsMatch(normalized))
        {
            return Result.Failure<Slug>("Slug doit contenir uniquement des lettres minuscules, chiffres et tirets.");
        }

        return Result.Success(new Slug(slug));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Slug slug) => slug.Value;

    public override string ToString() => Value;
}
