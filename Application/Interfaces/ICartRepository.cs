using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Locks the user's cart row for the duration of the current transaction.
    /// Used by checkout so two concurrent requests cannot create duplicate orders.
    /// </summary>
    Task<Cart?> GetByUserIdForCheckoutAsync(int userId, CancellationToken ct = default);

    Task UpdateAsync(Cart cart, CancellationToken ct = default);
    //Task<Cart?> GetByIdAsync(int id, CancellationToken ct = default);
    //Task AddAsync(Cart cart, CancellationToken ct = default);
    //Task UpdateAsync(Cart cart, CancellationToken ct = default);
}

