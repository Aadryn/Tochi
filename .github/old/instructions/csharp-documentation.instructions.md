---
applyTo: '**/*.cs'
---

## C# XML Documentation Requirements

### Mandatory Documentation
- Document ALL public classes, interfaces, enums, methods, properties, and fields
- Use triple-slash XML comments (///) only
- Write ALL documentation in French
- The document must be pertinent to the related code.

### Required XML Tags
- `<summary>`: Brief description (mandatory for all elements)
- `<param name="paramName">`: Parameter description (mandatory for each parameter)  
- `<returns>`: Return value description (mandatory for non-void methods)
- `<exception cref="ExceptionType">`: Exception conditions (mandatory for thrown exceptions)
- `<typeparam name="T">`: Generic type parameter description (mandatory for generics)

### Optional XML Tags
- `<remarks>`: Additional details when summary is insufficient
- `<example>`: Code examples for complex public APIs
- `<see cref="TypeOrMember"/>`: Cross-references to related code
- `<value>`: Property value description and constraints

### Writing Rules
- Start summaries with action verbs: "Gets", "Sets", "Calculates", "Validates"
- Use active voice and present tense
- Write complete sentences with periods
- Document behavior, not implementation
- Explain WHY when behavior is non-obvious
- Avoid repeating method/property names in descriptions
- Be concise but informative; aim for clarity
- Keep documentation up-to-date with code changes
- Keep documentation consistent across similar APIs
- Use proper grammar, punctuation, and spelling
- Keep documentation focused on the API's purpose and usage
- The documentation must be understandable by developers unfamiliar with the codebase, and that are not native English speakers. Use simple and meaningful words.

### Code Examples
- Add `<example>` blocks for public APIs with complex usage
- Show realistic scenarios with actual input/output values
- Include error handling patterns
- Use `<code>` blocks for multi-line examples, `<c>` for inline code

### Exception Documentation  
- Document ALL possible exceptions using `<exception cref="ExceptionType">`
- Specify exact conditions that trigger each exception
- Include exceptions from called dependencies when relevant

### Documentation Template
```csharp
/// <summary>
/// Calculates compound interest for investment over specified period.
/// </summary>
/// <param name="principal">Initial investment amount in currency units.</param>
/// <param name="rate">Annual interest rate as decimal (0.05 = 5%).</param>
/// <param name="years">Investment duration in years.</param>
/// <param name="frequency">Compounding frequency per year.</param>
/// <returns>Final investment value after compound interest.</returns>
/// <exception cref="ArgumentException">Thrown when <paramref name="rate"/> is negative or <paramref name="years"/> is negative.</exception>
/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="frequency"/> is less than 1.</exception>
/// <example>
/// <code>
/// decimal result = CalculateCompoundInterest(1000m, 0.05m, 5, 12);
/// // Returns: 1283.36 (5% annual rate, compounded monthly for 5 years)
/// </code>
/// </example>
public decimal CalculateCompoundInterest(decimal principal, decimal rate, int years, int frequency)
```
