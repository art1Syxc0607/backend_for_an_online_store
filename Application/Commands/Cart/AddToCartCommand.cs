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
        public int productId { get; set; }
        [Required]
        public int countOfProduct { get; set; }
        [Required]
        public int userId { get; set; }

    }
}
