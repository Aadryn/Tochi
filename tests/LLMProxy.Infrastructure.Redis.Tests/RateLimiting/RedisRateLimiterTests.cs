using FluentAssertions;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using LLMProxy.Infrastructure.Redis.RateLimiting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using StackExchange.Redis;
using Xunit;

namespace LLMProxy.Infrastructure.Redis.Tests.RateLimiting;

/// <summary>
/// Tests unitaires pour RedisRateLimiter.
/// </summary>
/// <remarks>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// </remarks>
public sealed class RedisRateLimiterTests : IDisposable
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;
    private readonly ILogger<RedisRateLimiter> _logger;
    private readonly IRateLimiter _sut; // System Under Test

    public RedisRateLimiterTests()
    {
        // Arrange : Mock Redis
        _connectionMultiplexer = Substitute.For<IConnectionMultiplexer>();
        _database = Substitute.For<IDatabase>();
        _connectionMultiplexer.GetDatabase(Arg.Any<int>()).Returns(_database);

        _logger = Substitute.For<ILogger<RedisRateLimiter>>();

        _sut = new RedisRateLimiter(_connectionMultiplexer, _logger);
    }

    public void Dispose()
    {
        // Cleanup si nécessaire
    }

    #region CheckSlidingWindowAsync Tests

    [Fact]
    public async Task CheckSlidingWindowAsync_WhenUnderLimit_ShouldAllowRequest()
    {
        // Arrange
        const string key = "test:ip:192.168.1.1";
        const int limit = 100;
        var window = TimeSpan.FromMinutes(1);

        // Mock Lua script result : allowed=1, current=50, max=100
        var mockResult = RedisResult.Create(new RedisResult[] {
            RedisResult.Create((RedisValue)1),
            RedisResult.Create((RedisValue)50),
            RedisResult.Create((RedisValue)100)
        });
        _database.ScriptEvaluateAsync(
            Arg.Any<string>(),
            Arg.Any<RedisKey[]>(),
            Arg.Any<RedisValue[]>())
            .Returns(Task.FromResult(mockResult));

        // Act
        var result = await _sut.CheckSlidingWindowAsync(key, limit, window);

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.CurrentCount.Should().Be(50);
        result.Limit.Should().Be(100);
        result.Window.Should().Be(window);
    }

    [Fact]
    public async Task CheckSlidingWindowAsync_WhenLimitExceeded_ShouldRejectRequest()
    {
        // Arrange
        const string key = "test:ip:192.168.1.2";
        const int limit = 100;
        var window = TimeSpan.FromMinutes(1);

        // Mock Lua script result : allowed=0, current=100, max=100
        var mockResult = RedisResult.Create(new RedisResult[] {
            RedisResult.Create((RedisValue)0),
            RedisResult.Create((RedisValue)100),
            RedisResult.Create((RedisValue)100)
        });
        _database.ScriptEvaluateAsync(
            Arg.Any<string>(),
            Arg.Any<RedisKey[]>(),
            Arg.Any<RedisValue[]>())
            .Returns(Task.FromResult(mockResult));

        // Act
        var result = await _sut.CheckSlidingWindowAsync(key, limit, window);

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.CurrentCount.Should().Be(100);
        result.Limit.Should().Be(100);
        result.RetryAfter.Should().BeCloseTo(window, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task CheckSlidingWindowAsync_WhenRedisThrowsException_ShouldAllowRequest()
    {
        // Arrange : Fail-open strategy
        const string key = "test:ip:192.168.1.3";
        const int limit = 100;
        var window = TimeSpan.FromMinutes(1);

        _database.ScriptEvaluateAsync(
            Arg.Any<string>(),
            Arg.Any<RedisKey[]>(),
            Arg.Any<RedisValue[]>())
            .Returns<RedisResult>(_ => throw new RedisException("Redis connection failed"));

        // Act
        var result = await _sut.CheckSlidingWindowAsync(key, limit, window);

        // Assert : Fail-open (autoriser en cas d'erreur Redis)
        result.IsAllowed.Should().BeTrue();
        result.CurrentCount.Should().Be(0);
        result.Limit.Should().Be(limit);

        // Vérifier que l'erreur a été loguée
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Redis")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion

    #region CheckTokenBucketAsync Tests

    [Fact]
    public async Task CheckTokenBucketAsync_WhenEnoughTokens_ShouldAllowRequest()
    {
        // Arrange
        const string key = "test:tenant:guid:endpoint";
        const int capacity = 1000;
        const int tokensPerInterval = 100;
        var interval = TimeSpan.FromMinutes(1);
        const int tokensRequired = 50;

        // Mock Lua script result : allowed=1, remaining=50, capacity=1000
        var mockResult = RedisResult.Create(new RedisResult[] {
            RedisResult.Create((RedisValue)1),
            RedisResult.Create((RedisValue)50),
            RedisResult.Create((RedisValue)1000)
        });
        _database.ScriptEvaluateAsync(
            Arg.Any<string>(),
            Arg.Any<RedisKey[]>(),
            Arg.Any<RedisValue[]>())
            .Returns(Task.FromResult(mockResult));

        // Act
        var result = await _sut.CheckTokenBucketAsync(
            key, capacity, tokensPerInterval, interval, tokensRequired);

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.RemainingTokens.Should().Be(50);
        result.Limit.Should().Be(capacity);
    }

    [Fact]
    public async Task CheckTokenBucketAsync_WhenInsufficientTokens_ShouldRejectRequest()
    {
        // Arrange
        const string key = "test:tenant:guid:endpoint";
        const int capacity = 1000;
        const int tokensPerInterval = 100;
        var interval = TimeSpan.FromMinutes(1);
        const int tokensRequired = 150;

        // Mock Lua script result : allowed=0, remaining=100, capacity=1000
        var mockResult = RedisResult.Create(new RedisResult[] {
            RedisResult.Create((RedisValue)0),
            RedisResult.Create((RedisValue)100),
            RedisResult.Create((RedisValue)1000)
        });
        _database.ScriptEvaluateAsync(
            Arg.Any<string>(),
            Arg.Any<RedisKey[]>(),
            Arg.Any<RedisValue[]>())
            .Returns(Task.FromResult(mockResult));

        // Act
        var result = await _sut.CheckTokenBucketAsync(
            key, capacity, tokensPerInterval, interval, tokensRequired);

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.RemainingTokens.Should().Be(100);
        result.Limit.Should().Be(capacity);
        result.RetryAfter.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task CheckTokenBucketAsync_WhenRedisThrowsException_ShouldAllowRequest()
    {
        // Arrange : Fail-open strategy
        const string key = "test:tenant:guid:endpoint";
        const int capacity = 1000;
        const int tokensPerInterval = 100;
        var interval = TimeSpan.FromMinutes(1);
        const int tokensRequired = 50;

        _database.ScriptEvaluateAsync(
            Arg.Any<string>(),
            Arg.Any<RedisKey[]>(),
            Arg.Any<RedisValue[]>())
            .Returns<RedisResult>(_ => throw new RedisTimeoutException("Redis timeout", CommandStatus.Unknown));

        // Act
        var result = await _sut.CheckTokenBucketAsync(
            key, capacity, tokensPerInterval, interval, tokensRequired);

        // Assert : Fail-open
        result.IsAllowed.Should().BeTrue();
        result.RemainingTokens.Should().Be(0);
        result.Limit.Should().Be(capacity);

        // Vérifier que l'erreur a été loguée
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Redis")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion

    #region IncrementAsync Tests

    [Fact]
    public async Task IncrementAsync_WhenCalled_ShouldReturnIncrementedValue()
    {
        // Arrange
        const string key = "test:usage:tokens";
        const long increment = 1500;
        const long expectedTotal = 10500;

        _database.StringIncrementAsync(Arg.Any<RedisKey>(), increment, Arg.Any<CommandFlags>())
            .Returns(expectedTotal);

        // Act
        var result = await _sut.IncrementAsync(key, increment);

        // Assert
        result.Should().Be(expectedTotal);
        await _database.Received(1).StringIncrementAsync(key, increment, CommandFlags.None);
    }

    [Fact]
    public async Task IncrementAsync_WhenZeroIncrement_ShouldReturnCurrentValue()
    {
        // Arrange : Increment de 0 pour lire la valeur actuelle
        const string key = "test:usage:tokens";
        const long currentValue = 5000;

        _database.StringIncrementAsync(Arg.Any<RedisKey>(), 0, Arg.Any<CommandFlags>())
            .Returns(currentValue);

        // Act
        var result = await _sut.IncrementAsync(key, 0);

        // Assert
        result.Should().Be(currentValue);
    }

    [Fact]
    public async Task IncrementAsync_WhenRedisThrowsException_ShouldReturnZero()
    {
        // Arrange : Fail-open strategy
        const string key = "test:usage:tokens";
        const long increment = 1000;

        _database.StringIncrementAsync(Arg.Any<RedisKey>(), increment, Arg.Any<CommandFlags>())
            .Returns<long>(_ => throw new RedisException("Redis error"));

        // Act
        var result = await _sut.IncrementAsync(key, increment);

        // Assert : Fail-open (retourner 0 en cas d'erreur)
        result.Should().Be(0);

        // Vérifier que l'erreur a été loguée
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Redis")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion
}
