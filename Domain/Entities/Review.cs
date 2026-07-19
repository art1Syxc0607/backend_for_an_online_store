using Domain.Exceptions;

namespace Domain.Entities;

public class Review
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int ProductId { get; private set; }
    public string Text { get; private set; }
    public int Rating { get; private set; } // 1-5 stars
    public bool IsVerifiedPurchase { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    // навигацонные свойства
    public virtual User User { get; private set; }
    public virtual Product Product { get; private set; }

    private Review() { }

    public Review(User user, Product product, string text, int rating, bool isVerifiedPurchase)
    {
        User = user ?? throw new DomainException("User cannot be null.");
        Product = product ?? throw new DomainException("Product cannot be null.");
        UserId = user.Id;
        ProductId = product.Id;
        SetText(text);
        SetRating(rating);
        IsVerifiedPurchase = isVerifiedPurchase;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string? newText = null, int? newRating = null)
    {
        if (newText != null) SetText(newText);
        if (newRating.HasValue) SetRating(newRating.Value);
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Review text cannot be empty.");
        Text = text;
    }

    private void SetRating(int rating)
    {
        if (rating < 1 || rating > 5)
            throw new DomainException("Rating must be between 1 and 5.");
        Rating = rating;
    }
}