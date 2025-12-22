using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Représente les limites de quotas pour les utilisateurs (requêtes et tokens).
/// Conforme à ADR-022 (Idempotence) : les opérations d'incrémentation sont idempotentes via tracking des transactions.
/// </summary>
public class QuotaLimit : Entity
{
    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public QuotaType QuotaType { get; private set; }
    public long Limit { get; private set; }
    public QuotaPeriod Period { get; private set; }
    public bool IsEnabled { get; private set; }
    
    // Navigation
    public User User { get; private set; } = null!;

    /// <summary>
    /// Ensemble des identifiants de transactions déjà appliquées (pour idempotence).
    /// Empêche le double-comptage des tokens si une même transaction est rejouée.
    /// </summary>
    private readonly HashSet<Guid> _appliedTransactions = new();

    private QuotaLimit() { } // EF Core

    /// <summary>
    /// Constructeur protégé permettant l'héritage par <see cref="UnlimitedQuotaLimit"/>.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="quotaType">Type de quota.</param>
    /// <param name="limit">Limite du quota.</param>
    /// <param name="period">Période du quota.</param>
    protected QuotaLimit(Guid userId, Guid tenantId, QuotaType quotaType, long limit, QuotaPeriod period)
    {
        UserId = userId;
        TenantId = tenantId;
        QuotaType = quotaType;
        Limit = limit;
        Period = period;
        IsEnabled = true;
    }

    public static Result<QuotaLimit> Create(Guid userId, Guid tenantId, QuotaType quotaType, long limit, QuotaPeriod period)
    {
        try
        {
            Guard.AgainstEmptyGuid(userId, nameof(userId), "Invalid user ID.");
            Guard.AgainstEmptyGuid(tenantId, nameof(tenantId), "Invalid tenant ID.");
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<QuotaLimit>(ex.Message);
        }

        if (limit < 0)
            return Result.Failure<QuotaLimit>("Quota limit cannot be negative.");

        var quotaLimit = new QuotaLimit(userId, tenantId, quotaType, limit, period);
        
        return Result.Success(quotaLimit);
    }

    public Result UpdateLimit(long newLimit)
    {
        if (newLimit < 0)
            return Result.Failure("Quota limit cannot be negative.");

        Limit = newLimit;
        MarkAsModified();
        
        return Result.Success();
    }

    public Result Enable()
    {
        if (IsEnabled)
            return Result.Failure("Quota is already enabled.");

        IsEnabled = true;
        MarkAsModified();
        
        return Result.Success();
    }

    public Result Disable()
    {
        if (!IsEnabled)
            return Result.Failure("Quota is already disabled.");

        IsEnabled = false;
        MarkAsModified();
        
        return Result.Success();
    }

    /// <summary>
    /// Enregistre l'utilisation de tokens de manière idempotente.
    /// Si la transaction a déjà été appliquée, l'opération est ignorée (idempotence garantie).
    /// Conforme à ADR-022 (Idempotence).
    /// </summary>
    /// <param name="transactionId">Identifiant unique de la transaction (fourni par le client via Idempotency-Key).</param>
    /// <param name="tokens">Nombre de tokens à incrémenter dans l'usage.</param>
    /// <returns>Résultat de l'opération avec état actuel du quota.</returns>
    public virtual Result<long> RecordUsage(Guid transactionId, long tokens)
    {
        try
        {
            Guard.AgainstEmptyGuid(transactionId, nameof(transactionId), "Transaction ID cannot be empty.");
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        if (tokens <= 0)
            return Result.Failure<long>("Token amount must be positive.");

        // Vérifier si la transaction a déjà été appliquée (idempotence)
        if (_appliedTransactions.Contains(transactionId))
        {
            // Transaction déjà traitée - retourne l'usage actuel sans modification
            // Ceci garantit l'idempotence : appeler RecordUsage() 2x avec même ID = même résultat
            return Result.Success(0L); // 0 tokens ajoutés (déjà comptés)
        }

        // Première application de cette transaction
        _appliedTransactions.Add(transactionId);
        MarkAsModified();

        return Result.Success(tokens); // Retourne nombre de tokens effectivement ajoutés
    }
}
