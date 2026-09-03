using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Product;

public class UpdateProductCommand : IRequest
{
    public int ProductId { get; set; }
    public int? CategoryId { get; set; }
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public decimal? PurchasePrice { get; set; }
    public int? StockQuantity { get; set; }
    //public int? ReservedQuantity { get; set; }
    public string? Description { get; set; }
    public string? Sku { get; set; }

}
