using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Review;


public class RespondToReviewCommand : IRequest
{
    public int ReviewId { get; init; }
    public int AdminId { get; init; }
    public string Response { get; init; } = string.Empty;
}