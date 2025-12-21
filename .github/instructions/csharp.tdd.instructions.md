---
description: Test-Driven Development (TDD) with xUnit, NFluent, NSubstitute, and Bogus
name: CSharp_TDD_Testing
applyTo: "**/*.Tests.cs,**/*.Unit.Tests/**/*.cs"
---

# Test-Driven Development (TDD) - Guide Complet C#

Guide exhaustif pour TDD avec **xUnit**, **NFluent**, **NSubstitute**, et **Bogus**.

## 🎯 Principes Fondamentaux TDD

**RESPECTER le cycle RED-GREEN-REFACTOR :**

1. 🔴 **RED** : Écrire un test qui ÉCHOUE
   - Définir le comportement attendu
   - Le test ne compile pas ou échoue
   - Vérifier que le test échoue pour la bonne raison

2. 🟢 **GREEN** : Faire passer le test au PLUS VITE
   - Implémenter le minimum de code nécessaire
   - Pas d'optimisation prématurée
   - Le test doit passer

3. 🔵 **REFACTOR** : Améliorer le code sans changer le comportement
   - Éliminer duplication
   - Améliorer lisibilité
   - Tous les tests restent verts

**RÈGLES ABSOLUES :**
- ✅ Écrire le test AVANT le code
- ✅ Un test = un comportement
- ✅ Tests indépendants (pas d'ordre d'exécution)
- ✅ Tests rapides (< 1 seconde par test)
- ✅ Nommage explicite des tests
- ✅ Arrange-Act-Assert systématique

## 📚 Stack Technique

### xUnit - Framework de Tests

```csharp
// ✅ BON - Test unitaire basique
public class CalculatorTests
{
    [Fact]
    public void Add_TwoPositiveNumbers_ReturnsSum()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(5, 3);
        
        // Assert
        Assert.Equal(8, result);
    }
}

// ✅ BON - Test avec théories (paramétrisé)
public class CalculatorTests
{
    [Theory]
    [InlineData(5, 3, 8)]
    [InlineData(10, 5, 15)]
    [InlineData(-5, 3, -2)]
    [InlineData(0, 0, 0)]
    public void Add_VariousInputs_ReturnsExpectedSum(int a, int b, int expected)
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(a, b);
        
        // Assert
        Assert.Equal(expected, result);
    }
}

// ✅ BON - MemberData pour données complexes
public class UserValidatorTests
{
    public static IEnumerable<object[]> ValidEmails => new List<object[]>
    {
        new object[] { "user@example.com" },
        new object[] { "test.user@domain.co.uk" },
        new object[] { "user+tag@example.com" }
    };
    
    [Theory]
    [MemberData(nameof(ValidEmails))]
    public void ValidateEmail_ValidFormat_ReturnsTrue(string email)
    {
        // Arrange
        var validator = new UserValidator();
        
        // Act
        var isValid = validator.ValidateEmail(email);
        
        // Assert
        Assert.True(isValid);
    }
}

// ✅ BON - ClassData pour scénarios réutilisables
public class CalculatorTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { 5, 3, 8 };
        yield return new object[] { 10, 5, 15 };
        yield return new object[] { -5, 3, -2 };
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[Theory]
[ClassData(typeof(CalculatorTestData))]
public void Add_VariousScenarios_ReturnsExpectedResult(int a, int b, int expected)
{
    var calculator = new Calculator();
    var result = calculator.Add(a, b);
    Assert.Equal(expected, result);
}
```

### NFluent - Assertions Fluides

```csharp
using NFluent;

// ✅ BON - Assertions fluides et lisibles
public class UserServiceTests
{
    [Fact]
    public void CreateUser_ValidData_ReturnsCreatedUser()
    {
        // Arrange
        var service = new UserService();
        var email = "user@example.com";
        
        // Act
        var user = service.CreateUser(email, "John", "Doe");
        
        // Assert - NFluent
        Check.That(user).IsNotNull();
        Check.That(user.Email).IsEqualTo(email);
        Check.That(user.FirstName).IsEqualTo("John");
        Check.That(user.LastName).IsEqualTo("Doe");
        Check.That(user.Id).IsNotEqualTo(Guid.Empty);
        Check.That(user.CreatedAt).IsCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
    
    // ✅ BON - Vérifications de collections
    [Fact]
    public void GetActiveUsers_MultipleUsers_ReturnsOnlyActive()
    {
        // Arrange
        var service = new UserService();
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), IsActive = true, Name = "Alice" },
            new() { Id = Guid.NewGuid(), IsActive = false, Name = "Bob" },
            new() { Id = Guid.NewGuid(), IsActive = true, Name = "Charlie" }
        };
        
        // Act
        var activeUsers = service.FilterActiveUsers(users);
        
        // Assert
        Check.That(activeUsers).HasSize(2);
        Check.That(activeUsers).ContainsOnlyElementsThatMatch(u => u.IsActive);
        Check.That(activeUsers).Not.Contains(users[1]);  // Bob inactif
    }
    
    // ✅ BON - Vérifications de strings
    [Fact]
    public void GenerateWelcomeMessage_ValidUser_ReturnsFormattedMessage()
    {
        // Arrange
        var service = new MessageService();
        var user = new User { FirstName = "John", LastName = "Doe" };
        
        // Act
        var message = service.GenerateWelcomeMessage(user);
        
        // Assert
        Check.That(message).IsNotEmpty();
        Check.That(message).StartsWith("Welcome");
        Check.That(message).Contains("John Doe");
        Check.That(message).HasSize(greaterThan: 10);
    }
    
    // ✅ BON - Vérifications d'exceptions
    [Fact]
    public void CreateUser_NullEmail_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new UserService();
        
        // Act & Assert
        Check.ThatCode(() => service.CreateUser(null, "John", "Doe"))
            .Throws<ArgumentNullException>()
            .WithMessage(message => message.Contains("email"));
    }
    
    // ✅ BON - Vérifications numériques
    [Fact]
    public void CalculateDiscount_PremiumUser_ReturnsCorrectAmount()
    {
        // Arrange
        var service = new PricingService();
        var user = new User { IsPremium = true };
        
        // Act
        var discount = service.CalculateDiscount(user, 100m);
        
        // Assert
        Check.That(discount).IsStrictlyGreaterThan(0);
        Check.That(discount).IsStrictlyLessThan(100);
        Check.That(discount).IsEqualTo(10m);
        Check.That(discount).IsCloseTo(10m, 0.01m);
    }
    
    // ✅ BON - Vérifications d'objets complexes
    [Fact]
    public void MapToDto_ValidEntity_ReturnsMappedDto()
    {
        // Arrange
        var mapper = new UserMapper();
        var entity = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        
        // Act
        var dto = mapper.MapToDto(entity);
        
        // Assert
        Check.That(dto).IsInstanceOf<UserDto>();
        Check.That(dto).HasFieldsWithSameValues(new
        {
            Id = entity.Id,
            Email = entity.Email,
            FullName = "John Doe"
        });
    }
}
```

### NSubstitute - Mocking et Stubbing

```csharp
using NSubstitute;
using NSubstitute.ExceptionExtensions;

// ✅ BON - Mock simple
public class UserServiceTests
{
    [Fact]
    public void GetUser_ExistingId_ReturnsUser()
    {
        // Arrange
        var mockRepository = Substitute.For<IUserRepository>();
        var userId = Guid.NewGuid();
        var expectedUser = new User { Id = userId, Name = "John" };
        
        mockRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(expectedUser);
        
        var service = new UserService(mockRepository);
        
        // Act
        var result = await service.GetUserAsync(userId);
        
        // Assert
        Check.That(result).IsEqualTo(expectedUser);
    }
    
    // ✅ BON - Vérifier les appels
    [Fact]
    public async Task CreateUser_ValidData_CallsRepositoryOnce()
    {
        // Arrange
        var mockRepository = Substitute.For<IUserRepository>();
        var service = new UserService(mockRepository);
        var user = new User { Email = "user@example.com" };
        
        // Act
        await service.CreateUserAsync(user);
        
        // Assert
        await mockRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email == user.Email),
            Arg.Any<CancellationToken>()
        );
    }
    
    // ✅ BON - Vérifier qu'un appel N'a PAS eu lieu
    [Fact]
    public async Task GetUser_InvalidId_DoesNotCallRepository()
    {
        // Arrange
        var mockRepository = Substitute.For<IUserRepository>();
        var service = new UserService(mockRepository);
        
        // Act
        await service.GetUserAsync(Guid.Empty);
        
        // Assert
        await mockRepository.DidNotReceive().GetByIdAsync(
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>()
        );
    }
    
    // ✅ BON - Configurer exceptions
    [Fact]
    public async Task GetUser_RepositoryThrows_PropagatesException()
    {
        // Arrange
        var mockRepository = Substitute.For<IUserRepository>();
        mockRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Throws(new DatabaseException("Connection failed"));
        
        var service = new UserService(mockRepository);
        
        // Act & Assert
        Check.ThatAsyncCode(async () => await service.GetUserAsync(Guid.NewGuid()))
            .Throws<DatabaseException>()
            .WithMessage("Connection failed");
    }
    
    // ✅ BON - Argument matchers
    [Fact]
    public async Task SearchUsers_WithQuery_PassesCorrectParameters()
    {
        // Arrange
        var mockRepository = Substitute.For<IUserRepository>();
        var service = new UserService(mockRepository);
        
        // Act
        await service.SearchUsersAsync("john", isActive: true);
        
        // Assert
        await mockRepository.Received().SearchAsync(
            Arg.Is<string>(q => q.ToLower() == "john"),
            Arg.Is<bool>(active => active == true),
            Arg.Any<CancellationToken>()
        );
    }
    
    // ✅ BON - Retours séquentiels
    [Fact]
    public async Task RetryOperation_FirstFailsThenSucceeds_ReturnsResult()
    {
        // Arrange
        var mockService = Substitute.For<IExternalService>();
        mockService.GetDataAsync()
            .Returns(
                Task.FromException<string>(new TimeoutException()),  // 1er appel échoue
                Task.FromResult("Success")                            // 2ème appel réussit
            );
        
        var service = new ResilientService(mockService);
        
        // Act
        var result = await service.GetDataWithRetryAsync();
        
        // Assert
        Check.That(result).IsEqualTo("Success");
        await mockService.Received(2).GetDataAsync();
    }
    
    // ✅ BON - Callback pour actions complexes
    [Fact]
    public async Task SaveUser_ValidUser_UpdatesTimestamp()
    {
        // Arrange
        var mockRepository = Substitute.For<IUserRepository>();
        User capturedUser = null;
        
        mockRepository
            .When(r => r.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()))
            .Do(callInfo => capturedUser = callInfo.Arg<User>());
        
        var service = new UserService(mockRepository);
        var user = new User { Email = "user@example.com" };
        
        // Act
        await service.CreateUserAsync(user);
        
        // Assert
        Check.That(capturedUser).IsNotNull();
        Check.That(capturedUser.CreatedAt).IsCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
    
    // ✅ BON - Partial mock (spy)
    [Fact]
    public void CalculateTotal_CallsProtectedMethod()
    {
        // Arrange
        var service = Substitute.ForPartsOf<OrderService>();
        var order = new Order { Lines = new List<OrderLine>() };
        
        // Act
        var total = service.CalculateTotal(order);
        
        // Assert
        service.Received().CalculateTax(Arg.Any<decimal>());
    }
}
```

### Bogus - Génération de Données de Test

```csharp
using Bogus;

// ✅ BON - Faker simple
public class UserTestDataGenerator
{
    private readonly Faker<User> _userFaker;
    
    public UserTestDataGenerator()
    {
        _userFaker = new Faker<User>()
            .RuleFor(u => u.Id, f => Guid.NewGuid())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.BirthDate, f => f.Date.Past(50, DateTime.Now.AddYears(-18)))
            .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(u => u.Address, f => f.Address.FullAddress())
            .RuleFor(u => u.IsActive, f => f.Random.Bool(0.8f))  // 80% actifs
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(2));
    }
    
    public User GenerateUser() => _userFaker.Generate();
    
    public List<User> GenerateUsers(int count) => _userFaker.Generate(count);
}

// Utilisation dans tests
public class UserServiceTests
{
    private readonly UserTestDataGenerator _dataGenerator = new();
    
    [Fact]
    public void ProcessUsers_MultipleUsers_ProcessesAll()
    {
        // Arrange
        var users = _dataGenerator.GenerateUsers(10);
        var service = new UserService();
        
        // Act
        var processed = service.ProcessUsers(users);
        
        // Assert
        Check.That(processed).HasSize(10);
    }
}

// ✅ BON - Faker avec règles conditionnelles
public class OrderTestDataGenerator
{
    private readonly Faker<Order> _orderFaker;
    
    public OrderTestDataGenerator()
    {
        var orderLineFaker = new Faker<OrderLine>()
            .RuleFor(ol => ol.Id, f => Guid.NewGuid())
            .RuleFor(ol => ol.ProductName, f => f.Commerce.ProductName())
            .RuleFor(ol => ol.Quantity, f => f.Random.Int(1, 10))
            .RuleFor(ol => ol.UnitPrice, f => f.Finance.Amount(10, 1000))
            .RuleFor(ol => ol.Total, (f, ol) => ol.Quantity * ol.UnitPrice);
        
        _orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => Guid.NewGuid())
            .RuleFor(o => o.OrderNumber, f => f.Random.String2(10, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"))
            .RuleFor(o => o.CustomerId, f => Guid.NewGuid())
            .RuleFor(o => o.OrderDate, f => f.Date.Past(1))
            .RuleFor(o => o.Lines, f => orderLineFaker.Generate(f.Random.Int(1, 5)))
            .RuleFor(o => o.Status, f => f.PickRandom<OrderStatus>())
            .RuleFor(o => o.Total, (f, o) => o.Lines.Sum(l => l.Total))
            .RuleFor(o => o.ShippingAddress, f => new Address
            {
                Street = f.Address.StreetAddress(),
                City = f.Address.City(),
                PostalCode = f.Address.ZipCode(),
                Country = f.Address.Country()
            });
    }
    
    public Order GenerateOrder() => _orderFaker.Generate();
    
    public Order GenerateOrderWithStatus(OrderStatus status)
    {
        return _orderFaker
            .RuleFor(o => o.Status, _ => status)
            .Generate();
    }
}

// ✅ BON - Faker avec données localisées
public class FrenchUserDataGenerator
{
    private readonly Faker<User> _userFaker;
    
    public FrenchUserDataGenerator()
    {
        _userFaker = new Faker<User>("fr")  // Locale française
            .RuleFor(u => u.Id, f => Guid.NewGuid())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber("0# ## ## ## ##"))
            .RuleFor(u => u.Address, f => $"{f.Address.StreetAddress()}, {f.Address.ZipCode()} {f.Address.City()}");
    }
    
    public User GenerateUser() => _userFaker.Generate();
}

// ✅ BON - Faker avec datasets prédéfinis
public class ProductTestDataGenerator
{
    private readonly Faker<Product> _productFaker;
    
    private static readonly List<string> Categories = new()
    {
        "Electronics", "Books", "Clothing", "Food", "Toys"
    };
    
    public ProductTestDataGenerator()
    {
        _productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Lorem.Sentence(10))
            .RuleFor(p => p.Category, f => f.PickRandom(Categories))
            .RuleFor(p => p.Price, f => f.Finance.Amount(5, 500))
            .RuleFor(p => p.Stock, f => f.Random.Int(0, 100))
            .RuleFor(p => p.IsAvailable, (f, p) => p.Stock > 0)
            .RuleFor(p => p.Rating, f => f.Random.Double(0, 5))
            .RuleFor(p => p.Tags, f => f.Lorem.Words(f.Random.Int(2, 5)).ToList());
    }
    
    public Product GenerateProduct() => _productFaker.Generate();
    
    public List<Product> GenerateProductsInCategory(string category, int count)
    {
        return _productFaker
            .RuleFor(p => p.Category, _ => category)
            .Generate(count);
    }
}

// ✅ BON - Combinaison Bogus + NSubstitute
public class UserServiceIntegrationTests
{
    private readonly Faker<User> _userFaker = new Faker<User>()
        .RuleFor(u => u.Id, f => Guid.NewGuid())
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.FirstName, f => f.Name.FirstName())
        .RuleFor(u => u.LastName, f => f.Name.LastName());
    
    [Fact]
    public async Task CreateUser_RandomData_SavesCorrectly()
    {
        // Arrange
        var mockRepository = Substitute.For<IUserRepository>();
        var service = new UserService(mockRepository);
        
        var fakeUser = _userFaker.Generate();
        
        // Act
        await service.CreateUserAsync(fakeUser);
        
        // Assert
        await mockRepository.Received(1).AddAsync(
            Arg.Is<User>(u => 
                u.Email == fakeUser.Email &&
                u.FirstName == fakeUser.FirstName &&
                u.LastName == fakeUser.LastName
            ),
            Arg.Any<CancellationToken>()
        );
    }
}
```

## 🏗️ Patterns de Tests

### AAA Pattern (Arrange-Act-Assert)

```csharp
// ✅ BON - Structure AAA claire
[Fact]
public void Transfer_SufficientBalance_TransfersAmount()
{
    // Arrange - Préparation du contexte
    var sourceAccount = new Account { Balance = 1000m };
    var targetAccount = new Account { Balance = 500m };
    var service = new TransferService();
    var amount = 200m;
    
    // Act - Exécution de l'action
    var result = service.Transfer(sourceAccount, targetAccount, amount);
    
    // Assert - Vérifications
    Check.That(result).IsTrue();
    Check.That(sourceAccount.Balance).IsEqualTo(800m);
    Check.That(targetAccount.Balance).IsEqualTo(700m);
}

// ❌ MAUVAIS - Mélange des phases
[Fact]
public void Transfer_Test()
{
    var sourceAccount = new Account { Balance = 1000m };
    var result = service.Transfer(sourceAccount, targetAccount, 200m);  // Act avant Arrange complet
    var targetAccount = new Account { Balance = 500m };
    Check.That(result).IsTrue();
    var service = new TransferService();
    Check.That(sourceAccount.Balance).IsEqualTo(800m);
}
```

### Builder Pattern pour Tests

```csharp
// ✅ BON - Test Data Builder
public class UserBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _email = "user@example.com";
    private string _firstName = "John";
    private string _lastName = "Doe";
    private bool _isActive = true;
    private DateTime _createdAt = DateTime.UtcNow;
    
    public UserBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }
    
    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }
    
    public UserBuilder WithName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }
    
    public UserBuilder Inactive()
    {
        _isActive = false;
        return this;
    }
    
    public UserBuilder CreatedAt(DateTime date)
    {
        _createdAt = date;
        return this;
    }
    
    public User Build()
    {
        return new User
        {
            Id = _id,
            Email = _email,
            FirstName = _firstName,
            LastName = _lastName,
            IsActive = _isActive,
            CreatedAt = _createdAt
        };
    }
}

// Utilisation
[Fact]
public void GetActiveUsers_MixedUsers_ReturnsOnlyActive()
{
    // Arrange
    var users = new List<User>
    {
        new UserBuilder().WithEmail("active1@test.com").Build(),
        new UserBuilder().WithEmail("inactive@test.com").Inactive().Build(),
        new UserBuilder().WithEmail("active2@test.com").Build()
    };
    
    var service = new UserService();
    
    // Act
    var activeUsers = service.GetActiveUsers(users);
    
    // Assert
    Check.That(activeUsers).HasSize(2);
    Check.That(activeUsers).ContainsOnlyElementsThatMatch(u => u.IsActive);
}
```

### Object Mother Pattern

```csharp
// ✅ BON - Object Mother pour objets complexes réutilisables
public static class UserMother
{
    public static User CreateDefault()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "default@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public static User CreateAdmin()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User",
            Role = UserRole.Administrator,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public static User CreateInactive()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@example.com",
            FirstName = "Inactive",
            LastName = "User",
            IsActive = false,
            CreatedAt = DateTime.UtcNow.AddYears(-1)
        };
    }
    
    public static User CreateWithOrders(int orderCount)
    {
        var user = CreateDefault();
        user.Orders = OrderMother.CreateMultiple(orderCount);
        return user;
    }
}

// Utilisation
[Fact]
public void ProcessUser_AdminUser_GrantsFullAccess()
{
    // Arrange
    var admin = UserMother.CreateAdmin();
    var service = new UserService();
    
    // Act
    var result = service.ProcessUser(admin);
    
    // Assert
    Check.That(result.HasFullAccess).IsTrue();
}
```

### Test Fixtures avec IClassFixture

```csharp
// ✅ BON - Fixture pour ressources partagées
public class DatabaseFixture : IDisposable
{
    public AppDbContext Context { get; }
    
    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        Context = new AppDbContext(options);
        
        // Seed data
        Context.Users.AddRange(
            new User { Email = "user1@test.com", FirstName = "User", LastName = "One" },
            new User { Email = "user2@test.com", FirstName = "User", LastName = "Two" }
        );
        Context.SaveChanges();
    }
    
    public void Dispose()
    {
        Context.Dispose();
    }
}

public class UserRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    
    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task GetAllUsers_DatabaseWithUsers_ReturnsAllUsers()
    {
        // Arrange
        var repository = new UserRepository(_fixture.Context);
        
        // Act
        var users = await repository.GetAllAsync();
        
        // Assert
        Check.That(users).HasSize(2);
    }
}
```

## 🎭 Tests de Comportement

### Tests de Validation

```csharp
public class EmailValidatorTests
{
    private readonly EmailValidator _validator = new();
    
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@domain.co.uk")]
    [InlineData("user+tag@example.com")]
    [InlineData("user_name@example.com")]
    public void Validate_ValidEmailFormats_ReturnsTrue(string email)
    {
        // Act
        var isValid = _validator.Validate(email);
        
        // Assert
        Check.That(isValid).IsTrue();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user@@example.com")]
    public void Validate_InvalidEmailFormats_ReturnsFalse(string email)
    {
        // Act
        var isValid = _validator.Validate(email);
        
        // Assert
        Check.That(isValid).IsFalse();
    }
    
    [Fact]
    public void Validate_NullEmail_ThrowsArgumentNullException()
    {
        // Act & Assert
        Check.ThatCode(() => _validator.Validate(null))
            .Throws<ArgumentNullException>()
            .WithProperty("ParamName", "email");
    }
}
```

### Tests de Calculs Métier

```csharp
public class PricingServiceTests
{
    private readonly PricingService _service = new();
    
    [Theory]
    [InlineData(100, 0, 100)]      // Pas de remise
    [InlineData(100, 10, 90)]      // 10% de remise
    [InlineData(100, 25, 75)]      // 25% de remise
    [InlineData(100, 100, 0)]      // 100% de remise
    public void CalculateFinalPrice_VariousDiscounts_ReturnsCorrectAmount(
        decimal basePrice, 
        decimal discountPercent, 
        decimal expected)
    {
        // Act
        var finalPrice = _service.CalculateFinalPrice(basePrice, discountPercent);
        
        // Assert
        Check.That(finalPrice).IsEqualTo(expected);
    }
    
    [Fact]
    public void CalculateTax_FrenchVAT_Adds20Percent()
    {
        // Arrange
        var priceExcludingTax = 100m;
        
        // Act
        var priceIncludingTax = _service.CalculateTax(priceExcludingTax, "FR");
        
        // Assert
        Check.That(priceIncludingTax).IsEqualTo(120m);
    }
    
    [Theory]
    [InlineData(-10)]
    [InlineData(-1)]
    public void CalculateFinalPrice_NegativePrice_ThrowsArgumentException(decimal price)
    {
        // Act & Assert
        Check.ThatCode(() => _service.CalculateFinalPrice(price, 0))
            .Throws<ArgumentException>()
            .WithMessage("Price cannot be negative");
    }
}
```

### Tests Asynchrones

```csharp
public class UserServiceAsyncTests
{
    [Fact]
    public async Task GetUserAsync_ExistingId_ReturnsUser()
    {
        // Arrange
        var mockRepository = Substitute.For<IUserRepository>();
        var userId = Guid.NewGuid();
        var expectedUser = new User { Id = userId, Email = "user@example.com" };
        
        mockRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(expectedUser);
        
        var service = new UserService(mockRepository);
        
        // Act
        var user = await service.GetUserAsync(userId);
        
        // Assert
        Check.That(user).IsEqualTo(expectedUser);
    }
    
    [Fact]
    public async Task CreateUserAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockRepository = Substitute.For<IUserRepository>();
        var service = new UserService(mockRepository);
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        var user = new User { Email = "user@example.com" };
        
        // Act & Assert
        await Check.ThatAsyncCode(async () => await service.CreateUserAsync(user, cts.Token))
            .ThrowsAny();
    }
    
    [Fact]
    public async Task GetAllUsersAsync_MultipleParallelCalls_ReturnsConsistentResults()
    {
        // Arrange
        var mockRepository = Substitute.For<IUserRepository>();
        var users = new List<User> { new() { Id = Guid.NewGuid() } };
        mockRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(users);
        
        var service = new UserService(mockRepository);
        
        // Act
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => service.GetAllUsersAsync())
            .ToList();
        
        var results = await Task.WhenAll(tasks);
        
        // Assert
        Check.That(results).ContainsOnlyElementsOfType<IEnumerable<User>>();
        Check.That(results.All(r => r.Count() == 1)).IsTrue();
    }
}
```

## 🔍 Tests de Edge Cases

### Tests de Limites

```csharp
public class BoundaryTests
{
    private readonly Calculator _calculator = new();
    
    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    public void Add_BoundaryValues_HandlesCorrectly(int value)
    {
        // Act & Assert - Ne doit pas planter
        Check.ThatCode(() => _calculator.Add(value, 0)).DoesNotThrow();
    }
    
    [Fact]
    public void Add_MaxValuePlusOne_ThrowsOverflowException()
    {
        // Act & Assert
        Check.ThatCode(() => _calculator.Add(int.MaxValue, 1))
            .Throws<OverflowException>();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ProcessText_EmptyOrWhitespace_ReturnsEmptyString(string input)
    {
        // Arrange
        var processor = new TextProcessor();
        
        // Act
        var result = processor.Process(input);
        
        // Assert
        Check.That(result).IsEmpty();
    }
    
    [Fact]
    public void GetUsers_EmptyCollection_ReturnsEmptyList()
    {
        // Arrange
        var service = new UserService();
        var emptyList = new List<User>();
        
        // Act
        var result = service.FilterActiveUsers(emptyList);
        
        // Assert
        Check.That(result).IsEmpty();
    }
}
```

### Tests de Null Safety

```csharp
public class NullSafetyTests
{
    [Fact]
    public void CreateUser_NullEmail_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new UserService();
        
        // Act & Assert
        Check.ThatCode(() => service.CreateUser(null, "John", "Doe"))
            .Throws<ArgumentNullException>()
            .WithProperty("ParamName", "email");
    }
    
    [Fact]
    public void GetUserById_NullRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        Check.ThatCode(() => new UserService(null))
            .Throws<ArgumentNullException>();
    }
    
    [Fact]
    public void ProcessUser_NullUser_ReturnsNull()
    {
        // Arrange
        var service = new UserService();
        
        // Act
        var result = service.ProcessUser(null);
        
        // Assert
        Check.That(result).IsNull();
    }
}
```

## 📋 Nommage des Tests

### Convention de Nommage

```csharp
// ✅ BON - Format: MethodName_Scenario_ExpectedBehavior
public class NamingConventionTests
{
    [Fact]
    public void Add_TwoPositiveNumbers_ReturnsSum() { }
    
    [Fact]
    public void Add_NegativeNumbers_ReturnsNegativeSum() { }
    
    [Fact]
    public void Divide_ByZero_ThrowsDivideByZeroException() { }
    
    [Fact]
    public void GetUser_NonExistingId_ReturnsNull() { }
    
    [Fact]
    public void ValidateEmail_InvalidFormat_ReturnsFalse() { }
    
    [Fact]
    public void CreateOrder_InsufficientStock_ThrowsInsufficientStockException() { }
}

// ❌ MAUVAIS - Noms vagues
public class BadNamingTests
{
    [Fact]
    public void Test1() { }  // Trop vague
    
    [Fact]
    public void AddTest() { }  // Pas de scénario
    
    [Fact]
    public void TestUserCreation() { }  // Manque comportement attendu
    
    [Fact]
    public void ShouldWork() { }  // Non descriptif
}

// ✅ ACCEPTABLE - Format alternatif avec Given-When-Then
public class GivenWhenThenTests
{
    [Fact]
    public void GivenTwoNumbers_WhenAdding_ThenReturnsSum() { }
    
    [Fact]
    public void GivenInvalidEmail_WhenValidating_ThenReturnsFalse() { }
}
```

## ✅ Checklist TDD

Avant de committer, VÉRIFIER :

### Couverture et Qualité
- [ ] Tests écrits AVANT le code de production
- [ ] Couverture de code > 80% sur logique métier
- [ ] Tous les tests passent (100% verts)
- [ ] Pas de tests ignorés sans justification
- [ ] Tests rapides (< 1 seconde chacun)

### Structure et Nommage
- [ ] Nommage explicite : `Method_Scenario_ExpectedBehavior`
- [ ] Pattern AAA respecté (Arrange-Act-Assert)
- [ ] Un test = un comportement
- [ ] Tests indépendants (ordre d'exécution indifférent)

### Assertions
- [ ] NFluent utilisé pour assertions fluides
- [ ] Vérifications complètes (pas seulement NotNull)
- [ ] Messages d'erreur explicites
- [ ] Pas d'assertions multiples non liées

### Mocking
- [ ] NSubstitute pour tous les mocks
- [ ] Vérification des appels avec Received()
- [ ] Arguments matchers appropriés (Arg.Is, Arg.Any)
- [ ] Pas de over-mocking (seulement dépendances externes)

### Données de Test
- [ ] Bogus pour génération de données complexes
- [ ] Builders ou Object Mother pour réutilisation
- [ ] Données représentatives (pas toujours "John Doe")
- [ ] Edge cases couverts (null, empty, limites)

### Organisation
- [ ] Tests dans projet séparé (*.Unit.Tests)
- [ ] Tests groupés par classe testée
- [ ] Utilisation de IClassFixture pour ressources partagées
- [ ] Pas de logique complexe dans les tests

## 📚 Ressources

### Documentation Officielle
- [xUnit Documentation](https://xunit.net/)
- [NFluent - Fluent Assertions](https://www.n-fluent.net/)
- [NSubstitute Documentation](https://nsubstitute.github.io/)
- [Bogus - Fake Data Generator](https://github.com/bchavez/Bogus)

### Livres Recommandés
- **Test-Driven Development: By Example** - Kent Beck
- **The Art of Unit Testing** - Roy Osherove
- **Growing Object-Oriented Software, Guided by Tests** - Steve Freeman & Nat Pryce
- **Working Effectively with Legacy Code** - Michael Feathers
