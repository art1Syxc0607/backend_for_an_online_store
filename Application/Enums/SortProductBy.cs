using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Enums;

// чтобы Swagger отображал строковые значения enum'а!
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortProductBy
{
    Name = 0,
    Price = 1,
    AvailableQuantity = 2,
    Rating = 3,
    ReviewAmount = 4,
    PaymentAmount = 5,
}

