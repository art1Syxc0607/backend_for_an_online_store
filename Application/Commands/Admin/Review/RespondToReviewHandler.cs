using Application.DTOs.Email;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
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
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public RespondToReviewHandler(
        IReviewRepository reviewRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RespondToReviewCommand command, CancellationToken ct)
    {
        // 1. Получаем отзыв
        var review = await _reviewRepository.GetReviewByIdAsync(command.ReviewId, ct);
        if (review == null)
            throw new DomainException("Review not found");

        // 2. Проверяем, что отзыв одобрен
        if (review.Status != ReviewStatus.Approved)
            throw new DomainException("Cannot respond to a review that is not approved");

        // 3. Проверяем, что админ существует
        var admin = await _userRepository.GetByIdAsync(command.AdminId, ct);
        if (admin == null)
            throw new DomainException("Admin not found");

        if (admin.Role != UserRole.Admin)
            throw new UnauthorizedAccessException("Only admins can respond to reviews");

        // 4. Добавляем ответ
        review.AddAdminResponse(command.Response);

        await _unitOfWork.SaveChangesAsync(ct);

        // 5. Отправляем уведомление пользователю (опционально)
        await SendNotificationAsync(review.User, review.Product, review, command.Response, ct);
    }

    private async Task SendNotificationAsync(Domain.Entities.User user, Domain.Entities.Product product,
        Domain.Entities.Review review, string response, CancellationToken ct)
    {
        var emailDto = new EmailDto
        {
            To = user.Email,
            Subject = $"Ответ на ваш отзыв о товаре \"{product.Name}\"",
            Body = $@"
                <html>
                    <body>
                        <h2>Здравствуйте, {user.UserName}!</h2>
                        <p>Администратор ответил на ваш отзыв о товаре <strong>\{product.Name}\</strong>:</p>
                        < div style = 'background: #f5f5f5; padding: 15px; border-radius: 8px; margin: 15px 0;' >
                            < p >< strong > Ваш отзыв:</ strong > {review.Text}</ p >
                            < p >< strong > Ответ администратора:</ strong > {response}</ p >
                        </ div >
                        < p > С уважением, команда магазина.</ p >
                    </ body >
                </ html >
            ",
            IsHtml = true
        };

        await _emailService.SendEmailAsync(emailDto);
    }
}