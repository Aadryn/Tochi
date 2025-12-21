namespace LLMProxy.Domain.Common;

/// <summary>
/// Représente le résultat d'une opération de domaine avec une valeur
/// </summary>
public class Result<T> : Result
{
    public T Value { get; }

    internal Result(T value, bool isSuccess, string? error) : base(isSuccess, error)
    {
        Value = value;
    }
}
