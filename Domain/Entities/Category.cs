using Domain.Exceptions;

namespace Domain.Entities;

public class Category
{
    private List<Product> _products = new();

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private Category() { }

    public Category(string name, string? description = null)
    {
        SetName(name);
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string? name = null, string? description = null)
    {
        if (name != null) SetName(name);
        if (description != null) Description = description;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name cannot be empty.");
        if (name.Length > 100)
            throw new DomainException("Category name cannot exceed 100 characters.");
        Name = name;
    }
}