using Application.Commands.Cart;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Cart
{
    public class AddToCartCommand : IRequest
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public int UserId { get; set; }

    }
}
