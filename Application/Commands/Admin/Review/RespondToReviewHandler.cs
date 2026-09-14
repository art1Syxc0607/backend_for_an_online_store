using Application.DTOs.Email;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Review;

// Application/Commands/Review/RespondToReview/RespondToReviewHandler.cs
public class RespondToReviewHandler : IRequestHandler<RespondToReviewCommand>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IEmailBackgroundService _emailBackgroundService;
    private readonly ILogger<RespondToReviewHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public RespondToReviewHandler(
        IReviewRepository reviewRepository,
        IUserRepository userRepository,
        IProductRepository productRepository,
        IEmailTemplateService emailTemplateService,
        IEmailBackgroundService emailBackgroundService,
        ILogger<RespondToReviewHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _emailTemplateService = emailTemplateService;
        _emailBackgroundService = emailBackgroundService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RespondToReviewCommand command, CancellationToken ct)
    {
        // 1. Получаем отзыв
        var review = await _reviewRepository.GetReviewByIdAsync(command.ReviewId, ct);
        if (review == null)
            throw new DomainException("Review not found");

        if(!string.IsNullOrWhiteSpace(review.AdminResponse))
            throw new DomainException("Review is answered");

        // 2. Проверяем, что отзыв одобрен
        if (review.Status != ReviewStatus.Approved)
            throw new DomainException("Cannot respond to a review that is not approved");

        var user = await _userRepository.GetByIdAsync(review.UserId, ct);
        if (user == null)
            throw new DomainException("User not found");

        var product = await _productRepository.GetByIdAsync(review.ProductId, ct);
        if (product == null)
            throw new DomainException("Product not found");

        // 3. Проверяем, что админ существует
        var admin = await _userRepository.GetByIdAsync(command.AdminId, ct);
        if (admin == null)
            throw new DomainException("Admin not found");

        if (admin.Role != UserRole.Admin)
            throw new UnauthorizedAccessException("Only admins can respond to reviews");

        // 4. Добавляем ответ
        review.AddAdminResponse(command.Response);

        await _unitOfWork.SaveChangesAsync(ct);

        // ✅ Отправляем email в фоне
        try
        {
            var email = _emailTemplateService.CreateReviewResponseNotificationEmail(
                review,
                user,
                product,
                command.BaseUrl
            );

            await _emailBackgroundService.Enqueue(email);

            _logger.LogInformation(
                "Review response notification email queued for {Email}, ReviewId {ReviewId}",
                user.Email, review.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to queue review response email for {Email}", user.Email);
        }
    }
}