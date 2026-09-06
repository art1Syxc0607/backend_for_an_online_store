using Application.DTOs.Review;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Admin.Review;

public class GetReviewByIdCommand : IRequest<ReviewResponseDto>
{
    public int Id { get; set; }
}
