using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Email;

public class SendOrderConfirmationCommand : IRequest
{
    //public int OrderId { get; init; } // before
    [Required]
    public Domain.Entities.Order Order { get; set; }
    [Required]
    public Domain.Entities.User User { get; set; }
}
