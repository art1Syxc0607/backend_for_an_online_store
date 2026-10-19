using Application.DTOs.Admin.User;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.Admin.User;

public class GetAdminUserByIdHandler : IRequestHandler<GetAdminUserByIdQuery, AdminUserDetailDto?>
{
    private readonly IUserRepository _userRepository;

    public GetAdminUserByIdHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AdminUserDetailDto?> Handle(GetAdminUserByIdQuery request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdWithDetailsAsync(request.UserId, ct);
        if (user == null)
            return null;

        var orders = user.Orders.ToList();
        var reviews = user.Reviews.ToList();

        return new AdminUserDetailDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            Role = user.Role.ToString(),

            IsEmailConfirmed = user.IsEmailConfirmed,
            EmailConfirmedAt = user.EmailConfirmedAt,
            IsActive = user.IsActive,
            BlockReason = user.BlockReason,
            BlockedAt = user.BlockedAt,
            CreatedAt = user.CreatedAt,

            OrdersCount = orders.Count,
            TotalSpent = orders
                .Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Delivered)
                .Sum(o => o.TotalAmount),
            ReviewsCount = reviews.Count,

            Orders = orders
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new AdminUserOrderBriefDto
                {
                    OrderId = o.Id,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt
                })
                .ToList(),

            Reviews = reviews
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new AdminUserReviewBriefDto
                {
                    ReviewId = r.Id,
                    ProductId = r.ProductId,
                    ProductName = r.Product != null ? r.Product.Name : string.Empty,
                    Rating = r.Rating,
                    Text = r.Text,
                    CreatedAt = r.CreatedAt
                })
                .ToList()
        };
    }
}
