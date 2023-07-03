using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AggregatorTest.Json;

public class ObjectConverter : JsonConverter<object>
{
    public override object Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }
        if (reader.TokenType == JsonTokenType.Number)
        {
            {
                if (reader.TryGetInt32(out var i))
                    return i;
                if (reader.TryGetInt64(out var l))
                    return l;
                // BigInteger could be added here.
                // if (FloatFormat == FloatFormat.Decimal && reader.TryGetDecimal(out var m))
                // return m;
                // else if (FloatFormat == FloatFormat.Double && reader.TryGetDouble(out var d))
                // return d;
                // using var doc = JsonDocument.ParseValue(ref reader);
                // if (UnknownNumberFormat == UnknownNumberFormat.JsonElement)
                // return doc.RootElement.Clone();
                // throw new JsonException(
                // string.Format("Cannot parse number {0}", doc.RootElement.ToString())
                // );
            }
        }
        if (reader.TokenType == JsonTokenType.True)
        {
            return true;
        }

        if (reader.TokenType == JsonTokenType.False)
        {
            return false;
        }

        // Forward to the JsonElement converter
        var converter = options.GetConverter(typeof(JsonElement)) as JsonConverter<JsonElement>;
        if (converter != null)
        {
            return converter.Read(ref reader, typeToConvert, options);
        }

        throw new JsonException(string.Format("Unknown token {0}", reader.TokenType));
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        // var converter = options.GetConverter(typeof(JsonElement)) as JsonConverter<JsonElement>;
        // converter.Write(writer, value, options);
    }
}
