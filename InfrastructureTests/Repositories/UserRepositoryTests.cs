// InfrastructureTests/Repositories/UserRepositoryTests.cs
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Repositories;
using InfrastructureTests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InfrastructureTests.Repositories;

public class UserRepositoryTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly UserRepository _repository;

    public UserRepositoryTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new UserRepository(_fixture.Context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUser()
    {
        // Arrange
        var user = new User("test@mail.com", "hashedPassword", "JohnDoe");

        // Act
        await _repository.AddAsync(user);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var savedUser = await _fixture.Context.Users.FirstOrDefaultAsync(u => u.Email == "test@mail.com");
        savedUser.Should().NotBeNull();
        savedUser!.UserName.Should().Be("JohnDoe");
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = new User("test@mail.com", "hashedPassword", "JohnDoe");
        await _repository.AddAsync(user);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("test@mail.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@mail.com");
        result!.UserName.Should().Be("JohnDoe");
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@mail.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = new User("test@mail.com", "hashedPassword", "JohnDoe");
        await _repository.AddAsync(user);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@mail.com");
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenEmailExists_ShouldReturnTrue()
    {
        // Arrange
        var user = new User("test@mail.com", "hashedPassword", "JohnDoe");
        await _repository.AddAsync(user);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByEmailAsync("test@mail.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        // Arrange
        var user = new User("test@mail.com", "hashedPassword", "JohnDoe");
        await _repository.AddAsync(user);
        await _fixture.Context.SaveChangesAsync();

        // Act
        user.UpdateProfile("new@mail.com", "JaneDoe");
        await _repository.UpdateAsync(user);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var updated = await _fixture.Context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        updated!.Email.Should().Be("new@mail.com");
        updated!.UserName.Should().Be("JaneDoe");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteUser()
    {
        // Arrange
        var user = new User("test@mail.com", "hashedPassword", "JohnDoe");
        await _repository.AddAsync(user);
        await _fixture.Context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(user);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var deleted = await _fixture.Context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        deleted.Should().BeNull();
    }
}