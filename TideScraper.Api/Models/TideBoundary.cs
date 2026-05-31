using System.Text.Json.Serialization;

namespace TideScraper.Api.Models;

public class TideBoundary : Tide
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TideType TideType { get; set; }
}