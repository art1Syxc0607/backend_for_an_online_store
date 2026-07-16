using System.Threading.Tasks;
using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;

namespace Application.Queries.Order;

public class GetOrderHistoryQuery : IRequest<List<OrderResponseDto>>
{
    public int userId { get; init; }
}
