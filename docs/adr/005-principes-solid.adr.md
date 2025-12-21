# 5. Principes SOLID

Date: 2025-12-21

## Statut

Accepté

## Contexte

Les principes SOLID, introduits par Robert C. Martin, sont des fondamentaux de la conception orientée objet. Ils visent à créer du code :
- Facile à maintenir et étendre
- Résistant aux changements
- Testable unitairement
- Faiblement couplé

Sans ces principes, le code tend vers :
- Des classes "God Object" qui font tout
- Un couplage fort rendant les modifications risquées
- Une impossibilité de tester unitairement
- Une rigidité face aux évolutions

## Décision

**Appliquer les 5 principes SOLID dans la conception du code.**

---

### S - Single Responsibility Principle (SRP)

**Une classe ne doit avoir qu'une seule raison de changer.**

```csharp
// ❌ VIOLATION SRP : Classe avec multiples responsabilités
public class UserService
{
    public void CreateUser(User user) { /* ... */ }
    public void SendWelcomeEmail(User user) { /* ... */ }
    public void GenerateReport(User user) { /* ... */ }
    public void ValidateUser(User user) { /* ... */ }
}

// ✅ SRP : Une responsabilité par classe
public class UserService
{
    public void CreateUser(User user) { /* ... */ }
}

public class EmailService
{
    public void SendWelcomeEmail(User user) { /* ... */ }
}

public class UserReportGenerator
{
    public Report Generate(User user) { /* ... */ }
}

public class UserValidator
{
    public ValidationResult Validate(User user) { /* ... */ }
}
```

---

### O - Open/Closed Principle (OCP)

**Les entités logicielles doivent être ouvertes à l'extension mais fermées à la modification.**

```csharp
// ❌ VIOLATION OCP : Modification nécessaire pour chaque nouveau type
public class DiscountCalculator
{
    public decimal Calculate(Order order)
    {
        return order.CustomerType switch
        {
            "Regular" => order.Total * 0.05m,
            "Premium" => order.Total * 0.10m,
            "VIP" => order.Total * 0.15m,
            // Ajouter un nouveau type = modifier cette classe
            _ => 0
        };
    }
}

// ✅ OCP : Extension sans modification
public interface IDiscountStrategy
{
    decimal Calculate(Order order);
}

public class RegularDiscount : IDiscountStrategy
{
    public decimal Calculate(Order order) => order.Total * 0.05m;
}

public class PremiumDiscount : IDiscountStrategy
{
    public decimal Calculate(Order order) => order.Total * 0.10m;
}

// Nouveau type = nouvelle classe, pas de modification existante
public class VIPDiscount : IDiscountStrategy
{
    public decimal Calculate(Order order) => order.Total * 0.15m;
}
```

---

### L - Liskov Substitution Principle (LSP)

**Les objets d'une classe dérivée doivent pouvoir remplacer les objets de la classe de base sans altérer le comportement du programme.**

```csharp
// ❌ VIOLATION LSP : Le carré ne peut pas substituer le rectangle
public class Rectangle
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }
    public int Area => Width * Height;
}

public class Square : Rectangle
{
    public override int Width
    {
        set { base.Width = base.Height = value; }
    }
    public override int Height
    {
        set { base.Width = base.Height = value; }
    }
}

// Ce code échoue avec Square :
Rectangle rect = new Square();
rect.Width = 5;
rect.Height = 10;
// Attendu: Area = 50, Réel: Area = 100 (carré!)

// ✅ LSP : Hiérarchie correcte
public interface IShape
{
    int Area { get; }
}

public class Rectangle : IShape
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Area => Width * Height;
}

public class Square : IShape
{
    public int Side { get; set; }
    public int Area => Side * Side;
}
```

---

### I - Interface Segregation Principle (ISP)

**Les clients ne doivent pas être forcés de dépendre d'interfaces qu'ils n'utilisent pas.**

```csharp
// ❌ VIOLATION ISP : Interface trop large
public interface IWorker
{
    void Work();
    void Eat();
    void Sleep();
}

public class Robot : IWorker
{
    public void Work() { /* OK */ }
    public void Eat() { throw new NotSupportedException(); } // Robot ne mange pas!
    public void Sleep() { throw new NotSupportedException(); } // Robot ne dort pas!
}

// ✅ ISP : Interfaces ségrégées
public interface IWorkable
{
    void Work();
}

public interface IFeedable
{
    void Eat();
}

public interface ISleepable
{
    void Sleep();
}

public class Human : IWorkable, IFeedable, ISleepable
{
    public void Work() { /* ... */ }
    public void Eat() { /* ... */ }
    public void Sleep() { /* ... */ }
}

public class Robot : IWorkable
{
    public void Work() { /* ... */ }
}
```

---

### D - Dependency Inversion Principle (DIP)

**Les modules de haut niveau ne doivent pas dépendre des modules de bas niveau. Les deux doivent dépendre d'abstractions.**

```csharp
// ❌ VIOLATION DIP : Dépendance directe sur l'implémentation
public class OrderService
{
    private readonly SqlServerDatabase _database = new();
    private readonly SmtpEmailSender _emailSender = new();
    
    public void CreateOrder(Order order)
    {
        _database.Save(order);
        _emailSender.Send(order.CustomerEmail, "Order Created");
    }
}

// ✅ DIP : Dépendance sur abstractions + injection
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IEmailSender _emailSender;
    
    public OrderService(IOrderRepository repository, IEmailSender emailSender)
    {
        _repository = repository;
        _emailSender = emailSender;
    }
    
    public void CreateOrder(Order order)
    {
        _repository.Save(order);
        _emailSender.Send(order.CustomerEmail, "Order Created");
    }
}
```

---

## Conséquences

### Positives

- **Testabilité** : Code facilement testable unitairement grâce au DIP
- **Maintenabilité** : Modifications localisées grâce au SRP et OCP
- **Extensibilité** : Ajout de fonctionnalités sans casser l'existant (OCP)
- **Flexibilité** : Composants interchangeables (LSP, DIP)
- **Clarté** : Responsabilités claires et interfaces ciblées (SRP, ISP)

### Négatives

- **Plus de fichiers** : Multiplication des classes et interfaces
  - *Mitigation* : Organisation en dossiers par domaine/fonctionnalité
- **Indirection** : Navigation moins directe dans le code
  - *Mitigation* : Utilisation des outils IDE (Go to Implementation)
- **Courbe d'apprentissage** : Concepts à maîtriser pour l'équipe
  - *Mitigation* : Formation et revues de code pédagogiques

### Neutres

- SOLID ne doit pas être appliqué dogmatiquement ; le pragmatisme reste de mise

## Alternatives considérées

### Option A : Code procédural

- **Description** : Écrire du code séquentiel sans structure objet
- **Avantages** : Simple à écrire initialement
- **Inconvénients** : Impossible à maintenir à grande échelle, non testable
- **Raison du rejet** : Ne scale pas pour un projet d'entreprise

### Option B : SOLID partiel

- **Description** : Appliquer uniquement certains principes (ex: SRP + DIP)
- **Avantages** : Moins de complexité immédiate
- **Inconvénients** : Incohérence, les principes se renforcent mutuellement
- **Raison du rejet** : Les 5 principes forment un tout cohérent

## Références

- [Clean Architecture - Robert C. Martin](https://www.amazon.com/Clean-Architecture-Craftsmans-Software-Structure/dp/0134494164)
- [SOLID Principles - Wikipedia](https://en.wikipedia.org/wiki/SOLID)
- [Agile Software Development - Robert C. Martin](https://www.amazon.com/Agile-Software-Development-Principles-Practices/dp/0135974445)
