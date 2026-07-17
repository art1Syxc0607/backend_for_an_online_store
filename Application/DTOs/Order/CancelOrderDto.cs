using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order;

public record CancelOrderDto
{
    [Required]
    public int UserId { get; set; }
    [Required]
    public int OrderId { get; set; }

}
