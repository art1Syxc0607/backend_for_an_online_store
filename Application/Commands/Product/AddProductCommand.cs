using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Product;

public class AddProductCommand : IRequest<int>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public int StockQuantity { get; set; }
    public int? CategoryId { get; set; }
}
