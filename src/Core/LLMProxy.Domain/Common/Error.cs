namespace LLMProxy.Domain.Common;

/// <summary>
/// Représente une erreur avec un code et un message.
/// </summary>
/// <param name="Code">Le code unique identifiant le type d'erreur (ex: "User.NotFound").</param>
/// <param name="Message">Le message descriptif de l'erreur.</param>
/// <remarks>
/// <para>
/// Le code suit la convention "{Domaine}.{Type}" pour faciliter le pattern matching
/// et le mapping vers des codes HTTP dans les contrôleurs.
/// </para>
/// <para>
/// Exemples de codes :
/// <list type="bullet">
/// <item><description>User.NotFound → 404 Not Found</description></item>
/// <item><description>User.EmailExists → 409 Conflict</description></item>
/// <item><description>Validation.Email.Invalid → 400 Bad Request</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record Error(string Code, string Message)
{
    /// <summary>
    /// Représente l'absence d'erreur (succès).
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Indique si cette erreur représente un succès (aucune erreur).
    /// </summary>
    public bool IsNone => Code == string.Empty && Message == string.Empty;

    /// <summary>
    /// Erreurs du domaine User.
    /// </summary>
    public static class User
    {
        /// <summary>
        /// Utilisateur non trouvé.
        /// </summary>
        public static Error NotFound(Guid id) =>
            new("User.NotFound", $"L'utilisateur avec l'ID {id:N} n'a pas été trouvé");

        /// <summary>
        /// Email déjà utilisé par un autre utilisateur.
        /// </summary>
        public static Error EmailAlreadyExists(string email) =>
            new("User.EmailExists", $"L'email '{email}' est déjà enregistré");

        /// <summary>
        /// Identifiants invalides lors de l'authentification.
        /// </summary>
        public static Error InvalidCredentials =>
            new("User.InvalidCredentials", "Email ou mot de passe invalide");

        /// <summary>
        /// Utilisateur inactif.
        /// </summary>
        public static Error Inactive(Guid id) =>
            new("User.Inactive", $"L'utilisateur {id:N} n'est pas actif");

        /// <summary>
        /// Mot de passe trop faible.
        /// </summary>
        public static Error WeakPassword =>
            new("User.WeakPassword", "Le mot de passe ne respecte pas les exigences de sécurité");
    }

    /// <summary>
    /// Erreurs du domaine Tenant.
    /// </summary>
    public static class Tenant
    {
        /// <summary>
        /// Tenant non trouvé.
        /// </summary>
        public static Error NotFound(Guid id) =>
            new("Tenant.NotFound", $"Le tenant avec l'ID {id:N} n'a pas été trouvé");

        /// <summary>
        /// Tenant inactif.
        /// </summary>
        public static Error Inactive(Guid id) =>
            new("Tenant.Inactive", $"Le tenant {id:N} n'est pas actif");

        /// <summary>
        /// Quota mensuel dépassé.
        /// </summary>
        public static Error QuotaExceeded(Guid tenantId, long current, long max) =>
            new("Tenant.QuotaExceeded", 
                $"Le tenant {tenantId:N} a dépassé son quota mensuel ({current:N0}/{max:N0} requêtes)");

        /// <summary>
        /// Nom déjà utilisé par un autre tenant.
        /// </summary>
        public static Error NameAlreadyExists(string name) =>
            new("Tenant.NameExists", $"Le nom '{name}' est déjà utilisé par un autre tenant");

        /// <summary>
        /// Slug déjà utilisé.
        /// </summary>
        public static Error SlugAlreadyExists(string slug) =>
            new("Tenant.SlugExists", $"Le slug '{slug}' est déjà utilisé par un autre tenant");
    }

    /// <summary>
    /// Erreurs du domaine ApiKey.
    /// </summary>
    public static class ApiKey
    {
        /// <summary>
        /// Clé API invalide.
        /// </summary>
        public static Error Invalid =>
            new("ApiKey.Invalid", "La clé API fournie est invalide");

        /// <summary>
        /// Clé API expirée.
        /// </summary>
        public static Error Expired(DateTime expirationDate) =>
            new("ApiKey.Expired", $"La clé API a expiré le {expirationDate:yyyy-MM-dd}");

        /// <summary>
        /// Clé API révoquée.
        /// </summary>
        public static Error Revoked =>
            new("ApiKey.Revoked", "La clé API a été révoquée");

        /// <summary>
        /// Clé API non trouvée.
        /// </summary>
        public static Error NotFound(Guid id) =>
            new("ApiKey.NotFound", $"La clé API avec l'ID {id:N} n'a pas été trouvée");

        /// <summary>
        /// Préfixe de clé API invalide.
        /// </summary>
        public static Error InvalidPrefix(string prefix) =>
            new("ApiKey.InvalidPrefix", $"Le préfixe '{prefix}' n'est pas reconnu");

        /// <summary>
        /// Hash de clé API invalide.
        /// </summary>
        public static Error InvalidHash =>
            new("ApiKey.InvalidHash", "Le hash de la clé API ne correspond pas");
    }

    /// <summary>
    /// Erreurs du domaine Quota.
    /// </summary>
    public static class Quota
    {
        /// <summary>
        /// Quota dépassé.
        /// </summary>
        public static Error Exceeded(Guid tenantId, long current, long max) =>
            new("Quota.Exceeded", 
                $"Le quota a été dépassé pour le tenant {tenantId:N} ({current:N0}/{max:N0})");

        /// <summary>
        /// Quota invalide (valeur négative ou nulle).
        /// </summary>
        public static Error Invalid(long value) =>
            new("Quota.Invalid", $"La valeur de quota {value:N0} est invalide (doit être > 0)");

        /// <summary>
        /// Quota non trouvé.
        /// </summary>
        public static Error NotFound(Guid tenantId) =>
            new("Quota.NotFound", $"Aucun quota configuré pour le tenant {tenantId:N}");
    }

    /// <summary>
    /// Erreurs de validation.
    /// </summary>
    public static class Validation
    {
        /// <summary>
        /// Champ requis manquant.
        /// </summary>
        public static Error Required(string fieldName) =>
            new($"Validation.{fieldName}.Required", $"Le champ '{fieldName}' est requis");

        /// <summary>
        /// Format d'email invalide.
        /// </summary>
        public static Error InvalidEmail(string email) =>
            new("Validation.Email.Invalid", $"Le format de l'email '{email}' est invalide");

        /// <summary>
        /// Valeur trop courte.
        /// </summary>
        public static Error TooShort(string fieldName, int minLength) =>
            new($"Validation.{fieldName}.TooShort", 
                $"Le champ '{fieldName}' doit contenir au moins {minLength} caractères");

        /// <summary>
        /// Valeur trop longue.
        /// </summary>
        public static Error TooLong(string fieldName, int maxLength) =>
            new($"Validation.{fieldName}.TooLong", 
                $"Le champ '{fieldName}' ne doit pas dépasser {maxLength} caractères");

        /// <summary>
        /// Valeur hors limites.
        /// </summary>
        public static Error OutOfRange(string fieldName, object min, object max) =>
            new($"Validation.{fieldName}.OutOfRange", 
                $"Le champ '{fieldName}' doit être entre {min} et {max}");
    }
}
