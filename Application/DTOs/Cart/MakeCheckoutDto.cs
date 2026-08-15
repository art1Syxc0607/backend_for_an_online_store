using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Cart;

public class MakeCheckoutDto
{
    [Required(ErrorMessage = "Shipping address is required")]
    [MinLength(5, ErrorMessage = "Shipping address must be at least 5 characters")]
    [MaxLength(500, ErrorMessage = "Shipping address cannot exceed 500 characters")]
    public string ShippingAddress { get; init; } = string.Empty;
}
