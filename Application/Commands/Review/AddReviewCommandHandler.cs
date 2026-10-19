using Application.Commands.Order;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Exceptions;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;



namespace Application.Commands.Review;

public class AddReviewCommandHandler : IRequestHandler<AddReviewCommand, int>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddReviewCommandHandler> _logger;

    public AddReviewCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork, IUserRepository userRepository, IProductRepository productRepository,
        IReviewRepository reviewRepository, IFileStorageService fileStorageService,
        ILogger<AddReviewCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _reviewRepository = reviewRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
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
            throw new DomainException("User didn't recieved this product.");

        // ✅ Проверка 2: не оставлял ли уже отзыв
        if (await _reviewRepository.HasUserReviewedProductAsync(
                command.UserId, command.ProductId, ct))
        {
            throw new DomainException(
                "You have already reviewed this product. " +
                "Only one review per product is allowed.");
        }

        // 4. Создаём отзыв
        var review = new Domain.Entities.Review(user, product, command.Text, command.Rating, true);

        var uploadedUrls = new List<string>();

        try
        {
            // 5. ✅ СНАЧАЛА загружаем файлы
            if (command.Files != null)
            {
                // ✅ Валидация ДО записи на диск — чтобы не оставить orphan-файлы
                review.EnsureCanAddFiles(command.Files.Count);

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
                    uploadedUrls.Add(url);

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
            await _unitOfWork.SaveChangesAsync(ct);

            // не нужно явно добовлять product.AddReview?
            //Короткий ответ: ДА, НЕ НУЖНО!
            //Если связь настроена правильно(через навигационные свойства), EF Core сам свяжет Review
            //с Product и User.Явно добавлять product.Reviews.Add(review) не обязательно.

            return review.Id;
        }
        catch
        {
            // ✅ Компенсация: БД откатит SaveChanges, а файлы удаляем вручную
            await TryDeleteUploadedFilesAsync(uploadedUrls, ct);
            throw;
        }
    }

    private async Task TryDeleteUploadedFilesAsync(List<string> urls, CancellationToken ct)
    {
        if (urls.Count == 0)
            return;

        try
        {
            await _fileStorageService.DeleteMultipleFilesAsync(urls, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to roll back {Count} uploaded review file(s)", urls.Count);
        }
    }
}
