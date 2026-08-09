using Application.DTOs.Admin.User;
using MediatR;

namespace Application.Queries.Admin.User;

public class GetAllUsersQuery : IRequest<List<UserAdminDto>>
{
    public UserFilterDto Filter { get; init; }
}
