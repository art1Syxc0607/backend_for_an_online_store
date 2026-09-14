using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Review;


public class RespondToReviewCommand : IRequest
{
    [Required]
    public int ReviewId { get; init; }
    [Required]
    public int AdminId { get; init; }
    [Required]
    public string Response { get; init; } = string.Empty;
    [Required]
    public string BaseUrl { get; init; } = string.Empty;
}