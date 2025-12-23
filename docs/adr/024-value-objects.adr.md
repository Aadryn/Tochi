# 24. Value Objects

Date: 2025-12-21

## Statut

Accepté

## Contexte

L'utilisation de types primitifs pour représenter des concepts métier pose des problèmes :
- **Primitive Obsession** : `string` pour email, `decimal` pour montant, `Guid` pour ID
- **Validation dispersée** : Mêmes règles répétées partout
- **Confusion** : Quelle `string` est un email, laquelle est un nom ?
- **Bugs subtils** : Passer un prix là où on attend une quantité

```csharp
// ❌ PRIMITIVE OBSESSION : Types primitifs partout
public class Order
{
    public Guid Id { get; set; }           // Quel type d'ID ?
    public Guid CustomerId { get; set; }   // Peut-on les confondre ?
    public string Email { get; set; }      // Validé où ?
    public decimal Amount { get; set; }    // Dans quelle devise ?
    public decimal Discount { get; set; }  // Pourcentage ou montant ?
}

// Problème : on peut mélanger les IDs
order.CustomerId = order.Id; // Compile ! Bug silencieux
```

## Décision

**Utiliser des Value Objects pour encapsuler les concepts métier avec leurs règles de validation et comportements.**

### 1. Définition d'un Value Object

```csharp
/// <summary>
/// Classe de base pour les Value Objects.
/// Deux Value Objects sont égaux si leurs valeurs sont égales.
/// </summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();
    
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;
        
        var other = (ValueObject)obj;
        
        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }
    
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }
    
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }
    
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }
}
```

### 2. Value Object Email

```csharp
/// <summary>
/// Représente une adresse email validée.
/// </summary>
public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    public string Value { get; }
    
    private Email(string value)
    {
        Value = value.ToLowerInvariant(); // Normalisation
    }
    
    /// <summary>
    /// Crée une adresse email validée.
    /// </summary>
    /// <exception cref="ArgumentException">Si l'email est invalide.</exception>
    public static Email Create(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        
        var trimmed = email.Trim();
        
        if (!EmailRegex.IsMatch(trimmed))
            throw new ArgumentException(
                $"'{email}' is not a valid email address", 
                nameof(email));
        
        return new Email(trimmed);
    }
    
    /// <summary>
    /// Tente de créer une adresse email.
    /// </summary>
    public static bool TryCreate(string email, out Email? result)
    {
        result = null;
        
        if (string.IsNullOrWhiteSpace(email))
            return false;
        
        var trimmed = email.Trim();
        
        if (!EmailRegex.IsMatch(trimmed))
            return false;
        
        result = new Email(trimmed);
        return true;
    }
    
    public string Domain => Value.Split('@')[1];
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
    
    public static implicit operator string(Email email) => email.Value;
}
```

### 3. Value Object Money (avec devise)

```csharp
/// <summary>
/// Représente un montant monétaire avec sa devise.
/// </summary>
public sealed class Money : ValueObject, IComparable<Money>
{
    public decimal Amount { get; }
    public Currency Currency { get; }
    
    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    public static Money Create(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);
        
        // Arrondi selon la devise
        var rounded = Math.Round(amount, currency.DecimalPlaces);
        
        return new Money(rounded, currency);
    }
    
    public static Money Zero(Currency currency) => Create(0, currency);
    
    public static Money Euros(decimal amount) => Create(amount, Currency.EUR);
    public static Money Dollars(decimal amount) => Create(amount, Currency.USD);
    
    /// <summary>
    /// Additionne deux montants (même devise).
    /// </summary>
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount + other.Amount, Currency);
    }
    
    /// <summary>
    /// Soustrait un montant (même devise).
    /// </summary>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount - other.Amount, Currency);
    }
    
    /// <summary>
    /// Multiplie par un facteur.
    /// </summary>
    public Money Multiply(decimal factor)
    {
        return Create(Amount * factor, Currency);
    }
    
    /// <summary>
    /// Applique un pourcentage de réduction.
    /// </summary>
    public Money ApplyDiscount(Percentage discount)
    {
        return Create(Amount * (1 - discount.Value / 100), Currency);
    }
    
    public bool IsZero => Amount == 0;
    public bool IsPositive => Amount > 0;
    public bool IsNegative => Amount < 0;
    
    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot operate on different currencies: {Currency} and {other.Currency}");
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
    
    public int CompareTo(Money? other)
    {
        if (other is null) return 1;
        EnsureSameCurrency(other);
        return Amount.CompareTo(other.Amount);
    }
    
    public override string ToString() => $"{Amount:N2} {Currency.Code}";
    
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);
    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;
    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;
}

/// <summary>
/// Représente une devise.
/// </summary>
public sealed class Currency : ValueObject
{
    public string Code { get; }
    public int DecimalPlaces { get; }
    
    private Currency(string code, int decimalPlaces)
    {
        Code = code;
        DecimalPlaces = decimalPlaces;
    }
    
    public static readonly Currency EUR = new("EUR", 2);
    public static readonly Currency USD = new("USD", 2);
    public static readonly Currency JPY = new("JPY", 0); // Pas de décimales
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
    
    public override string ToString() => Code;
}
```

### 4. Strongly-Typed IDs

```csharp
/// <summary>
/// ID fortement typé pour les tenants.
/// </summary>
public readonly record struct TenantId
{
    public Guid Value { get; }
    
    public TenantId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty", nameof(value));
        
        Value = value;
    }
    
    public static TenantId New() => new(Guid.NewGuid());
    public static TenantId Parse(string value) => new(Guid.Parse(value));
    
    public override string ToString() => Value.ToString();
    
    public static implicit operator Guid(TenantId id) => id.Value;
}

/// <summary>
/// ID fortement typé pour les utilisateurs.
/// </summary>
public readonly record struct UserId
{
    public Guid Value { get; }
    
    public UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(value));
        
        Value = value;
    }
    
    public static UserId New() => new(Guid.NewGuid());
    
    public override string ToString() => Value.ToString();
}

// Utilisation : impossible de confondre les IDs !
public class Order
{
    public OrderId Id { get; }
    public TenantId TenantId { get; }
    public UserId CustomerId { get; }
    
    // order.TenantId = order.CustomerId; // ❌ Erreur de compilation !
}
```

### 5. Value Object DateRange

```csharp
/// <summary>
/// Représente une plage de dates.
/// </summary>
public sealed class DateRange : ValueObject
{
    public DateOnly Start { get; }
    public DateOnly End { get; }
    
    private DateRange(DateOnly start, DateOnly end)
    {
        Start = start;
        End = end;
    }
    
    public static DateRange Create(DateOnly start, DateOnly end)
    {
        if (end < start)
            throw new ArgumentException(
                $"End date {end} cannot be before start date {start}");
        
        return new DateRange(start, end);
    }
    
    /// <summary>
    /// Crée une plage pour un mois complet.
    /// </summary>
    public static DateRange ForMonth(int year, int month)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return new DateRange(start, end);
    }
    
    /// <summary>
    /// Nombre de jours dans la plage (inclusif).
    /// </summary>
    public int DaysCount => End.DayNumber - Start.DayNumber + 1;
    
    /// <summary>
    /// Vérifie si une date est dans la plage.
    /// </summary>
    public bool Contains(DateOnly date)
    {
        return date >= Start && date <= End;
    }
    
    /// <summary>
    /// Vérifie si deux plages se chevauchent.
    /// </summary>
    public bool Overlaps(DateRange other)
    {
        return Start <= other.End && End >= other.Start;
    }
    
    /// <summary>
    /// Fusionne deux plages qui se chevauchent.
    /// </summary>
    public DateRange? Merge(DateRange other)
    {
        if (!Overlaps(other))
            return null;
        
        var newStart = Start < other.Start ? Start : other.Start;
        var newEnd = End > other.End ? End : other.End;
        
        return new DateRange(newStart, newEnd);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
    
    public override string ToString() => $"{Start:d} - {End:d}";
}
```

### 6. Value Object Percentage

```csharp
/// <summary>
/// Représente un pourcentage (0-100).
/// </summary>
public sealed class Percentage : ValueObject, IComparable<Percentage>
{
    public decimal Value { get; }
    
    private Percentage(decimal value)
    {
        Value = value;
    }
    
    public static Percentage Create(decimal value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(
                nameof(value), 
                $"Percentage must be between 0 and 100, got {value}");
        
        return new Percentage(value);
    }
    
    public static Percentage Zero => new(0);
    public static Percentage Full => new(100);
    
    /// <summary>
    /// Crée un pourcentage depuis une fraction (0.15 → 15%).
    /// </summary>
    public static Percentage FromFraction(decimal fraction)
    {
        return Create(fraction * 100);
    }
    
    /// <summary>
    /// Convertit en fraction (15% → 0.15).
    /// </summary>
    public decimal ToFraction() => Value / 100;
    
    /// <summary>
    /// Applique ce pourcentage à un montant.
    /// </summary>
    public decimal ApplyTo(decimal amount) => amount * ToFraction();
    
    /// <summary>
    /// Calcule le complément (15% → 85%).
    /// </summary>
    public Percentage Complement() => new(100 - Value);
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public int CompareTo(Percentage? other)
    {
        return other is null ? 1 : Value.CompareTo(other.Value);
    }
    
    public override string ToString() => $"{Value}%";
    
    public static bool operator <(Percentage left, Percentage right) 
        => left.Value < right.Value;
    public static bool operator >(Percentage left, Percentage right) 
        => left.Value > right.Value;
}
```

### 7. Utilisation dans les entités

```csharp
public class Tenant
{
    public TenantId Id { get; private set; }
    public TenantName Name { get; private set; }
    public Email ContactEmail { get; private set; }
    public Money MonthlyBudget { get; private set; }
    public Percentage DiscountRate { get; private set; }
    
    private Tenant() { } // Pour EF Core
    
    public static Tenant Create(
        TenantName name, 
        Email contactEmail, 
        Money monthlyBudget)
    {
        return new Tenant
        {
            Id = TenantId.New(),
            Name = name,
            ContactEmail = contactEmail,
            MonthlyBudget = monthlyBudget,
            DiscountRate = Percentage.Zero
        };
    }
    
    public void UpdateContactEmail(Email newEmail)
    {
        // Email est déjà validé par construction !
        ContactEmail = newEmail;
    }
    
    public void ApplySpecialDiscount(Percentage discount)
    {
        // Percentage est déjà validé (0-100) !
        DiscountRate = discount;
    }
    
    public Money CalculateDiscountedPrice(Money originalPrice)
    {
        // Les opérations sont type-safe
        return originalPrice.ApplyDiscount(DiscountRate);
    }
}
```

### 8. Configuration EF Core pour Value Objects

```csharp
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);
        
        // Strongly-typed ID
        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => new TenantId(value));
        
        // Value Object simple (Email)
        builder.Property(t => t.ContactEmail)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .HasMaxLength(255);
        
        // Value Object complexe (Money) - Owned Entity
        builder.OwnsOne(t => t.MonthlyBudget, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("MonthlyBudgetAmount")
                .HasPrecision(18, 2);
            
            money.Property(m => m.Currency)
                .HasColumnName("MonthlyBudgetCurrency")
                .HasMaxLength(3)
                .HasConversion(
                    c => c.Code,
                    code => code switch
                    {
                        "EUR" => Currency.EUR,
                        "USD" => Currency.USD,
                        _ => throw new InvalidOperationException($"Unknown currency: {code}")
                    });
        });
        
        // Value Object simple (Percentage)
        builder.Property(t => t.DiscountRate)
            .HasConversion(
                p => p.Value,
                value => Percentage.Create(value))
            .HasColumnName("DiscountRatePercent");
    }
}
```

## Conséquences

### Positives

- **Type Safety** : Impossible de confondre Email et TenantName
- **Validation centralisée** : Règles au même endroit
- **Auto-documentation** : Les types expriment le domaine
- **Comportements riches** : Opérations sur Money, DateRange, etc.
- **Testabilité** : Value Objects testables en isolation

### Négatives

- **Plus de classes** : Une classe par concept
  - *Mitigation* : Les gains en maintenabilité compensent
- **Mapping EF Core** : Configuration supplémentaire
  - *Mitigation* : Conventions et extensions helper

### Neutres

- Les Value Objects sont un concept fondamental du DDD

## Alternatives considérées

### Option A : Rester avec des primitives

- **Description** : Utiliser `string`, `decimal`, `Guid`
- **Avantages** : Simple, pas de classes supplémentaires
- **Inconvénients** : Primitive obsession, validation dispersée
- **Raison du rejet** : Trop de bugs potentiels

### Option B : Records C# sans validation

- **Description** : `public record Email(string Value);`
- **Avantages** : Syntaxe concise
- **Inconvénients** : Pas de validation, constructeur public
- **Raison du rejet** : On perd les garanties du Value Object

## Références

- [Value Objects - Martin Fowler](https://martinfowler.com/bliki/ValueObject.html)
- [Domain-Driven Design - Eric Evans](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)
- [Implementing Value Objects in C#](https://enterprisecraftsmanship.com/posts/value-objects-explanation/)
