using Authorization.Domain.ValueObjects;
using NFluent;
using Xunit;

namespace Authorization.Domain.Tests.ValueObjects;

/// <summary>
/// Tests unitaires pour l'enum PrincipalType et ses extensions.
/// </summary>
public class PrincipalTypeTests
{
    #region ParsePrincipalType

    [Theory]
    [InlineData("user", PrincipalType.User)]
    [InlineData("USER", PrincipalType.User)]
    [InlineData("User", PrincipalType.User)]
    [InlineData("group", PrincipalType.Group)]
    [InlineData("GROUP", PrincipalType.Group)]
    [InlineData("Group", PrincipalType.Group)]
    [InlineData("service_account", PrincipalType.ServiceAccount)]
    [InlineData("SERVICE_ACCOUNT", PrincipalType.ServiceAccount)]
    [InlineData("serviceaccount", PrincipalType.ServiceAccount)]
    public void ParsePrincipalType_ValidValue_ReturnsCorrectType(string value, PrincipalType expected)
    {
        // Act
        var result = PrincipalTypeExtensions.ParsePrincipalType(value);

        // Assert
        Check.That(result).IsEqualTo(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    [InlineData("admin")]
    [InlineData("unknown")]
    public void ParsePrincipalType_InvalidValue_ThrowsArgumentException(string value)
    {
        // Act & Assert
        Check.ThatCode(() => PrincipalTypeExtensions.ParsePrincipalType(value))
            .Throws<ArgumentException>();
    }

    #endregion

    #region ToOpenFgaPrefix

    [Theory]
    [InlineData(PrincipalType.User, "user")]
    [InlineData(PrincipalType.Group, "group")]
    [InlineData(PrincipalType.ServiceAccount, "serviceaccount")]
    public void ToOpenFgaPrefix_ValidType_ReturnsLowercaseString(PrincipalType type, string expected)
    {
        // Act
        var result = type.ToOpenFgaPrefix();

        // Assert
        Check.That(result).IsEqualTo(expected);
    }

    #endregion

    #region Enum Values

    [Fact]
    public void PrincipalType_HasExpectedValues()
    {
        // Assert
        Check.That(Enum.GetValues<PrincipalType>())
            .Contains(PrincipalType.User, PrincipalType.Group, PrincipalType.ServiceAccount);
    }

    #endregion
}
