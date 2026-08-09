using Application.DTOs.Admin.User;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Admin.User;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserAdminDto>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserAdminDto>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        var users = await _userRepository.GetAllUsersFilteredAsync(
            request.Filter.Search,
            request.Filter.Role,
            request.Filter.IsActive,
            request.Filter.PageNumber,
            request.Filter.PageSize,
            request.Filter.SortBy,
            request.Filter.SortDesc,
            ct
        );

        return users.Select(u => new UserAdminDto
        {
            Id = u.Id,
            Email = u.Email,
            UserName = u.UserName,
            Role = u.Role,
            IsEmailConfirmed = u.IsEmailConfirmed,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            OrdersCount = u.Orders?.Count ?? 0,
            TotalSpent = u.Orders?.Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Delivered)
                .Sum(o => o.TotalAmount) ?? 0
        }).ToList();
    }
}