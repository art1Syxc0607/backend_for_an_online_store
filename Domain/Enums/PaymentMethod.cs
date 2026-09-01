using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Enums;

// чтобы Swagger отображал строковые значения enum'а!
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentMethod
{
    Card = 0,
    GooglePay = 1,
    ApplePay = 2,
    SBP = 3
}