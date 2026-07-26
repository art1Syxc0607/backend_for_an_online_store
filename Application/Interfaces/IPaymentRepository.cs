using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IPaymentRepository
{
    Task<int> AddAsync(Payment payment, CancellationToken ct);
}
