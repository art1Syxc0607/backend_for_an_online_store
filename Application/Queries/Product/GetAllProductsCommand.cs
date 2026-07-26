using Application.DTOs.Product;
using MediatR;

namespace Application.Queries.Product;

public class GetAllProductsCommand : IRequest<List<ProductResponseDto>>
{

}
