using Authorization.Domain.ValueObjects;
using NFluent;
using Xunit;

namespace Authorization.Domain.Tests.ValueObjects;

/// <summary>
/// Tests unitaires pour le Value Object PrincipalId.
/// </summary>
public class PrincipalIdTests
{
    #region Create

    [Fact]
    public void Create_ValidGuid_ReturnsPrincipalId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var principalId = PrincipalId.Create(guid);

        // Assert
        Check.That(principalId.Value).IsEqualTo(guid);
    }

    [Fact]
    public void Create_EmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act & Assert
        Check.ThatCode(() => PrincipalId.Create(emptyGuid))
            .Throws<ArgumentException>();
    }

    #endregion

    #region Parse

    [Theory]
    [InlineData("550e8400-e29b-41d4-a716-446655440000")]
    [InlineData("550E8400-E29B-41D4-A716-446655440000")]
    [InlineData("550e8400e29b41d4a716446655440000")]
    public void Parse_ValidGuidString_ReturnsPrincipalId(string value)
    {
        // Act
        var principalId = PrincipalId.Parse(value);

        // Assert
        Check.That(principalId.Value).IsNotEqualTo(Guid.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Parse_EmptyOrNull_ThrowsArgumentException(string? value)
    {
        // Act & Assert
        Check.ThatCode(() => PrincipalId.Parse(value!))
            .Throws<ArgumentException>();
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("12345")]
    [InlineData("xxx-xxx-xxx")]
    public void Parse_InvalidFormat_ThrowsArgumentException(string value)
    {
        // Act & Assert
        Check.ThatCode(() => PrincipalId.Parse(value))
            .Throws<ArgumentException>();
    }

    [Fact]
    public void Parse_EmptyGuidString_ThrowsArgumentException()
    {
        // Arrange
        const string emptyGuidString = "00000000-0000-0000-0000-000000000000";

        // Act & Assert
        Check.ThatCode(() => PrincipalId.Parse(emptyGuidString))
            .Throws<ArgumentException>();
    }

    #endregion

    #region TryParse

    [Fact]
    public void TryParse_ValidGuid_ReturnsTrueAndValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var stringValue = guid.ToString();

        // Act
        var success = PrincipalId.TryParse(stringValue, out var result);

        // Assert
        Check.That(success).IsTrue();
        Check.That(result.Value).IsEqualTo(guid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void TryParse_InvalidValue_ReturnsFalse(string? value)
    {
        // Act
        var success = PrincipalId.TryParse(value, out var result);

        // Assert
        Check.That(success).IsFalse();
        Check.That(result).IsEqualTo(default(PrincipalId));
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_SamePrincipalId_ReturnsTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = PrincipalId.Create(guid);
        var id2 = PrincipalId.Create(guid);

        // Act & Assert
        Check.That(id1).IsEqualTo(id2);
    }

    [Fact]
    public void Equals_DifferentPrincipalId_ReturnsFalse()
    {
        // Arrange
        var id1 = PrincipalId.Create(Guid.NewGuid());
        var id2 = PrincipalId.Create(Guid.NewGuid());

        // Act & Assert
        Check.That(id1).IsNotEqualTo(id2);
    }

    #endregion

    #region Conversion

    [Fact]
    public void ImplicitConversion_ToGuid_ReturnsValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var principalId = PrincipalId.Create(guid);

        // Act
        Guid result = principalId;

        // Assert
        Check.That(result).IsEqualTo(guid);
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var principalId = PrincipalId.Create(guid);

        // Act
        var result = principalId.ToString();

        // Assert
        Check.That(result).IsEqualTo(guid.ToString());
    }

    #endregion

    #region ToOpenFgaFormat

    [Theory]
    [InlineData(PrincipalType.User, "user:")]
    [InlineData(PrincipalType.Group, "group:")]
    [InlineData(PrincipalType.ServiceAccount, "serviceaccount:")]
    public void ToOpenFgaFormat_WithType_ReturnsCorrectFormat(PrincipalType type, string expectedPrefix)
    {
        // Arrange
        var guid = Guid.NewGuid();
        var principalId = PrincipalId.Create(guid);

        // Act
        var result = principalId.ToOpenFgaFormat(type);

        // Assert
        Check.That(result).StartsWith(expectedPrefix);
        Check.That(result).EndsWith(guid.ToString());
    }

    #endregion
}
