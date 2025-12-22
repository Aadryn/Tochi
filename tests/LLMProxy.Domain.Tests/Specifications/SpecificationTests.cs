using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Specifications;
using Xunit;

namespace LLMProxy.Domain.Tests.Specifications;

/// <summary>
/// Tests pour ISpecification et CompositeSpecification.
/// Conforme à ADR-028 (Specification Pattern) et ADR-999 (TDD).
/// </summary>
public sealed class SpecificationTests
{
    #region TenantIsEligibleSpecification Tests

    [Fact]
    public void TenantIsEligible_ActiveAndNotDeactivated_ShouldReturnTrue()
    {
        // Arrange
        var tenant = Tenant.Create("Active Tenant", "active-tenant").Value;
        var spec = new TenantIsEligibleSpecification();

        // Act
        var result = spec.IsSatisfiedBy(tenant);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TenantIsEligible_InactiveTenant_ShouldReturnFalse()
    {
        // Arrange
        var tenant = Tenant.Create("Inactive Tenant", "inactive").Value;
        tenant.Deactivate();
        var spec = new TenantIsEligibleSpecification();

        // Act
        var result = spec.IsSatisfiedBy(tenant);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TenantIsEligible_NullTenant_ShouldReturnFalse()
    {
        // Arrange
        Tenant? tenant = null;
        var spec = new TenantIsEligibleSpecification();

        // Act
        var result = spec.IsSatisfiedBy(tenant!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TenantIsEligible_ToExpression_ShouldGenerateCorrectLinqExpression()
    {
        // Arrange
        var activeTenant = Tenant.Create("Active", "active").Value;
        var inactiveTenant = Tenant.Create("Inactive", "inactive").Value;
        inactiveTenant.Deactivate();
        
        var tenants = new List<Tenant> { activeTenant, inactiveTenant };
        var spec = new TenantIsEligibleSpecification();

        // Act
        var expression = spec.ToExpression().Compile();
        var eligibleTenants = tenants.Where(expression).ToList();

        // Assert
        Assert.Single(eligibleTenants);
        Assert.Equal("Active", eligibleTenants.First().Name);
    }

    #endregion

    #region QuotaIsAvailableSpecification Tests

    [Fact]
    public void QuotaIsAvailable_SufficientQuota_ShouldReturnTrue()
    {
        // Arrange
        var quota = new QuotaUsage
        {
            UserId = Guid.NewGuid(),
            QuotaType = QuotaType.TokensPerMonth,
            CurrentUsage = 100,
            Limit = 1000,
            WindowStart = DateTime.UtcNow.AddDays(-15),
            WindowEnd = DateTime.UtcNow.AddDays(15)
        };
        var spec = new QuotaIsAvailableSpecification(requestedAmount: 500);

        // Act
        var result = spec.IsSatisfiedBy(quota);

        // Assert
        Assert.True(result); // 100 + 500 = 600 <= 1000
    }

    [Fact]
    public void QuotaIsAvailable_InsufficientQuota_ShouldReturnFalse()
    {
        // Arrange
        var quota = new QuotaUsage
        {
            UserId = Guid.NewGuid(),
            QuotaType = QuotaType.TokensPerMonth,
            CurrentUsage = 900,
            Limit = 1000,
            WindowStart = DateTime.UtcNow.AddDays(-15),
            WindowEnd = DateTime.UtcNow.AddDays(15)
        };
        var spec = new QuotaIsAvailableSpecification(requestedAmount: 200);

        // Act
        var result = spec.IsSatisfiedBy(quota);

        // Assert
        Assert.False(result); // 900 + 200 = 1100 > 1000
    }

    [Fact]
    public void QuotaIsAvailable_ExactlyAtLimit_ShouldReturnTrue()
    {
        // Arrange
        var quota = new QuotaUsage
        {
            UserId = Guid.NewGuid(),
            QuotaType = QuotaType.RequestsPerDay,
            CurrentUsage = 500,
            Limit = 1000,
            WindowStart = DateTime.UtcNow,
            WindowEnd = DateTime.UtcNow.AddDays(1)
        };
        var spec = new QuotaIsAvailableSpecification(requestedAmount: 500);

        // Act
        var result = spec.IsSatisfiedBy(quota);

        // Assert
        Assert.True(result); // 500 + 500 = 1000 <= 1000
    }

    [Fact]
    public void QuotaIsAvailable_NullQuota_ShouldReturnFalse()
    {
        // Arrange
        QuotaUsage? quota = null;
        var spec = new QuotaIsAvailableSpecification(requestedAmount: 100);

        // Act
        var result = spec.IsSatisfiedBy(quota!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void QuotaIsAvailable_Constructor_WithZeroOrNegative_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new QuotaIsAvailableSpecification(requestedAmount: 0));
        Assert.Throws<ArgumentException>(() => new QuotaIsAvailableSpecification(requestedAmount: -100));
    }

    [Fact]
    public void QuotaIsAvailable_ToExpression_ShouldGenerateCorrectLinqExpression()
    {
        // Arrange
        var quota1 = new QuotaUsage { CurrentUsage = 100, Limit = 1000 };
        var quota2 = new QuotaUsage { CurrentUsage = 950, Limit = 1000 };
        var quotas = new List<QuotaUsage> { quota1, quota2 };
        
        var spec = new QuotaIsAvailableSpecification(requestedAmount: 100);

        // Act
        var expression = spec.ToExpression().Compile();
        var availableQuotas = quotas.Where(expression).ToList();

        // Assert
        Assert.Single(availableQuotas);
        Assert.Equal(100, availableQuotas.First().CurrentUsage);
    }

    #endregion

    #region Logical Operators Tests

    [Fact]
    public void And_BothSpecificationsSatisfied_ShouldReturnTrue()
    {
        // Arrange
        var tenant = Tenant.Create("Active", "active").Value;
        var spec1 = new TenantIsEligibleSpecification();
        var spec2 = new TenantIsEligibleSpecification(); // Même spec pour test
        var combinedSpec = spec1.And(spec2);

        // Act
        var result = combinedSpec.IsSatisfiedBy(tenant);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void And_OneSpecificationNotSatisfied_ShouldReturnFalse()
    {
        // Arrange
        var tenant = Tenant.Create("Active", "active").Value;
        tenant.Deactivate();
        
        var spec1 = new TenantIsEligibleSpecification();
        var spec2 = new TenantIsEligibleSpecification();
        var combinedSpec = spec1.And(spec2);

        // Act
        var result = combinedSpec.IsSatisfiedBy(tenant);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Or_OneSpecificationSatisfied_ShouldReturnTrue()
    {
        // Arrange
        var tenant = Tenant.Create("Active", "active").Value;
        var spec1 = new TenantIsEligibleSpecification();
        var spec2 = new TenantIsEligibleSpecification();
        var combinedSpec = spec1.Or(spec2);

        // Act
        var result = combinedSpec.IsSatisfiedBy(tenant);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Or_BothSpecificationsNotSatisfied_ShouldReturnFalse()
    {
        // Arrange
        var tenant = Tenant.Create("Inactive", "inactive").Value;
        tenant.Deactivate();
        
        var spec1 = new TenantIsEligibleSpecification();
        var spec2 = new TenantIsEligibleSpecification();
        var combinedSpec = spec1.Or(spec2);

        // Act
        var result = combinedSpec.IsSatisfiedBy(tenant);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Not_SpecificationSatisfied_ShouldReturnFalse()
    {
        // Arrange
        var tenant = Tenant.Create("Active", "active").Value;
        var spec = new TenantIsEligibleSpecification();
        var notSpec = spec.Not();

        // Act
        var result = notSpec.IsSatisfiedBy(tenant);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Not_SpecificationNotSatisfied_ShouldReturnTrue()
    {
        // Arrange
        var tenant = Tenant.Create("Inactive", "inactive").Value;
        tenant.Deactivate();
        
        var spec = new TenantIsEligibleSpecification();
        var notSpec = spec.Not();

        // Act
        var result = notSpec.IsSatisfiedBy(tenant);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ComplexComposition_AndOrNot_ShouldWorkCorrectly()
    {
        // Arrange
        var activeTenant = Tenant.Create("Active", "active").Value;
        var inactiveTenant = Tenant.Create("Inactive", "inactive").Value;
        inactiveTenant.Deactivate();

        var spec1 = new TenantIsEligibleSpecification();
        var spec2 = new TenantIsEligibleSpecification();
        var spec3 = new TenantIsEligibleSpecification();
        
        // (spec1 AND spec2) OR (NOT spec3)
        var andSpec = spec1.And(spec2);
        var notSpec = spec3.Not();
        var complexSpec = ((CompositeSpecification<Tenant>)andSpec).Or(notSpec);

        // Act
        var resultActive = complexSpec.IsSatisfiedBy(activeTenant);
        var resultInactive = complexSpec.IsSatisfiedBy(inactiveTenant);

        // Assert
        Assert.True(resultActive); // spec1 AND spec2 = true
        Assert.True(resultInactive); // NOT spec3 = true
    }

    #endregion
}
