using Application.DTOs.Email;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Email;

public class ResendConfirmationHandler : IRequestHandler<ResendConfirmationCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public ResendConfirmationHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        ITokenGenerator tokenGenerator,
        IConfiguration configuration,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _tokenGenerator = tokenGenerator;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ResendConfirmationCommand command, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email, ct);
        if (user == null)
            throw new DomainException("User not found");

        if (user.IsEmailConfirmed)
            throw new DomainException("Email already confirmed");

        // Генерируем новый токен
        var token = _tokenGenerator.GenerateEmailConfirmationToken();
        var expiry = DateTime.UtcNow.AddHours(24);
        user.GenerateEmailConfirmationToken(token, expiry);

        await _unitOfWork.SaveChangesAsync(ct);

        // Отправляем письмо
        var baseUrl = _configuration["App:BaseUrl"];
        var confirmationUrl = $"{baseUrl}/api/auth/confirm-email?token={token}&userId={user.Id}";

        string To = user.Email;
        string Subject = "Подтверждение регистрации";
        string Body = $@"
                <h2>Здравствуйте, {user.UserName}!</h2>
                <p>Подтвердите ваш email по ссылке:</p>
                <p><a href='{confirmationUrl}'>Подтвердить email</a></p>
                <p>Ссылка действительна в течение 24 часов.</p>
            ";
        bool IsHtml = true;


        await _emailService.SendEmailAsync(To, Subject, Body, IsHtml);
    }
}
