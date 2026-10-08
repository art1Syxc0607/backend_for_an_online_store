// Application/Queries/User/GetProfileHandler.cs
using Application.DTOs.User;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;

public class GetProfileHandler : IRequestHandler<GetProfileQuery, UserProfileDto>
{
    private readonly IUserRepository _userRepository;

    public GetProfileHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileDto> Handle(
        GetProfileQuery query,
        CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId, ct)
            ?? throw new DomainException("User not found");

        return new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsEmailConfirmed = user.IsEmailConfirmed,
            CreatedAt = user.CreatedAt
        };
    }
}