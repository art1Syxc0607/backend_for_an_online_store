using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order;

public record NumberOfNewOrdersForThePeriodResponseDto
{
    public int NumberOfPending { get; init; }
    public int NumberOfPaid { get; init; }
    public int NumberOfShipped { get; init; }
    public int NumberOfDelivered { get; init; }
    public int NumberOfReceived { get; init; }
    public int NumberOfCancelled { get; init; }
    public int TotalOrders { get; init; }
}
