using Application.DTOs.Admin.User;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) 
        => _context = context;

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        return user;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>u.Email == email);
        return user;
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        return user;
    }

    public async Task<User?> GetByIdAsyncWithCart(int id, CancellationToken ct = default)
    {
        var user = await _context.Users.Include(u => u.Cart)
            .ThenInclude(c => c.Items)
            .FirstOrDefaultAsync(u => u.Id == id);
        return user;
    }
    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user);
    }

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Remove(user);
        return Task.CompletedTask;
    }

    public async Task<List<User>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _context.Users.ToListAsync();

        return users;
    }

    //public async Task<bool> IfBuyThisProduct(int productId, CancellationToken ct = default)
    //{
    //    var result = _context.Users
    //}
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email ==  email) != null; // true если есть
    }

    public async Task<bool> ExistsByUserNameAsync(string userName, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName) != null;
    }
    public async Task<bool> ExistsByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id) != null;
    }

    // Admin, User
    // Infrastructure/Repositories/UserRepository.cs
    public async Task<List<User>> GetAllUsersFilteredAsync(
        string? search,
        UserRole? role,
        bool? isActive,
        int pageNumber,
        int pageSize,
        SortUserBy sortBy,
        bool sortDesc,
        CancellationToken ct)
    {
        var query = _context.Users
            .Include(u => u.Orders)
            .AsQueryable();

        // ✅ Поиск по имени или email
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => u.UserName.Contains(search) || u.Email.Contains(search));
        }

        // ✅ Фильтр по роли
        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        // ✅ Фильтр по статусу
        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        // ✅ Сортировка
        query = sortBy switch
        {
            SortUserBy.CreatedAt => sortDesc
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),
            SortUserBy.UserName => sortDesc
                ? query.OrderByDescending(u => u.UserName)
                : query.OrderBy(u => u.UserName),
            SortUserBy.Email => sortDesc
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            SortUserBy.OrdersCount => sortDesc
                ? query.OrderByDescending(u => u.Orders.Count)
                : query.OrderBy(u => u.Orders.Count),
            SortUserBy.TotalSpent => sortDesc
                ? query.OrderByDescending(u => u.Orders.Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount))
                : query.OrderBy(u => u.Orders.Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount)),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        // ✅ Пагинация
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountAdminsAsync(CancellationToken ct)
    {
        return await _context.Users.CountAsync(u => u.Role == UserRole.Admin, ct);
    }
}

