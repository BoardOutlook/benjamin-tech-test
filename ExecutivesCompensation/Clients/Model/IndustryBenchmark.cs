using System.Text.Json.Serialization;

namespace ExecutivesCompensation.Clients.Model;

public class IndustryBenchmark
{
    [JsonPropertyName("industryTitle")]
    public string? IndustryTitle { get; set; }

    [JsonPropertyName("averageCompensation")]
    public double AverageCompensation { get; set; }
}
