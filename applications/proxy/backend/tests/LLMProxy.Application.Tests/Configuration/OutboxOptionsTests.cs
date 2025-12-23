using FluentAssertions;
using LLMProxy.Application.Configuration;

namespace LLMProxy.Application.Tests.Configuration;

/// <summary>
/// Tests unitaires pour <see cref="OutboxOptions"/>.
/// </summary>
public sealed class OutboxOptionsTests
{
    [Fact]
    public void Constructor_DoitInitialiserAvecValeursParDefaut()
    {
        // Act
        var options = new OutboxOptions();

        // Assert
        options.PollingInterval.Should().Be(TimeSpan.FromSeconds(5));
        options.BatchSize.Should().Be(100);
        options.MaxRetries.Should().Be(3);
        options.EnableCleanup.Should().BeTrue();
        options.RetentionPeriod.Should().Be(TimeSpan.FromDays(7));
        options.CleanupInterval.Should().Be(TimeSpan.FromHours(1));
        options.EnableDeadLetter.Should().BeTrue();
        options.DeadLetterCheckInterval.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void Validate_AvecValeursParDefaut_NeLevePasException()
    {
        // Arrange
        var options = new OutboxOptions();

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_QuandPollingIntervalNegatif_DoitLeverArgumentException()
    {
        // Arrange
        var options = new OutboxOptions
        {
            PollingInterval = TimeSpan.FromSeconds(-1)
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*PollingInterval*");
    }

    [Fact]
    public void Validate_QuandPollingIntervalZero_DoitLeverArgumentException()
    {
        // Arrange
        var options = new OutboxOptions
        {
            PollingInterval = TimeSpan.Zero
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*PollingInterval*");
    }

    [Fact]
    public void Validate_QuandBatchSizeZero_DoitLeverArgumentException()
    {
        // Arrange
        var options = new OutboxOptions
        {
            BatchSize = 0
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*BatchSize*");
    }

    [Fact]
    public void Validate_QuandBatchSizeNegatif_DoitLeverArgumentException()
    {
        // Arrange
        var options = new OutboxOptions
        {
            BatchSize = -10
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*BatchSize*");
    }

    [Fact]
    public void Validate_QuandMaxRetriesZero_DoitLeverArgumentException()
    {
        // Arrange
        var options = new OutboxOptions
        {
            MaxRetries = 0
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*MaxRetries*");
    }

    [Fact]
    public void Validate_QuandMaxRetriesNegatif_DoitLeverArgumentException()
    {
        // Arrange
        var options = new OutboxOptions
        {
            MaxRetries = -5
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*MaxRetries*");
    }

    [Fact]
    public void Validate_QuandRetentionPeriodNegatif_DoitLeverArgumentException()
    {
        // Arrange
        var options = new OutboxOptions
        {
            RetentionPeriod = TimeSpan.FromDays(-1)
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*RetentionPeriod*");
    }

    [Fact]
    public void Validate_QuandRetentionPeriodZero_DoitLeverArgumentException()
    {
        // Arrange
        var options = new OutboxOptions
        {
            RetentionPeriod = TimeSpan.Zero
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*RetentionPeriod*");
    }

    [Fact]
    public void Validate_QuandCleanupIntervalNegatif_DoitLeverArgumentException()
    {
        // Arrange
        var options = new OutboxOptions
        {
            CleanupInterval = TimeSpan.FromMinutes(-5)
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*CleanupInterval*");
    }

    [Fact]
    public void Validate_QuandCleanupIntervalZero_DoitLeverArgumentException()
    {
        // Arrange
        var options = new OutboxOptions
        {
            CleanupInterval = TimeSpan.Zero
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*CleanupInterval*");
    }

    [Fact]
    public void Validate_QuandDeadLetterCheckIntervalNegatif_DoitLeverArgumentException()
    {
        // Arrange
        var options = new OutboxOptions
        {
            DeadLetterCheckInterval = TimeSpan.FromMinutes(-1)
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*DeadLetterCheckInterval*");
    }

    [Fact]
    public void Validate_QuandDeadLetterCheckIntervalZero_DoitLeverArgumentException()
    {
        // Arrange
        var options = new OutboxOptions
        {
            DeadLetterCheckInterval = TimeSpan.Zero
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*DeadLetterCheckInterval*");
    }

    [Fact]
    public void Validate_AvecValeursPersonnaliseesValides_NeLevePasException()
    {
        // Arrange
        var options = new OutboxOptions
        {
            PollingInterval = TimeSpan.FromSeconds(10),
            BatchSize = 50,
            MaxRetries = 5,
            EnableCleanup = false,
            RetentionPeriod = TimeSpan.FromDays(14),
            CleanupInterval = TimeSpan.FromHours(2),
            EnableDeadLetter = false,
            DeadLetterCheckInterval = TimeSpan.FromMinutes(10)
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }
}
