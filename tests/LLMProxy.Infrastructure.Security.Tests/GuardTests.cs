using LLMProxy.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using NFluent;
using Xunit;

namespace LLMProxy.Infrastructure.Security.Tests;

/// <summary>
/// Tests unitaires de la classe Guard.
/// </summary>
public class GuardTests
{
    #region AgainstNull Tests

    [Fact]
    public void AgainstNull_WhenValueIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        object? nullValue = null;

        // Act & Assert
        var exception = Check.ThatCode(() => Guard.AgainstNull(nullValue, "testParam"))
            .Throws<ArgumentNullException>()
            .Value;

        Check.That(exception.ParamName).IsEqualTo("testParam");
        Check.That(exception.Message).Contains("testParam");
    }

    [Fact]
    public void AgainstNull_WhenValueIsNotNull_DoesNotThrow()
    {
        // Arrange
        var validValue = new object();

        // Act & Assert
        Check.ThatCode(() => Guard.AgainstNull(validValue, "testParam"))
            .DoesNotThrow();
    }

    [Fact]
    public void AgainstNull_WithCustomMessage_ThrowsWithCustomMessage()
    {
        // Arrange
        object? nullValue = null;
        const string customMessage = "Custom error message";

        // Act & Assert
        var exception = Check.ThatCode(() => Guard.AgainstNull(nullValue, "testParam", customMessage))
            .Throws<ArgumentNullException>()
            .Value;

        Check.That(exception.Message).Contains(customMessage);
    }

    #endregion

    #region AgainstNullOrWhiteSpace Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void AgainstNullOrWhiteSpace_WhenValueIsNullOrWhiteSpace_ThrowsArgumentException(string? invalidValue)
    {
        // Act & Assert
        var exception = Check.ThatCode(() => Guard.AgainstNullOrWhiteSpace(invalidValue, "testParam"))
            .Throws<ArgumentException>()
            .Value;

        Check.That(exception.ParamName).IsEqualTo("testParam");
        Check.That(exception.Message).Contains("testParam");
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("a")]
    [InlineData(" valid ")]
    [InlineData("multiple words")]
    public void AgainstNullOrWhiteSpace_WhenValueIsValid_DoesNotThrow(string validValue)
    {
        // Act & Assert
        Check.ThatCode(() => Guard.AgainstNullOrWhiteSpace(validValue, "testParam"))
            .DoesNotThrow();
    }

    #endregion

    #region AgainstEmptyGuid Tests

    [Fact]
    public void AgainstEmptyGuid_WhenValueIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act & Assert
        var exception = Check.ThatCode(() => Guard.AgainstEmptyGuid(emptyGuid, "testParam"))
            .Throws<ArgumentException>()
            .Value;

        Check.That(exception.ParamName).IsEqualTo("testParam");
        Check.That(exception.Message).Contains("testParam");
        Check.That(exception.Message).Contains("GUID vide");
    }

    [Fact]
    public void AgainstEmptyGuid_WhenValueIsValid_DoesNotThrow()
    {
        // Arrange
        var validGuid = Guid.NewGuid();

        // Act & Assert
        Check.ThatCode(() => Guard.AgainstEmptyGuid(validGuid, "testParam"))
            .DoesNotThrow();
    }

    #endregion

    #region AgainstResponseStarted Tests

    [Fact]
    public void AgainstResponseStarted_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        HttpResponse? nullResponse = null;

        // Act & Assert
        Check.ThatCode(() => Guard.AgainstResponseStarted(nullResponse!))
            .Throws<ArgumentNullException>();
    }

    [Fact]
    public void AgainstResponseStarted_WhenResponseNotStarted_DoesNotThrow()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        // Act & Assert
        Check.ThatCode(() => Guard.AgainstResponseStarted(httpContext.Response))
            .DoesNotThrow();
    }

    #endregion

    #region AgainstEmptyCollection Tests

    [Fact]
    public void AgainstEmptyCollection_WhenCollectionIsNull_ThrowsArgumentException()
    {
        // Arrange
        IEnumerable<int>? nullCollection = null;

        // Act & Assert
        var exception = Check.ThatCode(() => Guard.AgainstEmptyCollection(nullCollection, "testParam"))
            .Throws<ArgumentException>()
            .Value;

        Check.That(exception.ParamName).IsEqualTo("testParam");
    }

    [Fact]
    public void AgainstEmptyCollection_WhenCollectionIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var emptyCollection = Enumerable.Empty<int>();

        // Act & Assert
        Check.ThatCode(() => Guard.AgainstEmptyCollection(emptyCollection, "testParam"))
            .Throws<ArgumentException>();
    }

    [Fact]
    public void AgainstEmptyCollection_WhenCollectionHasElements_DoesNotThrow()
    {
        // Arrange
        var validCollection = new[] { 1, 2, 3 };

        // Act & Assert
        Check.ThatCode(() => Guard.AgainstEmptyCollection(validCollection, "testParam"))
            .DoesNotThrow();
    }

    #endregion

    #region AgainstNegativeOrZero Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void AgainstNegativeOrZero_WhenValueIsNegativeOrZero_ThrowsArgumentException(int invalidValue)
    {
        // Act & Assert
        var exception = Check.ThatCode(() => Guard.AgainstNegativeOrZero(invalidValue, "testParam"))
            .Throws<ArgumentException>()
            .Value;

        Check.That(exception.ParamName).IsEqualTo("testParam");
        Check.That(exception.Message).Contains("strictement positif");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void AgainstNegativeOrZero_WhenValueIsPositive_DoesNotThrow(int validValue)
    {
        // Act & Assert
        Check.ThatCode(() => Guard.AgainstNegativeOrZero(validValue, "testParam"))
            .DoesNotThrow();
    }

    #endregion

    #region AgainstOutOfRange Tests

    [Theory]
    [InlineData(0, 1, 10)]
    [InlineData(11, 1, 10)]
    [InlineData(-5, 1, 10)]
    [InlineData(100, 1, 10)]
    public void AgainstOutOfRange_WhenValueIsOutOfRange_ThrowsArgumentOutOfRangeException(int value, int min, int max)
    {
        // Act & Assert
        var exception = Check.ThatCode(() => Guard.AgainstOutOfRange(value, min, max, "testParam"))
            .Throws<ArgumentOutOfRangeException>()
            .Value;

        Check.That(exception.ParamName).IsEqualTo("testParam");
        Check.That(exception.Message).Contains("compris entre");
    }

    [Theory]
    [InlineData(1, 1, 10)]
    [InlineData(5, 1, 10)]
    [InlineData(10, 1, 10)]
    public void AgainstOutOfRange_WhenValueIsInRange_DoesNotThrow(int value, int min, int max)
    {
        // Act & Assert
        Check.ThatCode(() => Guard.AgainstOutOfRange(value, min, max, "testParam"))
            .DoesNotThrow();
    }

    [Fact]
    public void AgainstOutOfRange_WithStrings_WorksCorrectly()
    {
        // Arrange
        const string value = "m";
        const string min = "a";
        const string max = "z";

        // Act & Assert - valeur dans la plage
        Check.ThatCode(() => Guard.AgainstOutOfRange(value, min, max, "testParam"))
            .DoesNotThrow();

        // Act & Assert - valeur hors plage (inférieure - ordre lexicographique)
        Check.ThatCode(() => Guard.AgainstOutOfRange("Z", min, max, "testParam"))
            .Throws<ArgumentOutOfRangeException>();
    }

    #endregion
}
