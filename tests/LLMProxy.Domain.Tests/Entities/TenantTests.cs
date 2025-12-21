using FluentAssertions;
using LLMProxy.Domain.Entities;
using Xunit;

namespace LLMProxy.Domain.Tests.Entities;

/// <summary>
/// Unit tests for Tenant entity
/// Following TDD Red-Green-Refactor approach
/// </summary>
public class TenantTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var name = "Test Tenant";
        var slug = "test-tenant";
        var settings = TenantSettings.Default();

        // Act
        var result = Tenant.Create(name, slug, settings);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Slug.Should().Be(slug);
        result.Value.IsActive.Should().BeTrue();
        result.Value.Settings.Should().Be(settings);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldFail(string? invalidName)
    {
        // Arrange
        var slug = "test-tenant";

        // Act
        var result = Tenant.Create(invalidName!, slug);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Invalid Slug")]
    [InlineData("invalid_slug")]
    [InlineData("INVALID")]
    public void Create_WithInvalidSlug_ShouldFail(string? invalidSlug)
    {
        // Arrange
        var name = "Test Tenant";

        // Act
        var result = Tenant.Create(name, invalidSlug!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("slug");
    }

    [Fact]
    public void Create_WithValidSlug_ShouldAccept()
    {
        // Arrange
        var name = "Test Tenant";
        var validSlugs = new[] { "test", "test-tenant", "test-123", "abc123" };

        foreach (var slug in validSlugs)
        {
            // Act
            var result = Tenant.Create(name, slug);

            // Assert
            result.IsSuccess.Should().BeTrue($"slug '{slug}' should be valid");
        }
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSucceed()
    {
        // Arrange
        var tenant = Tenant.Create("Test", "test").Value;

        // Act
        var result = tenant.Deactivate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        tenant.IsActive.Should().BeFalse();
        tenant.DeactivatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldFail()
    {
        // Arrange
        var tenant = Tenant.Create("Test", "test").Value;
        tenant.Deactivate();

        // Act
        var result = tenant.Deactivate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already deactivated");
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSucceed()
    {
        // Arrange
        var tenant = Tenant.Create("Test", "test").Value;
        tenant.Deactivate();

        // Act
        var result = tenant.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        tenant.IsActive.Should().BeTrue();
        tenant.DeactivatedAt.Should().BeNull();
    }

    [Fact]
    public void UpdateSettings_WithNewSettings_ShouldSucceed()
    {
        // Arrange
        var tenant = Tenant.Create("Test", "test").Value;
        var newSettings = new TenantSettings(
            maxUsers: 200,
            maxProviders: 20,
            enableAuditLogging: false,
            auditRetentionDays: 30,
            enableResponseCache: false
        );

        // Act
        var result = tenant.UpdateSettings(newSettings);

        // Assert
        result.IsSuccess.Should().BeTrue();
        tenant.Settings.Should().Be(newSettings);
        tenant.Settings.MaxUsers.Should().Be(200);
        tenant.Settings.EnableAuditLogging.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var tenant1 = Tenant.Create("Test1", "test1").Value;
        var tenant2 = Tenant.Create("Test2", "test2").Value;
        
        // Use reflection to set same ID for testing
        var idProperty = typeof(Tenant).BaseType!.GetProperty("Id")!;
        idProperty.SetValue(tenant2, tenant1.Id);

        // Act & Assert
        tenant1.Should().Be(tenant2);
        (tenant1 == tenant2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var tenant1 = Tenant.Create("Test1", "test1").Value;
        var tenant2 = Tenant.Create("Test2", "test2").Value;

        // Act & Assert
        tenant1.Should().NotBe(tenant2);
        (tenant1 != tenant2).Should().BeTrue();
    }
}
