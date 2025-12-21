---
applyTo: "**/*.cs"
---

# C# Coding Standards

## General Guidelines
- Write clean, readable, and maintainable code.
- Follow consistent naming conventions throughout your codebase.
- Limit class responsibilities to a single purpose.
- Include XML documentation for all public APIs.

## Naming Conventions
* Use PascalCase for classes, methods, properties, and events.
* Use camelCase for variables, parameters, and private fields.
* Use PascalCase with `I` prefix for interfaces.
* Use UPPER_CASE for constants.
* Choose descriptive names over abbreviations.

```csharp
public class UserService
{
    public string GetUserName() { }
    public int UserId { get; set; }
}

public interface IUserRepository { }
public const int MAX_RETRY_COUNT = 3;
```

## Class Organization
* Order class members consistently:
  1. Constants
  2. Private fields
  3. Public properties
  4. Constructors
  5. Public methods
  6. Private methods
* Use regions only for major logical sections.
* Keep classes focused on a single responsibility.

## Type Declarations
* Use `var` only when the type is obvious from the right side.
* Use explicit nullable types with C# 8+.
* Initialize collections with modern syntax.

```csharp
var users = new List<User>();        // Good - type is clear
List<User> result = GetUsers();      // Good - type not obvious

public string? Name { get; set; }    // Explicit nullable

var numbers = new List<int> { 1, 2, 3 };
var dict = new Dictionary<string, int>
{
    ["key1"] = 1,
    ["key2"] = 2
};
```

## Error Handling
* Throw specific exceptions, never generic `Exception`.
* Document exceptions in XML comments.
* Use `using` statements for resource management.
* Validate parameters at method entry points.

```csharp
/// <exception cref="ArgumentNullException">Thrown when userId is null</exception>
public User GetUser(int? userId)
{
    if (userId == null)
        throw new ArgumentNullException(nameof(userId));
    
    using var connection = new SqlConnection(connectionString);
    // implementation
}
```

## Asynchronous Programming
* Suffix async methods with `Async`.
* Return `Task` or `Task<T>`, never `void` (except event handlers).
* Use `ConfigureAwait(false)` in libraries to avoid deadlocks.
* Handle cancellation tokens where appropriate.

```csharp
public async Task<User> GetUserAsync(int userId, CancellationToken cancellationToken = default)
{
    var result = await repository.FindAsync(userId, cancellationToken).ConfigureAwait(false);
    return result;
}
```

## LINQ and Collections
* Write readable LINQ queries with proper formatting.
* Prefer LINQ methods over loops when they improve readability.
* Use `Any()` instead of `Count() > 0` for existence checks.
* Use appropriate collection types for performance.

```csharp
var activeUsers = users
    .Where(u => u.IsActive)
    .OrderBy(u => u.Name)
    .Select(u => u.Email)
    .ToList();

bool hasActiveUsers = users.Any(u => u.IsActive);  // Not Count() > 0
```

## Code Formatting
* Use 4 spaces for indentation, never tabs.
* Place opening braces on new lines (Allman style).
* Limit lines to 120 characters maximum.
* Add spaces around operators and after commas.
* Use blank lines to separate logical code groups.

```csharp
if (condition)
{
    DoSomething();
}

var result = a + b * c;
```

## Best Practices
* Validate parameters at method entry points.
* Use `string.IsNullOrEmpty()` or `string.IsNullOrWhiteSpace()` for string validation.
* Prefer immutability when possible - use `readonly` for fields.
* Limit methods to 4-5 parameters - use configuration objects for more.
* Implement `IDisposable` correctly when managing unmanaged resources.

```csharp
public void ProcessUser(User user)
{
    if (user == null)
        throw new ArgumentNullException(nameof(user));
    
    // business logic
}
```

## Performance Guidelines
* Use `StringBuilder` for multiple string concatenations.
* Prefer generic collections (`List<T>`, `Dictionary<TKey, TValue>`).
* Use `StringComparison.OrdinalIgnoreCase` for case-insensitive comparisons.
* Avoid unnecessary allocations in performance-critical loops.
* Consider memory usage patterns in high-frequency operations.

```csharp
var sb = new StringBuilder();
foreach (var item in items)
{
    sb.AppendLine(item.ToString());
}
return sb.ToString();
```