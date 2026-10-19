using System;
using System.Collections.Generic;

namespace Application.DTOs.Admin.User;

// ✅ Полная информация о пользователе для админ-панели
public class AdminUserDetailDto
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;

    public bool IsEmailConfirmed { get; init; }
    public DateTime? EmailConfirmedAt { get; init; }
    public bool IsActive { get; init; }
    public string? BlockReason { get; init; }
    public DateTime? BlockedAt { get; init; }
    public DateTime CreatedAt { get; init; }

    public int OrdersCount { get; init; }
    public decimal TotalSpent { get; init; }
    public int ReviewsCount { get; init; }

    public List<AdminUserOrderBriefDto> Orders { get; init; } = new();
    public List<AdminUserReviewBriefDto> Reviews { get; init; } = new();
}

public class AdminUserOrderBriefDto
{
    public int OrderId { get; init; }
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public class AdminUserReviewBriefDto
{
    public int ReviewId { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Rating { get; init; }
    public string Text { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
