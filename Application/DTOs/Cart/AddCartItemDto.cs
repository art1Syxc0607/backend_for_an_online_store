

using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Cart;

public class AddCartItemDto
{
    [Required]
    public int productId { get; set; }
    [Required]
    public int countOfProduct { get; set; }
}

