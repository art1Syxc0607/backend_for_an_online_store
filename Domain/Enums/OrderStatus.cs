using System.Text.Json.Serialization;

namespace Domain.Enums;

// чтобы Swagger отображал строковые значения enum'а!
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    Pending = 0,
    Paid = 1,
    Shipped = 2,
    Delivered = 3,
    Received = 4,
    Cancelled = 5
}