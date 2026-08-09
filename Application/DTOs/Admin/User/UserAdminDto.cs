using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Admin.User;


public class UserAdminDto
{
    public int Id { get; init; }
    public string Email { get; init; }
    public string UserName { get; init; }
    public UserRole Role { get; init; }
    public bool IsEmailConfirmed { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public int OrdersCount { get; init; }
    public decimal TotalSpent { get; init; }
}

public class UserFilterDto
{
    public string? Search { get; set; }          // Поиск по имени/email
    public UserRole? Role { get; set; }          // Фильтр по роли
    public bool? IsActive { get; set; }          // Фильтр по статусу
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public SortUserBy SortBy { get; set; } = SortUserBy.CreatedAt;
    public bool SortDesc { get; set; } = true;
}

public enum SortUserBy
{
    CreatedAt,
    UserName,
    Email,
    OrdersCount,
    TotalSpent
}