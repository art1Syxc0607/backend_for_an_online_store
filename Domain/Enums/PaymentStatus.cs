using System.Text.Json.Serialization;

namespace Domain.Enums;

// чтобы Swagger отображал строковые значения enum'а!
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3
}