---
description: 'C# Testing Guidelines and Best Practices'
applyTo: '**/*Tests.cs'
---
- 1 fichier à tester -> 1 fichier de tests

## Testing Framework & Libraries
- Never use FluentAssertions or Moq
- Use NFluent for assertions (`Check.That()`, `Check.ThatCode()`)
- Use NSubstitute for mocks (`Substitute.For<T>()`, `Received()`, `DidNotReceive()`)
- Use xUnit for unit testing framework
- Use Bogus for realistic test data when needed
- Use Microsoft.Extensions.DependencyInjection.ServiceCollection for DI container testing
- Use Microsoft.AspNetCore.Mvc.Testing for ASP.NET Core integration tests
- Use System.Text.Json for JSON serialization in tests

## Naming Conventions
- Name test classes with "Tests" suffix (e.g., `UserServiceTests`)
- Name test methods using "MethodName_StateUnderTest_ExpectedBehavior" format
- Use descriptive English names for test methods
- Prefix test variables with clear names (`sut`, `mockRepository`, `expectedResult`)

## Test Structure
- Use regions to separate test sections (Arrange, Act, Assert)
- Use private methods to create test objects (SUT and dependencies creation)
- Use [Fact] attributes for simple unit tests and [Theory] with [InlineData] for parameterized tests
- Implement IDisposable to clean up resources after tests when necessary
- Use [Collection] attribute to control test execution order when needed
- Use IClassFixture<T> for expensive setup shared across test class
- Use ICollectionFixture<T> for shared context across multiple test classes
- Add XML documentation comments to complex test methods
- Use const or static readonly for test constants
## Quality Standards
- Maintain zero build warnings
- Maintain zero build errors
- Maintain zero skipped tests
- Achieve code coverage of at least 95%
- Ensure all tests pass
- Keep tests independent and isolated
- Execute unit tests under 30ms each
- Use C# comments to explain test purpose and complex logic
- Test edge cases and error scenarios thoroughly
- Use [Trait] attributes to categorize tests (e.g., [Trait("Category", "Unit")])

## Test Coverage Requirements
- Cover all code paths (branches, conditions)
- Test both success AND failure cases
- Test boundary values (null, empty, edge cases)
- Test expected exceptions using `Check.ThatCode(() => action).Throws<TException>()`
- Test asynchronous behaviors with async/await patterns
- Test cancellation token scenarios for async methods
- Test timeout scenarios for long-running operations
- Test concurrent access patterns for thread-safe code
- Test disposal patterns for IDisposable implementations
- Test validation scenarios for input parameters
## Best Practices
- Use clear and precise assertions to verify expected results
- Use explicit and meaningful variable names
- Follow C# and .NET coding conventions (appropriate PascalCase, camelCase)
- Use helpers and extensions to reduce test code duplication
- Organize tests for easy reading and maintenance
- Maintain consistent structure across all test files
- Mirror folder and file structure of tested projects
- Use constants for magic values in tests
- Implement builders or factories for complex object creation
- Use custom extension methods to improve assertion readability
## Mocking & Isolation
- Use mocks to isolate SUT from external dependencies
- Use NFluent assertions for expected exceptions
- Use NSubstitute assertions for method call verification
- Use assertions for returned method values
- Use xUnit fixtures to share context between tests when necessary
- Verify dependency interactions using `mock.Received(1).Method()`
- Use `Returns()` and `ReturnsForAnyArgs()` to configure mock behavior
- Use `Throws()` to simulate exceptions in dependencies
- Never mock value objects, DTOs, or simple data classes
- Prefer fakes over mocks for complex dependencies

## Data Management
- Use immutable test objects when possible
- Create test data using builders or Object Mother pattern
- Isolate data between tests (no shared mutable data)
- Use appropriate setup/cleanup methods ([SetUp], [TearDown])
- Clean up external resources (files, DB connections) after each test
- Use temporary directories for file system tests
- Use in-memory databases for data layer tests when appropriate
- Seed test data consistently across test runs
- Use deterministic random values with fixed seeds
- Avoid hardcoded dates, use relative dates or clock abstractions
## Project Structure
- Structurer les projets de tests en fonction du type de tests :
    - Unit : `{NameOfProject}.Units.Tests`
        - Fast tests (<30ms), isolated, no external dependencies
        - Cover business logic, algorithms, validations
    - Integration : `{NameOfProject}.Integrations.Tests`
        - Respawn DB between tests if needed
        - Use Testcontainers for dependent services (e.g., databases, message brokers)
        - Test interactions between components
        - Use WebApplicationFactory for API testing
    - Performance : `{NameOfProject}.Performances.Tests`
        - Use BenchmarkDotNet for micro-benchmarks
        - Use NBomber for load testing
        - Measure response time, throughput, memory usage
    - Acceptance : `{NameOfProject}.Acceptances.Tests`
        - Use SpecFlow for BDD (Behavior-Driven Development) tests
        - Use Playwright for .NET for web E2E testing
        - Test from user perspective
    - Architecture : `{NameOfProject}.Architectures.Tests`
        - Use NetArchTest to validate architectural rules
        - Use ArchUnitNET for dependency constraints
        - Verify naming conventions and structure

## Error Handling & Debugging
- Use descriptive assertion messages
- Log relevant information for debugging purposes
- Use `Check.WithCustomMessage()` for custom error messages
- Capture and verify logs using ITestOutputHelper
- Use snapshots to compare complex objects when necessary

## Continuous Integration
- Configure tests to run in parallel when possible
- Use xUnit collections to organize tests requiring specific order
- Mark slow tests with [Trait("Speed", "Slow")]
- Configure appropriate timeouts to prevent hanging tests
- Separate tests by environment (Dev, Staging, Prod) using traits
- Use [Skip] attribute with reason for temporarily disabled tests
- Configure test retry policies for flaky tests in CI
- Set up code coverage reporting with threshold enforcement
- Use test result XML output for CI dashboard integration
- Configure test categorization for different CI pipeline stages

## Security Testing
- Test authorization and authentication scenarios
- Validate input sanitization and SQL injection prevention
- Test HTTPS/TLS certificate validation in integration tests
- Verify sensitive data is not logged or exposed in test outputs
- Test rate limiting and throttling mechanisms
- Validate CORS policies in API tests

## Performance Considerations
- Measure and assert execution time for critical paths
- Test memory allocation and garbage collection impact
- Validate database query performance and N+1 problems
- Test caching mechanisms and cache invalidation
- Monitor resource usage during test execution
- Use profiling tools to identify performance bottlenecks

## Documentation & Maintenance
- Document test scenarios in README files
- Maintain test data setup scripts and documentation
- Document known test limitations and workarounds
- Keep test dependencies up to date
- Regular review and refactoring of test code
- Archive or remove obsolete tests