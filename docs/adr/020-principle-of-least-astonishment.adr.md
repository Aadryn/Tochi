# 20. Principle of Least Astonishment (POLA)

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un code surprenant crée de la confusion et des bugs :
- **Comportements inattendus** : Une méthode fait plus/moins que son nom suggère
- **Effets de bord cachés** : Des modifications non annoncées
- **Conventions violées** : Code qui ne suit pas les patterns habituels
- **API trompeuse** : Paramètres ou retours qui ne correspondent pas aux attentes

Exemples de code surprenant :

```csharp
// ❌ SURPRENANT : GetUser modifie l'état !
public User GetUser(Guid id)
{
    var user = _repository.Find(id);
    user.LastAccessedAt = DateTime.UtcNow; // Effet de bord !
    _repository.Save(user);
    return user;
}

// ❌ SURPRENANT : Delete ne supprime pas vraiment
public void DeleteUser(Guid id)
{
    var user = _repository.Find(id);
    user.Status = UserStatus.Deleted; // Soft delete non annoncé
    _repository.Save(user);
}

// ❌ SURPRENANT : Add retourne un bool au lieu de l'élément
public bool Add(Product product)
{
    _context.Products.Add(product);
    return _context.SaveChanges() > 0;
}
```

## Décision

**Appliquer le Principe de Moindre Surprise : le code doit se comporter comme son nom et sa signature le suggèrent.**

### 1. Nommage qui reflète le comportement

```csharp
// ❌ SURPRENANT : Le nom ne reflète pas l'action
public User GetUser(Guid id) // Mais modifie LastAccessedAt !

// ✅ PRÉVISIBLE : Noms clairs pour chaque action
public User GetUserById(Guid id)
{
    return _repository.Find(id); // Lecture pure
}

public User GetUserAndRecordAccess(Guid id)
{
    var user = _repository.Find(id);
    user.RecordAccess();
    _repository.Save(user);
    return user;
}
```

### 2. Méthodes Get/Find ne modifient jamais

```csharp
// ✅ PRÉVISIBLE : Les méthodes de lecture sont pures
public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct)
{
    // Aucune modification, lecture seule
    return await _context.Tenants
        .AsNoTracking() // Lecture seule explicite
        .FirstOrDefaultAsync(t => t.Id == id, ct);
}

public async Task<IReadOnlyList<Tenant>> FindByStatusAsync(
    TenantStatus status, 
    CancellationToken ct)
{
    // Aucune modification, lecture seule
    return await _context.Tenants
        .AsNoTracking()
        .Where(t => t.Status == status)
        .ToListAsync(ct);
}
```

### 3. Delete qui supprime vraiment (ou nom explicite)

```csharp
// ❌ SURPRENANT : Delete qui ne supprime pas
public void Delete(Guid id) { /* soft delete */ }

// ✅ PRÉVISIBLE : Noms explicites
public async Task DeleteAsync(Guid id, CancellationToken ct)
{
    var entity = await _context.Tenants.FindAsync(id, ct);
    _context.Tenants.Remove(entity); // Suppression réelle
    await _context.SaveChangesAsync(ct);
}

public async Task SoftDeleteAsync(Guid id, CancellationToken ct)
{
    var entity = await _context.Tenants.FindAsync(id, ct);
    entity.Status = TenantStatus.Deleted; // Soft delete explicite
    entity.DeletedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync(ct);
}

public async Task ArchiveAsync(Guid id, CancellationToken ct)
{
    var entity = await _context.Tenants.FindAsync(id, ct);
    entity.Status = TenantStatus.Archived;
    entity.ArchivedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync(ct);
}
```

### 4. Retours prévisibles

```csharp
// ❌ SURPRENANT : Add retourne bool
public bool Add(Product product);

// ❌ SURPRENANT : Save retourne l'entité modifiée
public Product Save(Product product);

// ✅ PRÉVISIBLE : Conventions standard
public async Task AddAsync(Product product, CancellationToken ct)
{
    await _context.Products.AddAsync(product, ct);
    // void - l'entité passée est modifiée (Id assigné par EF)
}

public async Task<int> SaveChangesAsync(CancellationToken ct)
{
    return await _context.SaveChangesAsync(ct);
    // int - nombre d'entités modifiées (convention EF)
}

// ✅ PRÉVISIBLE : Create retourne l'entité créée
public async Task<Product> CreateAsync(CreateProductCommand cmd, CancellationToken ct)
{
    var product = new Product(cmd.Name, cmd.Price);
    await _context.Products.AddAsync(product, ct);
    await _context.SaveChangesAsync(ct);
    return product; // Retourne l'entité avec Id
}
```

### 5. Paramètres sans surprise

```csharp
// ❌ SURPRENANT : Ordre non intuitif
public void Transfer(decimal amount, Account to, Account from);

// ❌ SURPRENANT : Paramètre modifié
public void Process(Order order) 
{
    order.Status = OrderStatus.Processed; // Modifie le paramètre !
}

// ✅ PRÉVISIBLE : Ordre logique
public void Transfer(Account from, Account to, decimal amount);

// ✅ PRÉVISIBLE : Retourne une nouvelle instance
public Order Process(Order order)
{
    return order with { Status = OrderStatus.Processed };
}

// ✅ PRÉVISIBLE : Ou méthode sur l'objet lui-même
public class Order
{
    public void MarkAsProcessed()
    {
        Status = OrderStatus.Processed; // L'objet se modifie lui-même
    }
}
```

### 6. Exceptions prévisibles

```csharp
// ❌ SURPRENANT : Exception générique
public User GetUser(Guid id)
{
    var user = _repository.Find(id);
    if (user == null)
        throw new Exception("User not found"); // Trop générique
    return user;
}

// ❌ SURPRENANT : Retourne null parfois, exception parfois
public User GetUser(Guid id)
{
    if (id == Guid.Empty)
        throw new ArgumentException(); // Exception ici
    return _repository.Find(id); // Null là
}

// ✅ PRÉVISIBLE : Comportement cohérent et documenté
/// <summary>
/// Récupère un utilisateur par son ID.
/// </summary>
/// <returns>L'utilisateur trouvé.</returns>
/// <exception cref="UserNotFoundException">Si l'utilisateur n'existe pas.</exception>
public async Task<User> GetUserAsync(Guid id, CancellationToken ct)
{
    if (id == Guid.Empty)
        throw new ArgumentException("ID cannot be empty", nameof(id));
    
    var user = await _repository.FindAsync(id, ct);
    
    return user ?? throw new UserNotFoundException(id);
}

// ✅ PRÉVISIBLE : Ou pattern Try pour les cas d'erreur attendus
public async Task<User?> TryGetUserAsync(Guid id, CancellationToken ct)
{
    if (id == Guid.Empty)
        return null;
    
    return await _repository.FindAsync(id, ct);
}
```

### 7. Propriétés sans effets de bord

```csharp
// ❌ SURPRENANT : Getter avec effet de bord
public decimal Total
{
    get
    {
        _accessCount++; // Effet de bord !
        return _items.Sum(i => i.Price);
    }
}

// ❌ SURPRENANT : Setter qui fait plus que setter
public string Email
{
    set
    {
        _email = value;
        SendVerificationEmail(value); // Effet de bord !
        _logger.Log($"Email changed to {value}"); // Effet de bord !
    }
}

// ✅ PRÉVISIBLE : Propriétés pures
public decimal Total => _items.Sum(i => i.Price);

public string Email { get; private set; }

public void ChangeEmail(string newEmail)
{
    Email = newEmail;
    SendVerificationEmail(newEmail); // Effet explicite
    _logger.Log($"Email changed to {newEmail}");
}
```

### 8. Collections qui se comportent comme attendu

```csharp
// ❌ SURPRENANT : Add qui peut échouer silencieusement
public class UniqueList<T>
{
    public void Add(T item)
    {
        if (!_items.Contains(item))
            _items.Add(item);
        // Pas d'indication si l'élément existait déjà !
    }
}

// ✅ PRÉVISIBLE : Comportement explicite
public class UniqueCollection<T>
{
    public bool TryAdd(T item)
    {
        if (_items.Contains(item))
            return false;
        
        _items.Add(item);
        return true;
    }
    
    public void Add(T item)
    {
        if (_items.Contains(item))
            throw new InvalidOperationException($"Item already exists");
        
        _items.Add(item);
    }
}
```

### 9. Async/Await cohérent

```csharp
// ❌ SURPRENANT : Méthode async qui n'est pas vraiment async
public async Task<User> GetUserAsync(Guid id)
{
    return _cache.Get(id); // Synchrone malgré le nom !
}

// ❌ SURPRENANT : Méthode sync qui peut bloquer
public User GetUser(Guid id)
{
    return _repository.GetAsync(id).Result; // Bloque !
}

// ✅ PRÉVISIBLE : Cohérence sync/async
public User GetUser(Guid id)
{
    return _cache.Get(id); // Vraiment sync
}

public async Task<User> GetUserAsync(Guid id, CancellationToken ct)
{
    return await _repository.GetAsync(id, ct); // Vraiment async
}
```

## Conséquences

### Positives

- **Moins de bugs** : Le code fait ce qu'on attend
- **Lisibilité** : Le nom suffit à comprendre le comportement
- **Maintenabilité** : Modifications prévisibles
- **Onboarding** : Nouveaux développeurs comprennent rapidement
- **Confiance** : L'équipe peut se fier aux API

### Négatives

- **Noms plus longs** : Parfois nécessaires pour être explicites
  - *Mitigation* : La clarté vaut quelques caractères de plus
- **Plus de méthodes** : Séparer les comportements
  - *Mitigation* : Mieux que des surprises cachées

### Neutres

- POLA demande de se mettre à la place de l'utilisateur de l'API

## Alternatives considérées

### Option A : Documentation pour expliquer les surprises

- **Description** : Documenter les comportements non évidents
- **Avantages** : Code plus court
- **Inconvénients** : Documentation rarement lue, désynchronisation
- **Raison du rejet** : Le code clair est meilleur que la documentation

### Option B : Tests pour vérifier le comportement

- **Description** : S'appuyer sur les tests pour documenter
- **Avantages** : Tests comme documentation vivante
- **Inconvénients** : Ne résout pas le problème des surprises
- **Raison du rejet** : Les tests complètent mais ne remplacent pas POLA

## Références

- [Principle of Least Astonishment - Wikipedia](https://en.wikipedia.org/wiki/Principle_of_least_astonishment)
- [Clean Code - Robert C. Martin](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)
- [The Art of Readable Code - Dustin Boswell](https://www.amazon.com/Art-Readable-Code-Practical-Techniques/dp/0596802293)
