using Domain.Exceptions;
using Domain.Enums;
using System.Text.Json.Serialization;

namespace Domain.Entities;

public class Review
{
    private List<string> _imageUrls = new();
    private List<string> _videoUrls = new();

    private const int MaxFiles = 8;

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int ProductId { get; private set; }
    public string Text { get; private set; }
    public int Rating { get; private set; } // 1-5 stars
    public bool IsVerifiedPurchase { get; private set; }

    public ReviewStatus Status { get; private set; } = ReviewStatus.Approved;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // AdminResponse
    public string? AdminResponse { get; private set; }
    public DateTime? AdminResponseAt { get; private set; }


    // навигацонные свойства
    public virtual User User { get; private set; }
    public virtual Product Product { get; private set; }
    public IReadOnlyCollection<string> ImageUrls => _imageUrls.AsReadOnly();
    public IReadOnlyCollection<string> VideoUrls => _videoUrls.AsReadOnly();

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

    public void TestsSetReviewId(int id)
    {
        Id = id;
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

    // работа с файлами
    // ========== ОБНОВЛЕНИЕ ОДНОГО URL ==========
    public void UpdateImageUrl(string oldUrl, string newUrl)
    {
        // 1. Проверка входных данных
        if (string.IsNullOrWhiteSpace(oldUrl))
            throw new DomainException("Old image URL cannot be empty");
        if (string.IsNullOrWhiteSpace(newUrl))
            throw new DomainException("New image URL cannot be empty");

        // 2. Проверка, что старый URL существует
        if (!_imageUrls.Contains(oldUrl))
            throw new DomainException($"Image not found: {oldUrl}");

        // 3. Проверка, что новый URL не дублирует существующий (кроме самого себя)
        if (_imageUrls.Contains(newUrl) && newUrl != oldUrl)
            throw new DomainException($"Image URL already exists: {newUrl}");

        // 4. Заменяем
        var index = _imageUrls.IndexOf(oldUrl);
        _imageUrls[index] = newUrl;
    }

    public void UpdateVideoUrl(string oldUrl, string newUrl)
    {
        if (string.IsNullOrWhiteSpace(oldUrl))
            throw new DomainException("Old video URL cannot be empty");
        if (string.IsNullOrWhiteSpace(newUrl))
            throw new DomainException("New video URL cannot be empty");

        if (!_videoUrls.Contains(oldUrl))
            throw new DomainException($"Video not found: {oldUrl}");

        if (_videoUrls.Contains(newUrl) && newUrl != oldUrl)
            throw new DomainException($"Video URL already exists: {newUrl}");

        var index = _videoUrls.IndexOf(oldUrl);
        _videoUrls[index] = newUrl;
    }

    //AdminResponse

    public void AddAdminResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            throw new DomainException("Response cannot be empty.");

        if (Status != ReviewStatus.Approved)
            throw new DomainException("Cannot respond to a review that is not approved.");

        AdminResponse = response;
        AdminResponseAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAdminResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            throw new DomainException("Response cannot be empty.");

        if (AdminResponse == null)
            throw new DomainException("Cannot update a response that doesn't exist.");

        AdminResponse = response;
        AdminResponseAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveAdminResponse()
    {
        if (AdminResponse == null)
            throw new DomainException("No response to remove.");

        AdminResponse = null;
        AdminResponseAt = null;
        UpdatedAt = DateTime.UtcNow;
    }


    // ========== УДАЛЕНИЕ ОДНОГО URL ==========
    public void RemoveImage(string url)
    {
        // 1. Проверка входных данных
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Image URL cannot be empty");

        // 2. Проверка, что URL существует
        if (!_imageUrls.Remove(url))
            throw new DomainException($"Image not found: {url}");
    }

    public void RemoveVideo(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Video URL cannot be empty");

        if (!_videoUrls.Remove(url))
            throw new DomainException($"Video not found: {url}");
    }

    // ========== МАССОВАЯ ЗАГРУЗКА ==========
    public void SetImageUrls(List<string> urls)
    {
        if (urls == null || !urls.Any())
            throw new DomainException("Image URLs cannot be null or empty");

        AddUrls(urls, _imageUrls, "image");
    }

    public void SetVideoUrls(List<string> urls)
    {
        if (urls == null || !urls.Any())
            throw new DomainException("Video URLs cannot be null or empty");

        AddUrls(urls, _videoUrls, "video");
    }

    // ✅ Общий метод для добавления URL
    private void AddUrls(List<string> urls, List<string> targetCollection, string type)
    {
        var newUrls = urls.Distinct().ToList();
        var existingUrls = targetCollection.ToHashSet();

        var duplicates = newUrls.Where(u => existingUrls.Contains(u)).ToList();
        if (duplicates.Any())
            throw new DomainException($"Duplicate {type} URLs found: {string.Join(", ", duplicates)}");

        var totalFiles = _imageUrls.Count + _videoUrls.Count + newUrls.Count;
        if (totalFiles > MaxFiles)
            throw new DomainException($"Maximum {MaxFiles} files allowed (current: {_imageUrls.Count + _videoUrls.Count}, adding: {newUrls.Count})");

        targetCollection.AddRange(newUrls);
    }

    // ========== МАССОВОЕ УДАЛЕНИЕ ==========
    public void RemoveImages(List<string> urls)
    {
        if (urls == null || !urls.Any())
            throw new DomainException("No files specified for removal");

        // ✅ Проверяем, что все URL существуют
        var missing = urls.Where(u => !_imageUrls.Contains(u)).ToList();
        if (missing.Any())
            throw new DomainException($"Image(s) not found: {string.Join(", ", missing)}");

        // ✅ Удаляем все (убираем дубли в запросе)
        var toRemove = urls.Distinct().ToList();
        foreach (var url in toRemove)
        {
            _imageUrls.Remove(url);
        }
    }

    public void RemoveVideos(List<string> urls)
    {
        if (urls == null || !urls.Any())
            throw new DomainException("No files specified for removal");

        var missing = urls.Where(u => !_videoUrls.Contains(u)).ToList();
        if (missing.Any())
            throw new DomainException($"Video(s) not found: {string.Join(", ", missing)}");

        var toRemove = urls.Distinct().ToList();
        foreach (var url in toRemove)
        {
            _videoUrls.Remove(url);
        }
    }

    // ========== МАССОВАЯ ЗАМЕНА ==========
    public void ReplaceImageUrls(List<string> urls)
    {
        if (urls == null || !urls.Any())
            throw new DomainException("Image URLs cannot be null or empty");

        var uniqueUrls = urls.Distinct().ToList();
        if (uniqueUrls.Count > 8)
            throw new DomainException($"Maximum 8 images allowed (received: {uniqueUrls.Count})");

        _imageUrls.Clear();
        _imageUrls.AddRange(uniqueUrls);
    }

    public void ReplaceVideoUrls(List<string> urls)
    {
        if (urls == null || !urls.Any())
            throw new DomainException("Video URLs cannot be null or empty");

        var uniqueUrls = urls.Distinct().ToList();
        if (uniqueUrls.Count > 2)
            throw new DomainException($"Maximum 2 videos allowed (received: {uniqueUrls.Count})");

        _videoUrls.Clear();
        _videoUrls.AddRange(uniqueUrls);
    }

    // ========== ПОЛУЧЕНИЕ ВСЕХ URL ==========
    public List<string> GetAllFileUrls()
    {
        var all = new List<string>();
        all.AddRange(_imageUrls);
        all.AddRange(_videoUrls);
        return all;
    }

    // очистка
    public void ClearImageUrls()
    {
        _imageUrls.Clear();
    }

    public void ClearVideoUrls()
    {
        _videoUrls.Clear();
    }

    public void ClearAllFiles()
    {
        _imageUrls.Clear();
        _videoUrls.Clear();
    }
}