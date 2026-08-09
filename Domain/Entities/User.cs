using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class User
{
    private List<Review> _reviews = new();
    private List<Order> _orders = new();
    //private List<FavoriteProduct> _favoriteProducts = new();
    //private List<FavoriteSeller> _favoriteSellers = new();

    public int Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }= string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; private set; } 
    public DateTime CreatedAt { get; private set; }


    public bool IsActive { get; private set; } = true;
    public string? BlockReason { get; private set; }
    public DateTime? BlockedAt { get; private set; }

    // Registration confirming
    public bool IsEmailConfirmed { get; private set; }
    public DateTime? EmailConfirmedAt { get; private set; }
    public string? EmailConfirmationToken { get; private set; }
    public DateTime? EmailConfirmationTokenExpiry { get; private set; }

    // Навигационные свойства
    public Cart? Cart { get; private set; }
    public virtual IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();
    public virtual IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();
    //public virtual IReadOnlyCollection<FavoriteProduct> FavoriteProducts => _favoriteProducts.AsReadOnly();
    //public virtual IReadOnlyCollection<FavoriteSeller> FavoriteSellers => _favoriteSellers.AsReadOnly();

    public User(string email, string passwordHash, string userName)
    {
        SetEmail(email);
        SetPasswordHash(passwordHash);
        SetName(userName);
        Role = UserRole.Customer;
        CreatedAt = DateTime.UtcNow;
    }

    public void TestsSetUser(int? id = null)
    {
        if(id != null) Id = id.Value;
    }

    public void Block(string? reason = null)
    {
        if (!IsActive)
            throw new DomainException("User is already blocked");

        if (Role == UserRole.Admin)
            throw new DomainException("Cannot block an admin");

        IsActive = false;
        BlockReason = reason;
        BlockedAt = DateTime.UtcNow;
    }

    public void Unblock()
    {
        if (IsActive)
            throw new DomainException("User is not blocked");

        IsActive = true;
        BlockReason = null;
        BlockedAt = null;
    }

    // Метод для генерации токена подтверждения
    public void GenerateEmailConfirmationToken(string token, DateTime expiry)
    {
        EmailConfirmationToken = token;
        EmailConfirmationTokenExpiry = expiry;
        IsEmailConfirmed = false;
    }

    // Метод для подтверждения email
    public void ConfirmEmail(string token)
    {
        if (IsEmailConfirmed)
            throw new DomainException("Email already confirmed");

        if (EmailConfirmationToken != token)
            throw new DomainException("Invalid confirmation token");

        if (EmailConfirmationTokenExpiry < DateTime.UtcNow)
            throw new DomainException("Confirmation token expired");

        IsEmailConfirmed = true;
        EmailConfirmedAt = DateTime.UtcNow;
        EmailConfirmationToken = null;
        EmailConfirmationTokenExpiry = null;
    }

    public void EnsureEmailConfirmed()
    {
        if (!IsEmailConfirmed)
            throw new DomainException("Email not confirmed. Please confirm your email first.");
    }

    // Бизнес-методы
    public void UpdateProfile(string? email = null, string? name = null)
    {
        if (email != null)
            SetEmail(email);
        if (name != null)
            SetName(name);
    }
    public void SetCart(Cart cart)
    {
        if (cart == null) throw new DomainException("Cart is Null");

        Cart = cart;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        SetPasswordHash(newPasswordHash);
    }

    public void PromoteToAdmin()
    {
        if (Role == UserRole.Admin)
            throw new DomainException("User is already an admin.");
        Role = UserRole.Admin;
    }

    public void DemoteFromAdmin()
    {
        if (Role == UserRole.Customer)
            throw new DomainException("User is not an admin.");
        Role = UserRole.Customer;
    }

    // Приватные методы для валидации
    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty.");
        if (!email.Contains('@')) // Простая валидация
            throw new DomainException("Invalid email format.");
        Email = email;
    }


    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Full name cannot be empty.");
        UserName = name;
    }

    private void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash cannot be empty.");
        PasswordHash = passwordHash;
    }
}

