using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Enums;

// чтобы Swagger отображал строковые значения enum'а!
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DateSpan
{
    Day = 1,
    Week = 2,
    Month = 3,
    HalfOfMonth = 4,
    HalfOfYear = 5,
    Year = 6,

}
