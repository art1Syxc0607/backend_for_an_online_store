using Application.DTOs.Review;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Review;

public class GetProductReviewsCommand : IRequest<List<ReviewResponseDto>>
{
    public int ProductId { get; init; }
}
