using Application.DTOs.User;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Commands.User;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email, ct))
            throw new DomainException("The user with this Email already exist");

        if (await _userRepository.ExistsByUserNameAsync(request.UserName, ct))
            throw new DomainException("The user with this Name already exist");

        var user = new Domain.Entities.User(request.Email, _passwordHasher.HashPassword(request.Password),
            request.UserName);
        var cart = new Domain.Entities.Cart(user);
        user.SetCart(cart);

        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);


        var response = new AuthResponseDto
        {
            Email = request.Email,
            UserName = request.UserName,
            Token = _jwtService.GenerateToken(user),
            UserId = user.Id,
        };

        return response;
    }

}

