using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Product;

public class DeleteAllFilesCommand : IRequest
{
    public int ProductId { get; init; }
}