using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Application.Commands.Order;

public class CancelOrderCommand :IRequest
{
    [Required]
    public int UserId { get; set; }
    [Required]
    public int OrderId { get; set; }
}
