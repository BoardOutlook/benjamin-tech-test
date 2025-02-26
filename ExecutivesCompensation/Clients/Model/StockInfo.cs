using System.Text.Json.Serialization;

namespace ExecutivesCompensation.Clients.Model;

public class StockInfo
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("exchangeShortName")]
    public string? ExchangeShortName { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
