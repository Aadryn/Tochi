using LLMProxy.Infrastructure.Authorization.Abstractions;

namespace LLMProxy.Infrastructure.Authorization.Tests;

/// <summary>
/// Tests unitaires pour <see cref="AuthorizationRequest"/>.
/// </summary>
public class AuthorizationRequestTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var userId = "user@example.com";
        var relation = "can_view";
        var objectType = "tenant";
        var objectId = "tenant-123";

        // Act
        var request = new AuthorizationRequest(userId, relation, objectType, objectId);

        // Assert
        Check.That(request.UserId).IsEqualTo(userId);
        Check.That(request.Relation).IsEqualTo(relation);
        Check.That(request.ObjectType).IsEqualTo(objectType);
        Check.That(request.ObjectId).IsEqualTo(objectId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhiteSpaceUserId_ThrowsArgumentException(string? userId)
    {
        // Act & Assert
        Check.ThatCode(() => new AuthorizationRequest(userId!, "can_view", "tenant", "tenant-123"))
            .Throws<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhiteSpaceRelation_ThrowsArgumentException(string? relation)
    {
        // Act & Assert
        Check.ThatCode(() => new AuthorizationRequest("user@example.com", relation!, "tenant", "tenant-123"))
            .Throws<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhiteSpaceObjectType_ThrowsArgumentException(string? objectType)
    {
        // Act & Assert
        Check.ThatCode(() => new AuthorizationRequest("user@example.com", "can_view", objectType!, "tenant-123"))
            .Throws<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrWhiteSpaceObjectId_ThrowsArgumentException(string? objectId)
    {
        // Act & Assert
        Check.ThatCode(() => new AuthorizationRequest("user@example.com", "can_view", "tenant", objectId!))
            .Throws<ArgumentException>();
    }

    [Fact]
    public void GetFullObject_ReturnsCorrectFormat()
    {
        // Arrange
        var request = new AuthorizationRequest("user@example.com", "can_view", "tenant", "tenant-123");

        // Act
        var result = request.GetFullObject();

        // Assert
        Check.That(result).IsEqualTo("tenant:tenant-123");
    }

    [Theory]
    [InlineData("user@example.com", "user:user@example.com")]
    [InlineData("user:admin@test.com", "user:admin@test.com")]
    [InlineData("organization:org-1", "organization:org-1")]
    public void GetFullUser_ReturnsCorrectFormat(string userId, string expected)
    {
        // Arrange
        var request = new AuthorizationRequest(userId, "can_view", "tenant", "tenant-123");

        // Act
        var result = request.GetFullUser();

        // Assert
        Check.That(result).IsEqualTo(expected);
    }
}
