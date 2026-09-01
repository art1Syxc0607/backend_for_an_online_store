using System.Text.Json.Serialization;

namespace Domain.Enums;

// чтобы Swagger отображал строковые значения enum'а!
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    User = 0,
    Admin = 1
}