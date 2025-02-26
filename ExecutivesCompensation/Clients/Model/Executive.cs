using System.Text.Json.Serialization;

namespace ExecutivesCompensation.Clients.Model;

public class Executive
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("industryTitle")]
    public string? IndustryTitle { get; set; }

    [JsonPropertyName("nameAndPosition")]
    public string? NameAndPosition { get; set; }

    [JsonPropertyName("total")]
    public double Total { get; set; }
}
