using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Review;

public class AddReviewCommand : IRequest<int>
{
    public int UserId { get; init; }
    public int ProductId { get; init; }
    public string Text { get; init; }
    public int Rating { get; init; } // 1-5 stars
    //public bool IsVerifiedPurchase { get; private set; }
}
