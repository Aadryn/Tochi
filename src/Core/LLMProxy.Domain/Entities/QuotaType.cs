namespace LLMProxy.Domain.Entities;

/// <summary>
/// Types de quotas applicables (requêtes ou tokens par période)
/// </summary>
public enum QuotaType
{
    RequestsPerMinute = 0,
    RequestsPerHour = 1,
    RequestsPerDay = 2,
    TokensPerMinute = 10,
    TokensPerHour = 11,
    TokensPerDay = 12,
    TokensPerMonth = 13
}
