using Authorization.Domain.Abstractions;

namespace Authorization.Domain.ValueObjects;

/// <summary>
/// Représente une permission composée d'une action et d'un type de ressource.
/// </summary>
/// <remarks>
/// <para>
/// Les permissions sont granulaires et suivent le format {ResourceType}:{Action}.
/// Par exemple : prompts:create, models:read, configurations:update.
/// </para>
/// <example>
/// <code>
/// var permission = Permission.Create("prompts", "read");
/// var fromString = Permission.Parse("prompts:write");
/// 
/// // Vérification de correspondance
/// if (permission.Matches("prompts:read")) { ... }
/// </code>
/// </example>
/// </remarks>
public sealed class Permission : ValueObject
{
    private static readonly char[] Separator = [':'];

    /// <summary>
    /// Type de ressource concerné par la permission.
    /// </summary>
    /// <example>prompts, models, configurations, tenants</example>
    public string ResourceType { get; }

    /// <summary>
    /// Action autorisée sur la ressource.
    /// </summary>
    /// <example>read, create, update, delete, list, manage</example>
    public string Action { get; }

    /// <summary>
    /// Représentation canonique de la permission.
    /// </summary>
    public string Value => $"{ResourceType}:{Action}";

    private Permission(string resourceType, string action)
    {
        ResourceType = resourceType;
        Action = action;
    }

    /// <summary>
    /// Crée une permission à partir des composants.
    /// </summary>
    /// <param name="resourceType">Type de ressource.</param>
    /// <param name="action">Action autorisée.</param>
    /// <returns>Instance de Permission.</returns>
    /// <exception cref="ArgumentException">Si les paramètres sont invalides.</exception>
    public static Permission Create(string resourceType, string action)
    {
        if (string.IsNullOrWhiteSpace(resourceType))
        {
            throw new ArgumentException("Le type de ressource ne peut pas être vide.", nameof(resourceType));
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("L'action ne peut pas être vide.", nameof(action));
        }

        // Valider les caractères (alphanumériques et underscores uniquement)
        if (!IsValidIdentifier(resourceType))
        {
            throw new ArgumentException(
                $"Le type de ressource '{resourceType}' contient des caractères invalides. " +
                "Seuls les lettres, chiffres et underscores sont autorisés.",
                nameof(resourceType));
        }

        if (!IsValidIdentifier(action))
        {
            throw new ArgumentException(
                $"L'action '{action}' contient des caractères invalides. " +
                "Seuls les lettres, chiffres et underscores sont autorisés.",
                nameof(action));
        }

        return new Permission(resourceType.ToLowerInvariant(), action.ToLowerInvariant());
    }

    /// <summary>
    /// Parse une chaîne en permission.
    /// </summary>
    /// <param name="value">Chaîne au format ResourceType:Action.</param>
    /// <returns>Instance de Permission.</returns>
    /// <exception cref="ArgumentException">Si le format est invalide.</exception>
    public static Permission Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("La permission ne peut pas être vide.", nameof(value));
        }

        var parts = value.Split(Separator, 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            throw new ArgumentException(
                $"Format de permission invalide : '{value}'. " +
                "Format attendu : ResourceType:Action (ex: prompts:read)",
                nameof(value));
        }

        return Create(parts[0], parts[1]);
    }

    /// <summary>
    /// Tente de parser une chaîne en Permission.
    /// </summary>
    /// <param name="value">Chaîne à parser.</param>
    /// <param name="permission">Permission résultante si succès.</param>
    /// <returns>True si le parsing a réussi.</returns>
    public static bool TryParse(string? value, out Permission? permission)
    {
        permission = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            permission = Parse(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Vérifie si cette permission correspond à une autre.
    /// Supporte le wildcard '*' pour les correspondances génériques.
    /// </summary>
    /// <param name="other">Permission ou pattern à comparer.</param>
    /// <returns>True si les permissions correspondent.</returns>
    public bool Matches(string other)
    {
        if (!TryParse(other, out var otherPermission) || otherPermission is null)
        {
            return false;
        }

        return Matches(otherPermission);
    }

    /// <summary>
    /// Vérifie si cette permission correspond à une autre.
    /// Supporte le wildcard '*' pour les correspondances génériques.
    /// </summary>
    /// <param name="other">Permission à comparer.</param>
    /// <returns>True si les permissions correspondent.</returns>
    public bool Matches(Permission other)
    {
        // Correspondance exacte sur le type de ressource
        var resourceMatch = ResourceType == other.ResourceType ||
                            ResourceType == "*" ||
                            other.ResourceType == "*";

        // Correspondance sur l'action (avec support wildcard)
        var actionMatch = Action == other.Action ||
                          Action == "*" ||
                          other.Action == "*" ||
                          Action == "manage"; // "manage" implique toutes les actions

        return resourceMatch && actionMatch;
    }

    /// <summary>
    /// Vérifie si cette permission implique une autre permission.
    /// Par exemple, "prompts:manage" implique "prompts:read".
    /// </summary>
    /// <param name="other">Permission à vérifier.</param>
    /// <returns>True si cette permission implique l'autre.</returns>
    public bool Implies(Permission other)
    {
        if (ResourceType != other.ResourceType && ResourceType != "*")
        {
            return false;
        }

        // L'action "manage" implique toutes les autres actions
        if (Action == "manage")
        {
            return true;
        }

        // L'action "*" est équivalente à "manage"
        if (Action == "*")
        {
            return true;
        }

        // Sinon, correspondance exacte requise
        return Action == other.Action;
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ResourceType;
        yield return Action;
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    /// Retourne la représentation OpenFGA de la permission.
    /// </summary>
    /// <returns>L'action pour les relations OpenFGA (ex: can_read, can_write).</returns>
    public string ToOpenFgaRelation() => $"can_{Action}";

    private static bool IsValidIdentifier(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        // Doit commencer par une lettre
        if (!char.IsLetter(value[0]) && value[0] != '*')
        {
            return false;
        }

        // Caractères autorisés : lettres, chiffres, underscores, wildcard
        return value.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '*');
    }

    #region Permissions Prédéfinies

    /// <summary>
    /// Permissions standard pour les opérations CRUD.
    /// </summary>
    public static class Actions
    {
        /// <summary>Lecture.</summary>
        public const string Read = "read";

        /// <summary>Création.</summary>
        public const string Create = "create";

        /// <summary>Mise à jour.</summary>
        public const string Update = "update";

        /// <summary>Suppression.</summary>
        public const string Delete = "delete";

        /// <summary>Listage (pagination).</summary>
        public const string List = "list";

        /// <summary>Gestion complète (inclut toutes les actions).</summary>
        public const string Manage = "manage";
    }

    /// <summary>
    /// Types de ressources standards.
    /// </summary>
    public static class Resources
    {
        /// <summary>Organisations.</summary>
        public const string Organizations = "organizations";

        /// <summary>Tenants.</summary>
        public const string Tenants = "tenants";

        /// <summary>Prompts.</summary>
        public const string Prompts = "prompts";

        /// <summary>Modèles LLM.</summary>
        public const string Models = "models";

        /// <summary>Configurations.</summary>
        public const string Configurations = "configurations";

        /// <summary>Utilisateurs.</summary>
        public const string Users = "users";

        /// <summary>Rôles.</summary>
        public const string Roles = "roles";

        /// <summary>Permissions.</summary>
        public const string Permissions = "permissions";
    }

    #endregion
}
