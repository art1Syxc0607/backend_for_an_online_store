using MediatR;


namespace Application.Commands.Admin.User;

public class DemoteFromAdminCommand : IRequest
{
    public int UserId { get; init; }
    public int AdminId { get; init; }
}