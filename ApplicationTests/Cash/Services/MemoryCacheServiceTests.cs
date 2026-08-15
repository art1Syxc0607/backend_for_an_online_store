using Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using FluentAssertions;
using Xunit;

namespace ApplicationTests.Services;

public class MemoryCacheServiceTests
{
    private readonly MemoryCacheService _cacheService;
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheService = new MemoryCacheService(_memoryCache);
    }

    [Fact]
    public async Task SetAsync_ShouldStoreValueInCache()
    {
        // Arrange
        var key = "test:key";
        var value = new { Id = 1, Name = "Test" };

        // Act
        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));

        // Assert
        var cached = await _cacheService.GetAsync<object>(key);
        cached.Should().NotBeNull();
        cached.Should().BeEquivalentTo(value);
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ShouldReturnValue()
    {
        // Arrange
        var key = "test:key";
        var value = "Hello World";
        await _cacheService.SetAsync(key, value);

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().Be("Hello World");
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnDefault()
    {
        // Act
        var result = await _cacheService.GetAsync<string>("nonexistent:key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteKey()
    {
        // Arrange
        var key = "test:key";
        await _cacheService.SetAsync(key, "value");

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        var result = await _cacheService.GetAsync<string>(key);
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveByPrefix_ShouldDeleteAllKeysWithPrefix()
    {
        // Arrange
        await _cacheService.SetAsync("products:1", new { Id = 1 });
        await _cacheService.SetAsync("products:2", new { Id = 2 });
        await _cacheService.SetAsync("products:filter:test", new { Id = 3 });
        await _cacheService.SetAsync("categories:all", new { Id = 1 });

        // Act
        await _cacheService.RemoveByPrefix("products:");

        // Assert
        (await _cacheService.GetAsync<object>("products:1")).Should().BeNull();
        (await _cacheService.GetAsync<object>("products:2")).Should().BeNull();
        (await _cacheService.GetAsync<object>("products:filter:test")).Should().BeNull();
        (await _cacheService.GetAsync<object>("categories:all")).Should().NotBeNull();
    }

    [Fact]
    public async Task SetAsync_WithExpiration_ShouldExpireAfterTime()
    {
        // Arrange
        var key = "test:key";
        await _cacheService.SetAsync(key, "value", TimeSpan.FromMilliseconds(100));

        // Act
        var beforeExpiration = await _cacheService.GetAsync<string>(key);
        await Task.Delay(200);
        var afterExpiration = await _cacheService.GetAsync<string>(key);

        // Assert
        beforeExpiration.Should().Be("value");
        afterExpiration.Should().BeNull();
    }

    [Fact]
    public async Task RemoveByPrefix_WithMultipleKeys_ShouldRemoveAll()
    {
        // Arrange
        var keys = new[] { "products:1", "products:2", "products:3", "other:1" };
        foreach (var key in keys)
        {
            await _cacheService.SetAsync(key, "value");
        }

        // Act
        await _cacheService.RemoveByPrefix("products:");

        // Assert
        foreach (var key in keys)
        {
            if (key.StartsWith("products:"))
            {
                (await _cacheService.GetAsync<object>(key)).Should().BeNull();
            }
            else
            {
                (await _cacheService.GetAsync<object>(key)).Should().NotBeNull();
            }
        }
    }


    // Сводка теста: всего: 7; сбой: 0; успешно: 7; пропущено: 0; длительность: 1,5 с
}