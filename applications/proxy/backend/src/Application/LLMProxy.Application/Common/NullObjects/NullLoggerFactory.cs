using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Common.NullObjects;

/// <summary>
/// Logger Factory Null Object qui ne crée que des NullLogger.
/// </summary>
/// <remarks>
/// Conforme à l'ADR-026 Null Object Pattern.
/// Utile pour les tests unitaires où le logging n'est pas nécessaire.
/// </remarks>
public sealed class NullLoggerFactory : ILoggerFactory
{
    /// <summary>
    /// Instance singleton de la factory null.
    /// </summary>
    public static readonly NullLoggerFactory Instance = new();

    private NullLoggerFactory() { }

    /// <summary>
    /// Crée un NullLogger (comportement neutre).
    /// </summary>
    public ILogger CreateLogger(string categoryName) => NullLogger.Instance;

    /// <summary>
    /// Ajoute un provider - ne fait rien (comportement neutre).
    /// </summary>
    public void AddProvider(ILoggerProvider provider)
    {
        // Comportement neutre : ne fait rien
    }

    /// <summary>
    /// Dispose - ne fait rien (comportement neutre).
    /// </summary>
    public void Dispose()
    {
        // Comportement neutre : ne fait rien
    }
}
