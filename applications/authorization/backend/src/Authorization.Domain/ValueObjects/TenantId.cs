namespace Authorization.Domain.ValueObjects;

/// <summary>
/// Représente l'identifiant unique d'un tenant pour le multi-tenancy.
/// </summary>
/// <remarks>
/// <para>
/// Chaque tenant dispose de son propre store OpenFGA.
/// Le TenantId est utilisé pour isoler les données d'autorisation entre tenants.
/// </para>
/// </remarks>
public readonly record struct TenantId : IEquatable<TenantId>
{
    /// <summary>
    /// Valeur de l'identifiant du tenant.
    /// </summary>
    public string Value { get; }

    private TenantId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("TenantId ne peut pas être vide.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Crée un TenantId à partir d'une chaîne.
    /// </summary>
    /// <param name="value">Identifiant du tenant.</param>
    /// <returns>Instance de TenantId.</returns>
    public static TenantId Create(string value) => new(value.Trim());

    /// <summary>
    /// Parse une chaîne en TenantId.
    /// </summary>
    /// <param name="value">Chaîne à parser.</param>
    /// <returns>Instance de TenantId.</returns>
    public static TenantId Parse(string value) => Create(value);

    /// <summary>
    /// Tente de parser une chaîne en TenantId.
    /// </summary>
    /// <param name="value">Chaîne à parser.</param>
    /// <param name="tenantId">TenantId résultant si succès.</param>
    /// <returns>True si le parsing a réussi.</returns>
    public static bool TryParse(string? value, out TenantId tenantId)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            tenantId = default;
            return false;
        }

        tenantId = new TenantId(value.Trim());
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    /// Conversion implicite vers string.
    /// </summary>
    public static implicit operator string(TenantId id) => id.Value;

    /// <summary>
    /// Retourne le nom du store OpenFGA pour ce tenant.
    /// </summary>
    /// <returns>Nom du store au format "authz-{tenantId}".</returns>
    public string ToOpenFgaStoreName() => $"authz-{Value}";
}
