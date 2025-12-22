using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Null Object représentant un quota illimité (aucune limite).
/// Conforme à ADR-026 (Null Object Pattern).
/// </summary>
/// <remarks>
/// Ce quota fictif accepte toujours toutes les requêtes sans limites.
/// Utilisé pour les utilisateurs premium ou les environnements de test.
/// Pattern Singleton : une seule instance partagée via <see cref="Instance"/>.
/// </remarks>
public sealed class UnlimitedQuotaLimit : QuotaLimit
{
    /// <summary>
    /// Instance singleton du quota illimité.
    /// </summary>
    /// <remarks>
    /// Utiliser cette instance au lieu de créer de nouvelles instances.
    /// Économise la mémoire et garantit un comportement cohérent.
    /// </remarks>
    public static UnlimitedQuotaLimit Instance { get; } = new UnlimitedQuotaLimit();

    /// <summary>
    /// Constructeur privé pour empêcher l'instanciation externe.
    /// </summary>
    /// <remarks>
    /// Conforme au pattern Singleton : seule l'instance statique <see cref="Instance"/> peut être utilisée.
    /// </remarks>
    private UnlimitedQuotaLimit()
        : base(
            userId: Guid.Empty,
            tenantId: Guid.Empty,
            quotaType: QuotaType.TokensPerMonth,
            limit: long.MaxValue,
            period: QuotaPeriod.Month)
    {
    }

    /// <summary>
    /// Le quota illimité accepte toujours n'importe quelle quantité de tokens.
    /// </summary>
    /// <remarks>
    /// Retourne toujours succès avec 0 tokens consommés (quota illimité).
    /// Permet d'éviter les null checks dans le code appelant.
    /// </remarks>
    /// <param name="transactionId">Identifiant de la transaction (ignoré pour quota illimité).</param>
    /// <param name="tokens">Nombre de tokens demandés (toujours accepté).</param>
    /// <returns>Succès avec 0 tokens consommés.</returns>
    public override Result<long> RecordUsage(Guid transactionId, long tokens)
    {
        // Toujours accepté, retourne 0 (pas de quota consommé pour quota illimité)
        return 0L;
    }
}
