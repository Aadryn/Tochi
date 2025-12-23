using LLMProxy.Infrastructure.Authorization.Abstractions;

namespace LLMProxy.Infrastructure.Authorization.Tests;

/// <summary>
/// Tests unitaires pour <see cref="AuthorizationResult"/>.
/// </summary>
public class AuthorizationResultTests
{
    [Fact]
    public void Allowed_IsAllowed_ReturnsTrue()
    {
        // Arrange
        var result = AuthorizationResult.Allowed;

        // Assert
        Check.That(result.IsAllowed).IsTrue();
        Check.That(result.IsDenied).IsFalse();
        Check.That(result.Reason).IsNull();
    }

    [Fact]
    public void Denied_WithoutReason_ReturnsDeniedWithDefaultMessage()
    {
        // Act
        var result = AuthorizationResult.Denied();

        // Assert
        Check.That(result.IsAllowed).IsFalse();
        Check.That(result.IsDenied).IsTrue();
        Check.That(result.Reason).IsEqualTo("Accès refusé");
    }

    [Fact]
    public void Denied_WithReason_ReturnsDeniedWithSpecifiedReason()
    {
        // Arrange
        var reason = "Utilisateur non autorisé";

        // Act
        var result = AuthorizationResult.Denied(reason);

        // Assert
        Check.That(result.IsAllowed).IsFalse();
        Check.That(result.Reason).IsEqualTo(reason);
    }

    [Fact]
    public void NoRelation_ReturnsDetailedReason()
    {
        // Act
        var result = AuthorizationResult.NoRelation("can_view", "tenant", "tenant-123");

        // Assert
        Check.That(result.IsAllowed).IsFalse();
        Check.That(result.Reason).Contains("can_view");
        Check.That(result.Reason).Contains("tenant:tenant-123");
    }

    [Fact]
    public void Error_ReturnsErrorMessage()
    {
        // Arrange
        var errorMessage = "Connection timeout";

        // Act
        var result = AuthorizationResult.Error(errorMessage);

        // Assert
        Check.That(result.IsAllowed).IsFalse();
        Check.That(result.Reason).Contains("Erreur d'autorisation");
        Check.That(result.Reason).Contains(errorMessage);
    }

    [Fact]
    public void ImplicitBoolConversion_AllowedReturnsTrue()
    {
        // Arrange
        AuthorizationResult result = AuthorizationResult.Allowed;

        // Act
        bool isAllowed = result;

        // Assert
        Check.That(isAllowed).IsTrue();
    }

    [Fact]
    public void ImplicitBoolConversion_DeniedReturnsFalse()
    {
        // Arrange
        AuthorizationResult result = AuthorizationResult.Denied();

        // Act
        bool isAllowed = result;

        // Assert
        Check.That(isAllowed).IsFalse();
    }

    [Fact]
    public void ToString_Allowed_ReturnsAutorise()
    {
        // Arrange
        var result = AuthorizationResult.Allowed;

        // Act
        var toString = result.ToString();

        // Assert
        Check.That(toString).IsEqualTo("Autorisé");
    }

    [Fact]
    public void ToString_Denied_ReturnsRefuseWithReason()
    {
        // Arrange
        var result = AuthorizationResult.Denied("Permission manquante");

        // Act
        var toString = result.ToString();

        // Assert
        Check.That(toString).StartsWith("Refusé:");
        Check.That(toString).Contains("Permission manquante");
    }
}
