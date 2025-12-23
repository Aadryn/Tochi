namespace LLMProxy.Infrastructure.PostgreSQL;

/// <summary>
/// Extensions de chaînes pour conversions de nommage.
/// </summary>
/// <remarks>
/// Fournit des méthodes utilitaires pour convertir les noms de propriétés C# en conventions de nommage
/// PostgreSQL (snake_case).
/// </remarks>
public static class StringExtensions
{
    /// <summary>
    /// Convertit une chaîne PascalCase/camelCase en snake_case.
    /// </summary>
    /// <param name="input">Chaîne à convertir.</param>
    /// <returns>Chaîne convertie en snake_case (ex: "UserName" → "user_name").</returns>
    /// <remarks>
    /// Préserve les underscores initiaux et gère les acronymes correctement.
    /// </remarks>
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var startUnderscores = System.Text.RegularExpressions.Regex.Match(input, @"^_+");
        return startUnderscores + System.Text.RegularExpressions.Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }
}
