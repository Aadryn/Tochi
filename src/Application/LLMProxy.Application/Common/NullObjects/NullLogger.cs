using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Common.NullObjects;

/// <summary>
/// Logger Null Object qui ne journalise rien.
/// </summary>
/// <remarks>
/// Conforme à l'ADR-026 Null Object Pattern.
/// Utile pour les tests unitaires où le logging n'est pas nécessaire.
/// Implémente un comportement neutre qui ne fait rien sans lever d'exceptions.
/// </remarks>
public sealed class NullLogger : ILogger
{
    /// <summary>
    /// Instance singleton du logger null.
    /// </summary>
    public static readonly NullLogger Instance = new();

    private NullLogger() { }

    /// <summary>
    /// Ne log rien - comportement neutre.
    /// </summary>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }

    /// <summary>
    /// Toujours activé (mais ne fait rien).
    /// </summary>
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <summary>
    /// Ne log rien - comportement neutre.
    /// </summary>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        // Comportement neutre : ne fait rien
    }

    /// <summary>
    /// Scope Null Object pour le BeginScope.
    /// </summary>
    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        private NullScope() { }
        public void Dispose() { }
    }
}
