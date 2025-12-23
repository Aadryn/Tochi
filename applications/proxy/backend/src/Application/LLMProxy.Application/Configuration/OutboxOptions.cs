namespace LLMProxy.Application.Configuration;

/// <summary>
/// Options de configuration pour l'Outbox Pattern.
/// </summary>
/// <remarks>
/// Conforme à l'ADR-040 Outbox Pattern.
/// </remarks>
public sealed class OutboxOptions
{
    /// <summary>
    /// Intervalle de polling pour l'OutboxProcessor (récupération des messages non traités).
    /// </summary>
    /// <remarks>
    /// Par défaut : 5 secondes.
    /// Réduire pour moins de latence, augmenter pour moins de charge CPU/DB.
    /// </remarks>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Taille du batch de messages traités par itération.
    /// </summary>
    /// <remarks>
    /// Par défaut : 100 messages.
    /// Augmenter pour meilleur débit, réduire pour moins de charge mémoire.
    /// </remarks>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Nombre maximum de tentatives de traitement avant déplacement vers Dead Letter.
    /// </summary>
    /// <remarks>
    /// Par défaut : 3 tentatives.
    /// Après ce nombre, le message est déplacé vers OutboxDeadLetter pour investigation manuelle.
    /// </remarks>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Active le service de nettoyage des messages traités.
    /// </summary>
    /// <remarks>
    /// Par défaut : activé.
    /// Désactiver uniquement si vous souhaitez conserver tous les messages indéfiniment.
    /// </remarks>
    public bool EnableCleanup { get; set; } = true;

    /// <summary>
    /// Période de rétention des messages traités avant suppression.
    /// </summary>
    /// <remarks>
    /// Par défaut : 7 jours.
    /// Les messages traités plus anciens que cette période sont supprimés.
    /// </remarks>
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Intervalle entre chaque exécution du service de nettoyage.
    /// </summary>
    /// <remarks>
    /// Par défaut : 1 heure.
    /// </remarks>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Active le service de gestion des Dead Letters.
    /// </summary>
    /// <remarks>
    /// Par défaut : activé.
    /// Désactiver si vous ne souhaitez pas de Dead Letter Queue (messages en échec restent dans Outbox).
    /// </remarks>
    public bool EnableDeadLetter { get; set; } = true;

    /// <summary>
    /// Intervalle entre chaque vérification des messages en échec (Dead Letter).
    /// </summary>
    /// <remarks>
    /// Par défaut : 5 minutes.
    /// </remarks>
    public TimeSpan DeadLetterCheckInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Valide la configuration.
    /// </summary>
    /// <exception cref="ArgumentException">Si la configuration est invalide.</exception>
    public void Validate()
    {
        if (PollingInterval <= TimeSpan.Zero)
        {
            throw new ArgumentException(
                "PollingInterval doit être supérieur à zéro",
                nameof(PollingInterval));
        }

        if (BatchSize <= 0)
        {
            throw new ArgumentException(
                "BatchSize doit être supérieur à zéro",
                nameof(BatchSize));
        }

        if (MaxRetries < 1)
        {
            throw new ArgumentException(
                "MaxRetries doit être au moins 1",
                nameof(MaxRetries));
        }

        if (RetentionPeriod <= TimeSpan.Zero)
        {
            throw new ArgumentException(
                "RetentionPeriod doit être supérieur à zéro",
                nameof(RetentionPeriod));
        }

        if (CleanupInterval <= TimeSpan.Zero)
        {
            throw new ArgumentException(
                "CleanupInterval doit être supérieur à zéro",
                nameof(CleanupInterval));
        }

        if (DeadLetterCheckInterval <= TimeSpan.Zero)
        {
            throw new ArgumentException(
                "DeadLetterCheckInterval doit être supérieur à zéro",
                nameof(DeadLetterCheckInterval));
        }
    }
}
