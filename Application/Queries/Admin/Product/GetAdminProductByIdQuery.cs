using Application.DTOs.Admin.Product;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Admin.Product;

public class GetAdminProductByIdQuery : IRequest<AdminProductDto?>
{
    public int Id { get; init; }
}
