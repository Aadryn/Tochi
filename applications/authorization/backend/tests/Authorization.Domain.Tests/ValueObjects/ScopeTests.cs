using Authorization.Domain.ValueObjects;
using NFluent;
using Xunit;

namespace Authorization.Domain.Tests.ValueObjects;

/// <summary>
/// Tests unitaires pour le Value Object Scope.
/// </summary>
public class ScopeTests
{
    #region Parse - Cas Valides

    [Theory]
    [InlineData("api.llmproxy.com")]
    [InlineData("api.llmproxy.com/organizations/org-123")]
    [InlineData("api.llmproxy.com/organizations/org-123/tenants/tenant-456")]
    [InlineData("domain.example.com/projects/main")]
    public void Parse_ValidPath_ReturnsScope(string path)
    {
        // Act
        var scope = Scope.Parse(path);

        // Assert
        Check.That(scope).IsNotNull();
        Check.That(scope.Path).IsNotEmpty();
    }

    [Fact]
    public void Parse_DomainOnly_HasDepthOne()
    {
        // Arrange
        const string path = "api.llmproxy.com";

        // Act
        var scope = Scope.Parse(path);

        // Assert
        Check.That(scope.Depth).IsStrictlyGreaterThan(0);
        Check.That(scope.Domain).IsEqualTo("api.llmproxy.com");
    }

    [Fact]
    public void Parse_WithOrganization_ContainsSegments()
    {
        // Arrange
        const string path = "api.llmproxy.com/organizations/org-123";

        // Act
        var scope = Scope.Parse(path);

        // Assert
        Check.That(scope.Segments.Count).IsStrictlyGreaterThan(1);
    }

    #endregion

    #region Parse - Cas Invalides

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Parse_EmptyOrWhitespace_ThrowsArgumentException(string? path)
    {
        // Act & Assert
        Check.ThatCode(() => Scope.Parse(path!))
            .Throws<ArgumentException>();
    }

    [Fact]
    public void Parse_PathWithSpaces_ThrowsArgumentException()
    {
        // Arrange
        const string path = "api.llmproxy.com/organizations/org 123";

        // Act & Assert
        Check.ThatCode(() => Scope.Parse(path))
            .Throws<ArgumentException>();
    }

    [Theory]
    [InlineData("http://api.llmproxy.com")]
    [InlineData("https://api.llmproxy.com")]
    [InlineData("HTTP://API.LLMPROXY.COM")]
    public void Parse_PathWithHttpScheme_ThrowsArgumentException(string path)
    {
        // Act & Assert
        Check.ThatCode(() => Scope.Parse(path))
            .Throws<ArgumentException>();
    }

    [Fact]
    public void Parse_SlashOnly_ThrowsArgumentException()
    {
        // Arrange
        const string path = "/";

        // Act & Assert
        Check.ThatCode(() => Scope.Parse(path))
            .Throws<ArgumentException>();
    }

    #endregion

    #region Hierarchy

    [Fact]
    public void Contains_ParentContainsChild_ReturnsTrue()
    {
        // Arrange
        var parent = Scope.Parse("api.llmproxy.com/organizations/org-123");
        var child = Scope.Parse("api.llmproxy.com/organizations/org-123/tenants/tenant-456");

        // Act
        var contains = parent.Contains(child);

        // Assert
        Check.That(contains).IsTrue();
    }

    [Fact]
    public void Contains_SameScope_ReturnsFalse_StrictDescendantRelation()
    {
        // Arrange
        var scope = Scope.Parse("api.llmproxy.com/organizations/org-123");

        // Act - Un scope ne se contient pas lui-même (relation stricte descendant)
        var contains = scope.Contains(scope);

        // Assert - Contains vérifie IsDescendantOf qui est une relation stricte
        Check.That(contains).IsFalse();
    }

    [Fact]
    public void Contains_ChildDoesNotContainParent_ReturnsFalse()
    {
        // Arrange
        var parent = Scope.Parse("api.llmproxy.com/organizations/org-123");
        var child = Scope.Parse("api.llmproxy.com/organizations/org-123/tenants/tenant-456");

        // Act
        var contains = child.Contains(parent);

        // Assert
        Check.That(contains).IsFalse();
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_SameScope_ReturnsTrue()
    {
        // Arrange
        var scope1 = Scope.Parse("api.llmproxy.com/organizations/org-123");
        var scope2 = Scope.Parse("api.llmproxy.com/organizations/org-123");

        // Act & Assert
        Check.That(scope1).IsEqualTo(scope2);
    }

    [Fact]
    public void Equals_DifferentScope_ReturnsFalse()
    {
        // Arrange
        var scope1 = Scope.Parse("api.llmproxy.com/organizations/org-123");
        var scope2 = Scope.Parse("api.llmproxy.com/organizations/org-456");

        // Act & Assert
        Check.That(scope1).IsNotEqualTo(scope2);
    }

    #endregion
}
