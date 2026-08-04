using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order;

public record OrderItemDto(int ProductId, int Quantity, decimal PriceAtPurchase, 
    string ProductNameAtPurchase);
