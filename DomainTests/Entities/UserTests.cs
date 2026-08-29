// DomainTests/Entities/UserTests.cs
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace DomainTests.Entities;

public class UserTests
{
    // ========== 1. Конструктор и создание пользователя ==========

    [Fact]
    public void Constructor_WhenValidData_ShouldCreateUser()
    {
        // Arrange
        var email = "test@mail.com";
        var passwordHash = "hashedPassword123";
        var userName = "JohnDoe";

        // Act
        var user = new User(email, passwordHash, userName);

        // Assert
        user.Id.Should().Be(0);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.UserName.Should().Be(userName);
        user.Role.Should().Be(UserRole.User);
        user.IsEmailConfirmed.Should().BeFalse();
        user.EmailConfirmedAt.Should().BeNull();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WhenEmailIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var passwordHash = "hashedPassword123";
        var userName = "JohnDoe";

        // Act
        Action act = () => new User("", passwordHash, userName);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Email cannot be empty*");
    }

    [Fact]
    public void Constructor_WhenEmailIsInvalid_ShouldThrowDomainException()
    {
        // Arrange
        var invalidEmail = "not-an-email";
        var passwordHash = "hashedPassword123";
        var userName = "JohnDoe";

        // Act
        Action act = () => new User(invalidEmail, passwordHash, userName);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Invalid email format*");
    }

    [Fact]
    public void Constructor_WhenUserNameIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var email = "test@mail.com";
        var passwordHash = "hashedPassword123";

        // Act
        Action act = () => new User(email, passwordHash, "");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Full name cannot be empty*");
    }

    [Fact]
    public void Constructor_WhenPasswordHashIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var email = "test@mail.com";
        var userName = "JohnDoe";

        // Act
        Action act = () => new User(email, "", userName);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Password hash cannot be empty*");
    }

    // ========== 2. Обновление профиля ==========

    [Fact]
    public void UpdateProfile_WhenValidData_ShouldUpdateUser()
    {
        // Arrange
        var user = new User("old@mail.com", "hash", "OldName");
        var newEmail = "new@mail.com";
        var newName = "NewName";

        // Act
        user.UpdateProfile(newEmail, newName);

        // Assert
        user.Email.Should().Be(newEmail);
        user.UserName.Should().Be(newName);
    }

    [Fact]
    public void UpdateProfile_WhenOnlyEmailProvided_ShouldUpdateEmailOnly()
    {
        // Arrange
        var user = new User("old@mail.com", "hash", "John");
        var newEmail = "new@mail.com";

        // Act
        user.UpdateProfile(email: newEmail);

        // Assert
        user.Email.Should().Be(newEmail);
        user.UserName.Should().Be("John"); // Не изменилось
    }

    [Fact]
    public void UpdateProfile_WhenOnlyNameProvided_ShouldUpdateNameOnly()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        var newName = "Jane";

        // Act
        user.UpdateProfile(name: newName);

        // Assert
        user.Email.Should().Be("test@mail.com"); // Не изменилось
        user.UserName.Should().Be(newName);
    }

    [Fact]
    public void UpdateProfile_WhenEmailIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");

        // Act
        Action act = () => user.UpdateProfile(email: "");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Email cannot be empty*");
    }

    [Fact]
    public void UpdateProfile_WhenNameIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");

        // Act
        Action act = () => user.UpdateProfile(name: "");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Full name cannot be empty*");
    }

    // ========== 3. Смена пароля ==========

    [Fact]
    public void UpdatePassword_WhenValid_ShouldUpdatePasswordHash()
    {
        // Arrange
        var user = new User("test@mail.com", "oldHash", "John");
        var newHash = "newHash123";

        // Act
        user.UpdatePassword(newHash);

        // Assert
        user.PasswordHash.Should().Be(newHash);
    }

    [Fact]
    public void UpdatePassword_WhenHashIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("test@mail.com", "oldHash", "John");

        // Act
        Action act = () => user.UpdatePassword("");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Password hash cannot be empty*");
    }

    // ========== 4. Роли (Admin) ==========

    [Fact]
    public void PromoteToAdmin_WhenUserIsCustomer_ShouldPromote()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");

        // Act
        user.PromoteToAdmin();

        // Assert
        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void PromoteToAdmin_WhenUserIsAlreadyAdmin_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        user.PromoteToAdmin();

        // Act
        Action act = () => user.PromoteToAdmin();

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*User is already an admin*");
    }

    [Fact]
    public void DemoteFromAdmin_WhenUserIsAdmin_ShouldDemote()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        user.PromoteToAdmin();

        // Act
        user.DemoteFromAdmin();

        // Assert
        user.Role.Should().Be(UserRole.User);
    }

    [Fact]
    public void DemoteFromAdmin_WhenUserIsNotAdmin_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");

        // Act
        Action act = () => user.DemoteFromAdmin();

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*not an admin*");
    }

    // ========== 5. Подтверждение email ==========

    [Fact]
    public void GenerateEmailConfirmationToken_ShouldSetTokenAndExpiry()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        var token = "abc123token";
        var expiry = DateTime.UtcNow.AddHours(24);

        // Act
        user.GenerateEmailConfirmationToken(token, expiry);

        // Assert
        user.EmailConfirmationToken.Should().Be(token);
        user.EmailConfirmationTokenExpiry.Should().Be(expiry);
        user.IsEmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public void ConfirmEmail_WhenValidToken_ShouldConfirmEmail()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        var token = "abc123token";
        var expiry = DateTime.UtcNow.AddHours(24);
        user.GenerateEmailConfirmationToken(token, expiry);

        // Act
        user.ConfirmEmail(token);

        // Assert
        user.IsEmailConfirmed.Should().BeTrue();
        user.EmailConfirmedAt.Should().NotBeNull();
        user.EmailConfirmationToken.Should().BeNull();
        user.EmailConfirmationTokenExpiry.Should().BeNull();
    }

    [Fact]
    public void ConfirmEmail_WhenTokenIsInvalid_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        var token = "abc123token";
        var expiry = DateTime.UtcNow.AddHours(24);
        user.GenerateEmailConfirmationToken(token, expiry);

        // Act
        Action act = () => user.ConfirmEmail("wrong-token");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Invalid confirmation token*");
    }

    [Fact]
    public void ConfirmEmail_WhenTokenIsExpired_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        var token = "abc123token";
        var expiry = DateTime.UtcNow.AddHours(-1); // Истек
        user.GenerateEmailConfirmationToken(token, expiry);

        // Act
        Action act = () => user.ConfirmEmail(token);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*expired*");
    }

    [Fact]
    public void ConfirmEmail_WhenEmailAlreadyConfirmed_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        var token = "abc123token";
        user.GenerateEmailConfirmationToken(token, DateTime.UtcNow.AddHours(24));
        user.ConfirmEmail(token);

        // Act
        Action act = () => user.ConfirmEmail(token);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*already confirmed*");
    }

    // ========== 6. Метод EnsureEmailConfirmed ==========

    [Fact]
    public void EnsureEmailConfirmed_WhenEmailIsConfirmed_ShouldNotThrow()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        var token = "abc123token";
        user.GenerateEmailConfirmationToken(token, DateTime.UtcNow.AddHours(24));
        user.ConfirmEmail(token);

        // Act
        Action act = () => user.EnsureEmailConfirmed();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureEmailConfirmed_WhenEmailNotConfirmed_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");

        // Act
        Action act = () => user.EnsureEmailConfirmed();

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*not confirmed*");
    }

    // ========== 7. Избранное (Favorites) ==========

    //[Fact]
    //public void AddFavoriteProduct_WhenProductNotInFavorites_ShouldAdd()
    //{
    //    // Arrange
    //    var user = new User("test@mail.com", "hash", "John");
    //    var product = new Product("Phone", 599.99m, 10, "Test product");

    //    // Act
    //    user.AddFavoriteProduct(product);

    //    // Assert
    //    user.FavoriteProducts.Should().HaveCount(1);
    //    user.FavoriteProducts.First().ProductId.Should().Be(product.Id);
    //}

    //[Fact]
    //public void AddFavoriteProduct_WhenProductAlreadyInFavorites_ShouldThrowDomainException()
    //{
    //    // Arrange
    //    var user = new User("test@mail.com", "hash", "John");
    //    var product = new Product("Phone", 599.99m, 10, "Test product");
    //    user.AddFavoriteProduct(product);

    //    // Act
    //    Action act = () => user.AddFavoriteProduct(product);

    //    // Assert
    //    act.Should().Throw<DomainException>().WithMessage("*already in favorites*");
    //}

    //[Fact]
    //public void RemoveFavoriteProduct_WhenProductInFavorites_ShouldRemove()
    //{
    //    // Arrange
    //    var user = new User("test@mail.com", "hash", "John");
    //    var product = new Product("Phone", 599.99m, 10, "Test product");
    //    user.AddFavoriteProduct(product);

    //    // Act
    //    user.RemoveFavoriteProduct(product.Id);

    //    // Assert
    //    user.FavoriteProducts.Should().BeEmpty();
    //}

    //[Fact]
    //public void RemoveFavoriteProduct_WhenProductNotInFavorites_ShouldThrowDomainException()
    //{
    //    // Arrange
    //    var user = new User("test@mail.com", "hash", "John");

    //    // Act
    //    Action act = () => user.RemoveFavoriteProduct(999);

    //    // Assert
    //    act.Should().Throw<DomainException>().WithMessage("*not in favorites*");
    //}
}