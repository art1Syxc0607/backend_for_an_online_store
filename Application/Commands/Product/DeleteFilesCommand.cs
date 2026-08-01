using MediatR;
using Application.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Product;

public class DeleteFilesCommand : IRequest<DeleteFilesResponseDto>
{
    public int ProductId { get; init; }
    public List<string> FileUrls { get; init; } = new(); // список URL для удаления
}
