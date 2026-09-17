using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order;

public class CancelOrderDto
{
    [Required(ErrorMessage = "Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid order ID")]
    public int OrderId { get; init; }
}
