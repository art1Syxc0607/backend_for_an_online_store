using Application.Commands.Order;
using Application.Interfaces;
using MediatR;
using Domain.Exceptions;
using Domain.Entities;



namespace Application.Commands.Review;

public class AddReviewCommandHandler : IRequestHandler<AddReviewCommand, int>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddReviewCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork, IUserRepository userRepository, IProductRepository productRepository,
        IReviewRepository reviewRepository, IFileStorageService fileStorageService)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _reviewRepository = reviewRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<int> Handle(AddReviewCommand command, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, ct);
        if (user == null) throw new DomainException("No such user");
        // Проверка подтверждения email
        user.EnsureEmailConfirmed();

        var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
        if (product == null) throw new DomainException("No such product");

        if (!await _orderRepository.HasUserPurchasedProductAsync(command.UserId, command.ProductId, ct))
            throw new DomainException("User didn't buy or recieved this product.");

        // 4. Создаём отзыв
        var review = new Domain.Entities.Review(user, product, command.Text, command.Rating, true);

        // 5. ✅ СНАЧАЛА загружаем файлы
        if (command.Files != null)
        {
            var imageUrls = new List<string>();
            var videoUrls = new List<string>();

            foreach (var file in command.Files)
            {
                var url = await _fileStorageService.UploadFileAsync(
                    file.Stream,
                    file.FileName,
                    file.ContentType,
                    $"products/{product.Id}",
                    ct
                );

                if (file.ContentType.StartsWith("image/"))
                    imageUrls.Add(url);
                else if (file.ContentType.StartsWith("video/"))
                    videoUrls.Add(url);
            }

            if (imageUrls.Any())
                review.SetImageUrls(imageUrls);
            if (videoUrls.Any())
                review.SetVideoUrls(videoUrls);
        }

        // 6. Сохраняем отзыв (с файлами)
        await _reviewRepository.AddReviewAsync(review, ct);
        await _unitOfWork.SaveChangesAsync();

        // не нужно явно добовлять product.AddReview?
        //Короткий ответ: ДА, НЕ НУЖНО!
        //Если связь настроена правильно(через навигационные свойства), EF Core сам свяжет Review 
        //с Product и User.Явно добавлять product.Reviews.Add(review) не обязательно.

        return review.Id;

    }
}
