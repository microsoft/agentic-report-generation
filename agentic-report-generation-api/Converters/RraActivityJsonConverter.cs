using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticReportGenerationApi.Converters
{
    public class RraActivityJsonConverter : JsonConverter<RraActivity>
    {
        public override RraActivity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var activity = new RraActivity();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return activity;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var propertyName = reader.GetString();
                reader.Read();

                if (propertyName.ToLower() == "category")
                {
                    activity.category = reader.GetString();
                }
                else
                {
                    var value = reader.TokenType switch
                    {
                        JsonTokenType.Number => reader.GetDecimal().ToString(),
                        JsonTokenType.String => reader.GetString(),
                        JsonTokenType.True => "true",
                        JsonTokenType.False => "false",
                        _ => JsonSerializer.Deserialize<object>(ref reader, options)?.ToString() ?? string.Empty
                    };

                    activity.DynamicFields[propertyName] = value;
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, RraActivity value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("category", value.category);

            foreach (var kvp in value.DynamicFields)
            {
                writer.WriteString(kvp.Key, kvp.Value?.ToString());
            }

            writer.WriteEndObject();
        }
    }
}