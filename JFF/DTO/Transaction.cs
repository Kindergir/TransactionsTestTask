using System.Text.Json.Serialization;

namespace JFF.DTO;

public record Transaction
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("customer_id")]
    public string CustomerId { get; set; }

    [JsonPropertyName("load_amount")]
    [JsonConverter(typeof(LoadAmountCurrencyConverter))]
    public LoadAmount Amount { get; set; }

    [JsonPropertyName("time")]
    public DateTimeOffset Timestamp { get; set; }
}