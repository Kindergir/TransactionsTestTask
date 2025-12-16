using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JFF.DTO;

internal class LoadAmountCurrencyConverter : JsonConverter<LoadAmount>
{
    public override LoadAmount Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = reader.GetString()?.Trim() ?? "";

        if (string.IsNullOrEmpty(raw))
            throw new ArgumentException("Invalid input string");

        Currency currency = raw[0] switch
        {
            '$' => Currency.USD,
            _ => Currency.Unknown
        };

        var value = decimal.TryParse(raw.AsSpan(1), CultureInfo.InvariantCulture, out var result);
        if (!value || currency == Currency.Unknown)
            throw new ArgumentException("Invalid amount or currency");

        return new LoadAmount(result, currency);
    }

    public override void Write(Utf8JsonWriter writer, LoadAmount value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}