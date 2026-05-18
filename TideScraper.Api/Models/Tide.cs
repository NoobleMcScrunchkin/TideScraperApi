using System.Text.Json.Serialization;

namespace TideScraper.Api.Models;

public class Tide
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TideType TideType { get; set; }

    public DateTime Time { get; set; }

    public decimal Height { get; set; }
}