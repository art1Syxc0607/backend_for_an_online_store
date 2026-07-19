using Application.Interfaces;
using Domain.Entities;
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
}

