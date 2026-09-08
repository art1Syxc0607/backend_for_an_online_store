using Application.DTOs.Product;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Review;

public class DeleteReviewFilesCommand : IRequest<DeleteFilesResponseDto>
{
    public int ReviewId { get; set; }
    public int UserId { get; set; }
    public List<string> FileUrls { get; init; } = new(); // список URL для удаления
}
