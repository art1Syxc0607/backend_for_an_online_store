// InfrastructureTests/Repositories/CategoryRepositoryTests.cs
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Repositories;
using InfrastructureTests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InfrastructureTests.Repositories;

public class CategoryRepositoryTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new CategoryRepository(_fixture.Context);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddCategory()
    {
        // Arrange
        var category = new Category("Electronics", "Electronic devices");

        // Act
        await _repository.CreateAsync(category);
        await _fixture.Context.SaveChangesAsync();
        var result = await _repository.GetByIdAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Electronics");

        var saved = await _fixture.Context.Categories
            .FirstOrDefaultAsync(c => c.Id == result.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryExists_ShouldReturnCategory()
    {
        // Arrange
        var category = new Category("Electronics", "Electronic devices");
        await _repository.CreateAsync(category);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
        result!.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var category1 = new Category("Electronics", "Electronic devices");
        var category2 = new Category("Books", "Books and literature");
        var category3 = new Category("Clothing", "Fashion items");

        _fixture.Cleanup();
        await _repository.CreateAsync(category1);
        await _repository.CreateAsync(category2);
        await _repository.CreateAsync(category3);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllCategoriesAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(c => c.Name == "Electronics");
        result.Should().Contain(c => c.Name == "Books");
        result.Should().Contain(c => c.Name == "Clothing");
        //result.Should().BeInAscendingOrder(c => c.Name); // ?, may biz logic biz
    }

    [Fact]
    public async Task GetAllAsync_WhenNoCategories_ShouldReturnEmptyList()
    {
        // Clear first?
        // Act
        _fixture.Cleanup();
        var result = await _repository.GetAllCategoriesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategory()
    {
        // Arrange
        var category = new Category("Electronics", "Old description");
        await _repository.CreateAsync(category);
        await _fixture.Context.SaveChangesAsync();

        // Act
        category.Update("Gadgets", "New description");
        await _repository.UpdateAsync(category);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var updated = await _fixture.Context.Categories
            .FirstOrDefaultAsync(c => c.Id == category.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Gadgets");
        updated!.Description.Should().Be("New description");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCategory()
    {
        // Arrange
        var category = new Category("Electronics", "Electronic devices");
        await _repository.CreateAsync(category);
        await _fixture.Context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(category);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var deleted = await _fixture.Context.Categories
            .FirstOrDefaultAsync(c => c.Id == category.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WhenCategoryExists_ShouldReturnTrue()
    {
        // Arrange
        var category = new Category("Electronics", "Electronic devices");
        await _repository.CreateAsync(category);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistByIdAsync(category.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenCategoryDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistByIdAsync(999);

        // Assert
        result.Should().BeFalse();
    }


    [Fact]
    public void CreateCategory_WithEmptyName_ShouldThrowDomainException()
    {
        // Act
        Action act = () => new Category("", "Empty name");

        // Assert
        act.Should().Throw<Domain.Exceptions.DomainException>()
            .WithMessage("*Category name cannot be empty*");
    }

    [Fact]
    public async Task GetByIdsAsync_WhenCategoriesExist_ShouldReturnAll()
    {
        // Arrange
        var category1 = new Category("Electronics", "Electronic devices");
        var category2 = new Category("Books", "Books and literature");
        var category3 = new Category("Clothing", "Fashion items");

        await _repository.CreateAsync(category1);
        await _repository.CreateAsync(category2);
        await _repository.CreateAsync(category3);
        await _fixture.Context.SaveChangesAsync();

        var ids = new List<int> { category1.Id, category3.Id };

        // Act
        var result = await _repository.GetByIdsAsync(ids);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Electronics");
        result.Should().Contain(c => c.Name == "Clothing");
        result.Should().NotContain(c => c.Name == "Books");
    }
}