// Domain/Enums/AgentMessageRole.cs
using System.Text.Json.Serialization;

namespace Domain.Enums;

// чтобы Swagger отображал строковые значения enum'а!
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AgentMessageRole
{
    User = 0,
    Assistant = 1,
    System = 2,
    Tool = 3
}