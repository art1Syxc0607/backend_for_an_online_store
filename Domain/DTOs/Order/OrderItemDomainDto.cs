using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.Order;

public record OrderItemDomainDto(Product Product, int Quantity, 
    decimal? PriceAtPurchase = null);