using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.Order;

public record OrderItemDto(Product Product, int Quantity, decimal PriceAtPurchase);