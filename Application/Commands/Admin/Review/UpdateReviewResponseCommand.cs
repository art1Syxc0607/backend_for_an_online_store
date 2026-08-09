using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Review;


public class UpdateReviewResponseCommand : IRequest
{
    public int ReviewId { get; init; }
    public string NewResponse { get; init; } = string.Empty;
}