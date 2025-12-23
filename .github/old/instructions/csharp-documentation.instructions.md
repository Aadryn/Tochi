---
applyTo: '**/*.cs'
---

## C# XML Documentation Requirements

### Core Principles

**ABSOLUTE RULE**: Document ONLY what exists in the current file. NEVER add information from external sources, AI generation, or assumptions.

**LANGUAGE**: ALL documentation MUST be written in American English using simple, clear words.

**AUDIENCE**: Documentation must be understandable by ALL developers, from complete beginners to experienced professionals.

**MAINTENANCE**: Documentation MUST be updated systematically whenever the code file is modified.

### Mandatory Documentation Scope

Document ALL public members in the current file:
- Classes (including nested/inner classes)
- Interfaces
- Structs
- Records
- Enums (and their individual values)
- Methods (instance and static)
- Properties (auto-properties, read-only, get/set)
- Fields (instance and static)
- Constants
- Events
- Delegates
- Indexers
- Constructors (all public constructors)
- Operators (overloaded operators)

### Required XML Tags

- `<summary>`: Short description of what the code element does (mandatory for all public members)
- `<param name="paramName">`: What each parameter is used for (mandatory for each parameter)
- `<returns>`: What the method gives back (mandatory for non-void methods)
- `<exception cref="ExceptionType">`: When and why this exception happens (mandatory for each thrown exception)
- `<typeparam name="T">`: What the generic type represents (mandatory for generic parameters)

### Optional XML Tags

- `<remarks>`: Extra details when summary is not enough
- `<example>`: Code samples showing how to use the member (strongly recommended for complex APIs)
- `<see cref="TypeOrMember"/>`: Links to related code elements IN THE SAME FILE
- `<value>`: What the property holds and any limits

### Writing Rules

✅ **DO:**
- Start summaries with simple action words: "Gets", "Sets", "Finds", "Checks", "Creates"
- Use active voice and present tense
- Write complete sentences with periods
- Describe WHAT the code does, not HOW it does it
- Explain WHY when the purpose is not obvious from the name
- Use simple, common words that beginners can understand
- Give concrete examples with real values
- Update documentation every time you change the code

❌ **DO NOT:**
- Repeat the method/property name in the description
- Add information that is not in the current file
- Assume or invent behavior
- Use complex technical jargon without explanation
- Leave documentation outdated when code changes
- Document implementation details (internal logic)
- Reference external files or systems without context

### Documentation Content Rules

**CRITICAL**: Document ONLY based on:
- The actual code present in the file
- The direct behavior visible in the method/property
- Exceptions explicitly thrown in the code
- Parameters actually used in the signature

**FORBIDDEN**: DO NOT document:
- Assumed behavior from other files
- Generated or hypothetical scenarios
- External dependencies not directly visible
- Implementation details that might change

### Code Examples

- Add `<example>` blocks for all public APIs that are not immediately obvious
- Show real usage with actual values
- Include the expected result or outcome
- Show error cases when relevant
- Use `<code>` blocks for multi-line examples
- Use `<c>` for inline code references

### Exception Documentation

- Document ALL exceptions thrown directly in the method
- Explain the exact condition that causes the exception
- Use `<paramref>` to reference the parameter that causes the exception
- DO NOT document exceptions from methods you call (unless critical for understanding)

### Update Requirements

**MANDATORY**: When modifying a C# file, you MUST:
1. Review all XML documentation in the file
2. Update summaries if behavior changed
3. Update parameter descriptions if parameters changed
4. Update return descriptions if return type/behavior changed
5. Update exception documentation if error handling changed
6. Add examples if new complex functionality was added

### Documentation Template

```csharp
/// <summary>
/// Finds the total price after applying a discount.
/// </summary>
/// <param name="originalPrice">The starting price before discount.</param>
/// <param name="discountPercent">The discount to apply, between 0 and 100.</param>
/// <returns>The final price after discount is applied.</returns>
/// <exception cref="ArgumentOutOfRangeException">
/// Thrown when <paramref name="discountPercent"/> is less than 0 or greater than 100.
/// </exception>
/// <example>
/// <code>
/// decimal finalPrice = CalculateDiscountedPrice(100m, 20m);
/// // Returns: 80.00 (100 - 20% = 80)
/// </code>
/// </example>
public decimal CalculateDiscountedPrice(decimal originalPrice, decimal discountPercent)
{
    if (discountPercent < 0 || discountPercent > 100)
        throw new ArgumentOutOfRangeException(nameof(discountPercent));
    
    return originalPrice * (1 - discountPercent / 100);
}
```

### Simple Language Examples

❌ **TOO COMPLEX:**
```csharp
/// <summary>
/// Instantiates a repository implementation utilizing dependency injection to facilitate data persistence operations.
/// </summary>
```

✅ **SIMPLE AND CLEAR:**
```csharp
/// <summary>
/// Creates a new repository that saves data to the database.
/// </summary>
```

### Consistency Rules

- Use the same words for the same concepts throughout the file
- Follow the same structure for similar members
- Keep the same level of detail for similar complexity
- Update all related documentation when changing one part

## Documentation by Code Element Type

This section explains HOW to document each type of C# element. Each section includes:
- What information to provide
- Which XML tags to use
- Concrete examples

### Classes and Structs

**Purpose**: Explain what the class/struct represents and its main responsibility.

**Required tags**: `<summary>`, `<typeparam>` (for generic types)

**Writing guide**:
- Start with what the class represents: "Represents...", "Provides...", "Defines..."
- Explain the main purpose in one clear sentence
- If the class has a specific pattern (Builder, Factory, Repository), mention it

```csharp
/// <summary>
/// Represents a user account in the system with authentication information.
/// </summary>
public class UserAccount
{
    // ...
}

/// <summary>
/// Provides methods to create and validate email addresses.
/// </summary>
public static class EmailValidator
{
    // ...
}

/// <summary>
/// Stores the result of an operation with either a success value or an error.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public class Result<T>
{
    // ...
}
```

### Interfaces

**Purpose**: Describe the contract that implementing classes must fulfill.

**Required tags**: `<summary>`, `<typeparam>` (for generic interfaces)

**Writing guide**:
- Explain what capability or behavior the interface defines
- Use "Defines...", "Provides contract for...", "Represents capability to..."
- Focus on WHAT implementers must do, not HOW

```csharp
/// <summary>
/// Defines methods for saving and loading data.
/// </summary>
public interface IRepository
{
    // ...
}

/// <summary>
/// Represents the ability to send notifications to users.
/// </summary>
public interface INotificationSender
{
    // ...
}
```

### Properties

**Purpose**: Explain what information the property holds or provides access to.

**Required tags**: `<summary>`, `<value>` (optional but recommended)

**Writing guide**:
- For get-only: "Gets the [description]"
- For get/set: "Gets or sets the [description]"
- Include valid value ranges or constraints
- Mention if the property can be null

```csharp
/// <summary>
/// Gets or sets the user's email address.
/// </summary>
/// <value>A valid email address string, or null if not set.</value>
public string? Email { get; set; }

/// <summary>
/// Gets the unique identifier for this user.
/// </summary>
/// <value>A positive integer that never changes after creation.</value>
public int Id { get; }

/// <summary>
/// Gets or sets the age in years.
/// </summary>
/// <value>A number between 0 and 150.</value>
public int Age { get; set; }

/// <summary>
/// Gets whether the user account is currently active.
/// </summary>
/// <value>True if the account can be used; otherwise, false.</value>
public bool IsActive { get; }
```

### Fields

**Purpose**: Describe what data the field stores.

**Required tags**: `<summary>`

**Writing guide**:
- Explain what the field holds
- For constants, explain the meaning of the value
- For static fields, explain when/how it's shared
- Mention thread-safety if relevant

```csharp
/// <summary>
/// The maximum number of login attempts allowed before locking the account.
/// </summary>
public const int MaxLoginAttempts = 5;

/// <summary>
/// The default timeout in seconds for all network operations.
/// </summary>
public static readonly int DefaultTimeoutSeconds = 30;

/// <summary>
/// Stores the creation timestamp when this instance was created.
/// </summary>
private readonly DateTime _createdAt;

/// <summary>
/// Tracks the current number of failed login attempts.
/// </summary>
private int _failedAttempts;
```

### Constants

**Purpose**: Explain the meaning and use of the constant value.

**Required tags**: `<summary>`

**Writing guide**:
- Explain what the constant represents
- Include the actual value's meaning
- Explain when/why this value is used

```csharp
/// <summary>
/// The minimum password length required for security, which is 8 characters.
/// </summary>
public const int MinPasswordLength = 8;

/// <summary>
/// The database connection timeout in seconds, set to 30 seconds.
/// </summary>
private const int DatabaseTimeoutSeconds = 30;

/// <summary>
/// The default page size for list results, showing 20 items per page.
/// </summary>
public const int DefaultPageSize = 20;
```

### Methods

**Purpose**: Describe what the method does, its inputs, output, and possible errors.

**Required tags**: `<summary>`, `<param>`, `<returns>`, `<exception>`, `<typeparam>`

**Writing guide**:
- Start with an action verb: "Finds", "Creates", "Validates", "Calculates"
- Explain what the method accomplishes
- Document each parameter's purpose
- Explain what the method returns
- Document all exceptions that can be thrown

```csharp
/// <summary>
/// Adds two numbers together.
/// </summary>
/// <param name="first">The first number to add.</param>
/// <param name="second">The second number to add.</param>
/// <returns>The sum of the two numbers.</returns>
public int Add(int first, int second)
{
    return first + second;
}

/// <summary>
/// Finds a user by their email address.
/// </summary>
/// <param name="email">The email address to search for.</param>
/// <returns>The user with the matching email, or null if not found.</returns>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="email"/> is null.
/// </exception>
/// <exception cref="ArgumentException">
/// Thrown when <paramref name="email"/> is empty or whitespace.
/// </exception>
public User? FindByEmail(string email)
{
    if (email == null) throw new ArgumentNullException(nameof(email));
    if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.", nameof(email));
    
    // ... search logic
}
```

### Static Methods

**Purpose**: Explain what the static method does and when to use it.

**Required tags**: Same as instance methods

**Writing guide**:
- Clearly state if the method is a helper/utility function
- Explain that it doesn't require an instance
- Same rules as regular methods apply

```csharp
/// <summary>
/// Creates a new user account with the specified email and password.
/// This is a factory method that handles all initialization logic.
/// </summary>
/// <param name="email">The user's email address.</param>
/// <param name="password">The user's password.</param>
/// <returns>A new UserAccount instance ready to use.</returns>
/// <exception cref="ArgumentException">
/// Thrown when <paramref name="email"/> is invalid or <paramref name="password"/> is too short.
/// </exception>
public static UserAccount Create(string email, string password)
{
    // ... validation and creation
}
```

### Constructors

**Purpose**: Explain what the constructor initializes and any setup it performs.

**Required tags**: `<summary>`, `<param>`, `<exception>`

**Writing guide**:
- Use "Initializes a new instance of..." or "Creates a new..."
- Explain what state the object will be in after construction
- Document all parameters
- Document validation exceptions

```csharp
/// <summary>
/// Initializes a new instance of the UserAccount class with the specified email.
/// </summary>
/// <param name="email">The user's email address.</param>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="email"/> is null.
/// </exception>
public UserAccount(string email)
{
    Email = email ?? throw new ArgumentNullException(nameof(email));
}

/// <summary>
/// Initializes a new instance of the UserAccount class with default values.
/// The account will be inactive and have no email set.
/// </summary>
public UserAccount()
{
    IsActive = false;
    Email = null;
}
```

### Enums

**Purpose**: Explain what the enum represents and document each value.

**Required tags**: `<summary>` for enum and each value

**Writing guide**:
- For enum: Explain what set of values it represents
- For each value: Explain what that specific option means
- Include when each value should be used

```csharp
/// <summary>
/// Represents the different states a user account can be in.
/// </summary>
public enum AccountStatus
{
    /// <summary>
    /// The account is newly created but not yet verified.
    /// User cannot log in with this status.
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// The account is verified and can be used normally.
    /// User can log in and access all features.
    /// </summary>
    Active = 1,
    
    /// <summary>
    /// The account is temporarily suspended.
    /// User cannot log in until reactivated.
    /// </summary>
    Suspended = 2,
    
    /// <summary>
    /// The account is permanently closed and cannot be used.
    /// All data will be deleted after 30 days.
    /// </summary>
    Closed = 3
}
```

### Events

**Purpose**: Explain when the event is raised and what data it provides.

**Required tags**: `<summary>`

**Writing guide**:
- Explain when the event fires
- Describe what information the event provides
- Mention if handlers are called in a specific order

```csharp
/// <summary>
/// Occurs when the user successfully logs in.
/// This event fires after password validation but before session creation.
/// </summary>
public event EventHandler<LoginEventArgs>? LoginSuccessful;

/// <summary>
/// Occurs when any property value changes.
/// Handlers receive the name of the property that changed.
/// </summary>
public event PropertyChangedEventHandler? PropertyChanged;
```

### Delegates

**Purpose**: Explain what the delegate represents and when it's invoked.

**Required tags**: `<summary>`, `<param>`, `<returns>`

**Writing guide**:
- Explain what operation the delegate represents
- Document parameters as you would for methods
- Explain the expected return value

```csharp
/// <summary>
/// Represents a method that validates a user input value.
/// </summary>
/// <param name="value">The value to validate.</param>
/// <returns>True if the value is valid; otherwise, false.</returns>
public delegate bool ValidationDelegate(string value);

/// <summary>
/// Represents a method that transforms an input value into an output value.
/// </summary>
/// <typeparam name="TInput">The type of the input value.</typeparam>
/// <typeparam name="TOutput">The type of the output value.</typeparam>
/// <param name="input">The value to transform.</param>
/// <returns>The transformed value.</returns>
public delegate TOutput TransformDelegate<TInput, TOutput>(TInput input);
```

### Indexers

**Purpose**: Explain what the indexer provides access to.

**Required tags**: `<summary>`, `<param>`, `<value>`, `<exception>`

**Writing guide**:
- Explain what element the indexer returns
- Document the index parameter
- Explain valid index ranges
- Document out-of-range exceptions

```csharp
/// <summary>
/// Gets or sets the item at the specified position in the collection.
/// </summary>
/// <param name="index">The zero-based position of the item to get or set.</param>
/// <value>The item at the specified position.</value>
/// <exception cref="ArgumentOutOfRangeException">
/// Thrown when <paramref name="index"/> is less than 0 or greater than or equal to the collection size.
/// </exception>
public string this[int index]
{
    get { /* ... */ }
    set { /* ... */ }
}

/// <summary>
/// Gets the user with the specified email address from the collection.
/// </summary>
/// <param name="email">The email address of the user to find.</param>
/// <value>The user with the matching email, or null if not found.</value>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="email"/> is null.
/// </exception>
public User? this[string email]
{
    get { /* ... */ }
}
```

### Operators

**Purpose**: Explain what the operator does and when it's useful.

**Required tags**: `<summary>`, `<param>`, `<returns>`

**Writing guide**:
- Explain the operation in clear terms
- Show how the operator behaves
- Give examples when helpful

```csharp
/// <summary>
/// Adds two money values together.
/// </summary>
/// <param name="left">The first money value.</param>
/// <param name="right">The second money value.</param>
/// <returns>A new Money instance with the sum of both values.</returns>
public static Money operator +(Money left, Money right)
{
    return new Money(left.Amount + right.Amount);
}

/// <summary>
/// Checks if two user accounts are equal by comparing their IDs.
/// </summary>
/// <param name="left">The first user account to compare.</param>
/// <param name="right">The second user account to compare.</param>
/// <returns>True if both accounts have the same ID; otherwise, false.</returns>
public static bool operator ==(UserAccount left, UserAccount right)
{
    return left?.Id == right?.Id;
}
```

### Records

**Purpose**: Explain what the record represents and its key characteristics.

**Required tags**: Same as classes, plus document primary constructor parameters

**Writing guide**:
- Explain what data the record holds
- Document immutability if applicable
- Document all parameters in primary constructor

```csharp
/// <summary>
/// Represents an immutable user profile with name and email.
/// Once created, the values cannot be changed.
/// </summary>
/// <param name="Name">The user's full name.</param>
/// <param name="Email">The user's email address.</param>
public record UserProfile(string Name, string Email);

/// <summary>
/// Represents the result of a login attempt with status and user information.
/// </summary>
/// <param name="Success">True if login succeeded; otherwise, false.</param>
/// <param name="User">The logged-in user, or null if login failed.</param>
/// <param name="ErrorMessage">The error message if login failed, or null if successful.</param>
public record LoginResult(bool Success, User? User, string? ErrorMessage);
```

### Nested/Inner Classes

**Purpose**: Explain why the class is nested and its relationship to the outer class.

**Required tags**: Same as regular classes

**Writing guide**:
- Explain the relationship to the containing class
- State if it's only used internally
- Explain when it's appropriate to use

```csharp
/// <summary>
/// Manages user authentication and session handling.
/// </summary>
public class AuthenticationService
{
    /// <summary>
    /// Holds configuration settings specific to this authentication service.
    /// This class is only used internally and cannot be created outside.
    /// </summary>
    private class AuthConfig
    {
        // ...
    }
}
```

## Advanced Documentation Scenarios

### Async Methods

Document async methods with special attention to when they complete.

```csharp
/// <summary>
/// Saves the user data to the database asynchronously.
/// This method returns immediately and completes when the database write finishes.
/// </summary>
/// <param name="user">The user to save.</param>
/// <returns>
/// A task that completes when the save operation finishes.
/// The task result is true if saved successfully; otherwise, false.
/// </returns>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="user"/> is null.
/// </exception>
public async Task<bool> SaveUserAsync(User user)
{
    // ...
}
```

### Generic Types

Always document what each type parameter represents.

```csharp
/// <summary>
/// Wraps a value with additional metadata like timestamps and status.
/// </summary>
/// <typeparam name="T">The type of value being wrapped.</typeparam>
public class Wrapper<T>
{
    /// <summary>
    /// Gets the wrapped value.
    /// </summary>
    /// <value>The value of type <typeparamref name="T"/>.</value>
    public T Value { get; }
}
```

### Extension Methods

Clearly indicate that it's an extension method and what type it extends.

```csharp
/// <summary>
/// Checks if a string is a valid email address.
/// This is an extension method for the String class.
/// </summary>
/// <param name="value">The string to check.</param>
/// <returns>True if the string is a valid email; otherwise, false.</returns>
public static bool IsValidEmail(this string value)
{
    // ...
}
```

## Tone and Style Guidelines

### Didactic Approach

Explain concepts as if teaching someone who has never seen the code before:

```csharp
/// <summary>
/// Calculates the area of a circle using the formula: π × radius².
/// A circle's area is the space inside its edge.
/// </summary>
/// <param name="radius">The distance from the center to the edge of the circle.</param>
/// <returns>The area in the same units squared as the radius (e.g., if radius is in meters, result is in square meters).</returns>
public double CalculateCircleArea(double radius)
{
    return Math.PI * radius * radius;
}
```

### Descriptive Details

Provide context and explain the purpose:

```csharp
/// <summary>
/// Validates that a password meets security requirements.
/// The password must be at least 8 characters long and contain at least one number.
/// This helps protect user accounts from unauthorized access.
/// </summary>
/// <param name="password">The password to check.</param>
/// <returns>True if the password meets all requirements; otherwise, false.</returns>
public bool IsPasswordStrong(string password)
{
    // ...
}
```

### Explanatory Examples

When behavior might be unclear, add examples:

```csharp
/// <summary>
/// Formats a number as currency with the appropriate symbol and decimal places.
/// For example, 1234.5 becomes "$1,234.50" in USD format.
/// </summary>
/// <param name="amount">The number to format.</param>
/// <param name="currencyCode">The currency code like "USD", "EUR", or "GBP".</param>
/// <returns>The formatted currency string with symbol and proper decimal places.</returns>
/// <example>
/// <code>
/// string result = FormatCurrency(1234.5, "USD");
/// // result = "$1,234.50"
/// 
/// string euro = FormatCurrency(1234.5, "EUR");
/// // euro = "€1,234.50"
/// </code>
/// </example>
public string FormatCurrency(decimal amount, string currencyCode)
{
    // ...
}
```
