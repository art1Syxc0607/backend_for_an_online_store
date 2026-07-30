using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.DTOs.Product;

public class AddProductDto
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int? CategoryId { get; set; }
    public string Description { get; set; }
    public IFormFile? ImageFile { get; set; } // ← файл внутри DTO!
}