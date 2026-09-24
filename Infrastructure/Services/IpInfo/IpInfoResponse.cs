using System.Text.Json.Serialization;

namespace Infrastructure.Services.IpInfo;

/// <summary>
/// Ответ от IPinfo API (https://ipinfo.io/{ip}/json).
/// </summary>
public class IpInfoResponse
{
    /// <summary>
    /// IP-адрес, для которого запрошены данные.
    /// Пример: "8.8.8.8"
    /// </summary>
    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    /// <summary>
    /// Hostname (если есть).
    /// Пример: "dns.google"
    /// </summary>
    [JsonPropertyName("hostname")]
    public string? Hostname { get; set; }

    /// <summary>
    /// Город.
    /// Пример: "Mountain View"
    /// </summary>
    [JsonPropertyName("city")]
    public string? City { get; set; }

    /// <summary>
    /// Регион (штат, область).
    /// Пример: "California"
    /// </summary>
    [JsonPropertyName("region")]
    public string? Region { get; set; }

    /// <summary>
    /// Код страны (ISO 3166-1 alpha-2).
    /// Пример: "US"
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    /// <summary>
    /// Координаты в формате "lat,lng".
    /// Пример: "37.4056,-122.0775"
    /// </summary>
    [JsonPropertyName("loc")]
    public string? Loc { get; set; }

    /// <summary>
    /// Организация: AS-номер + название.
    /// Пример: "AS15169 Google LLC"
    /// </summary>
    [JsonPropertyName("org")]
    public string? Org { get; set; }

    /// <summary>
    /// Почтовый индекс.
    /// Пример: "94043"
    /// </summary>
    [JsonPropertyName("postal")]
    public string? Postal { get; set; }

    /// <summary>
    /// Часовой пояс (IANA).
    /// Пример: "America/Los_Angeles"
    /// </summary>
    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    /// <summary>
    /// true = приватный/loopback IP (не имеет публичной локации).
    /// Пример: true для 192.168.1.1
    /// </summary>
    [JsonPropertyName("bogon")]
    public bool? Bogon { get; set; }

    /// <summary>
    /// Широта (парсится из Loc).
    /// </summary>
    [JsonIgnore]
    public double? Latitude
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Loc)) return null;

            var parts = Loc.Split(',');
            if (parts.Length != 2) return null;

            return double.TryParse(
                parts[0],
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var lat)
                ? lat
                : null;
        }
    }

    /// <summary>
    /// Долгота (парсится из Loc).
    /// </summary>
    [JsonIgnore]
    public double? Longitude
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Loc)) return null;

            var parts = Loc.Split(',');
            if (parts.Length != 2) return null;

            return double.TryParse(
                parts[1],
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var lng)
                ? lng
                : null;
        }
    }

    /// <summary>
    /// Название провайдера (без AS-номера).
    /// "AS15169 Google LLC" → "Google LLC"
    /// </summary>
    [JsonIgnore]
    public string? Isp
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Org))
                return null;

            var spaceIndex = Org.IndexOf(' ');

            return spaceIndex > 0 && spaceIndex < Org.Length - 1
                ? Org[(spaceIndex + 1)..].Trim()
                : Org;
        }
    }

    /// <summary>
    /// AS-номер (Autonomous System).
    /// "AS15169 Google LLC" → "AS15169"
    /// </summary>
    [JsonIgnore]
    public string? Asn
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Org))
                return null;

            var spaceIndex = Org.IndexOf(' ');

            return spaceIndex > 0
                ? Org[..spaceIndex].Trim()
                : null;
        }
    }
}