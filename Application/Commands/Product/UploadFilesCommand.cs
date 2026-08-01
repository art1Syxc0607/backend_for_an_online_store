using MediatR;
using Application.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Product;

public class UploadFilesCommand : IRequest<List<FileUploadResponseDto>>
{
    public int ProductId { get; init; }
    public List<FileUploadDto> Files { get; init; } = new();
}
