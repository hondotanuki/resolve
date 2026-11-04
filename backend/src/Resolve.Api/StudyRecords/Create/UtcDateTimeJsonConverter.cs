using System.Text.Json;
using System.Text.Json.Serialization;

namespace Resolve.Api.StudyRecords.Create;

public sealed class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("日時にはタイムゾーンを指定してください。");
        }

        var value = reader.GetString();
        var hasOffset =
            value is not null
            && (
                value.EndsWith('Z')
                || (value.Length >= 6 && (value[^6] == '+' || value[^6] == '-') && value[^3] == ':')
            );

        if (!hasOffset || !reader.TryGetDateTimeOffset(out var dateTime))
        {
            throw new JsonException("日時にはUTCまたはオフセットを指定してください。");
        }

        return dateTime.UtcDateTime;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
