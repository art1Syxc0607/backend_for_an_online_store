using MediatR;
using Application.DTOs.File;

namespace Application.Commands.Review;

public class UploadReviewFilesCommand : IRequest<List<FileUploadResponseDto>>
{
    public int ReviewId { get; init; }
    public List<FileUploadDto> Files { get; init; } = new();
}
