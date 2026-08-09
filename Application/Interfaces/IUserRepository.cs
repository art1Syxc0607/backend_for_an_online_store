using Application.DTOs.Admin.User;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default);

    Task<User?> GetByIdAsyncWithCart(int id, CancellationToken ct = default);
    Task<List<User>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task DeleteAsync(User user, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    //Task<bool> IfBuyThisProduct(int productId, CancellationToken ct = default);
    Task<bool> ExistsByUserNameAsync(string userName, CancellationToken ct = default);
    Task<bool> ExistsByIdAsync(int id, CancellationToken ct = default);


    // Admin
    Task<List<User>> GetAllUsersFilteredAsync(
        string? search,
        UserRole? role,
        bool? isActive,
        int pageNumber,
        int pageSize,
        SortUserBy sortBy,
        bool sortDesc,
        CancellationToken ct);

    Task<int> CountAdminsAsync(CancellationToken ct);
}