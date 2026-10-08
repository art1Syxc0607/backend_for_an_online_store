// Application/Queries/User/GetProfileQuery.cs
using Application.DTOs.User;
using MediatR;

public class GetProfileQuery : IRequest<UserProfileDto>
{
    public int UserId { get; init; }
}