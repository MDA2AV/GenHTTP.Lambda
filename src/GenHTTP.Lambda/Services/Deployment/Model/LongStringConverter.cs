using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// Writes a string however long it is.
/// </summary>
/// <remarks>
/// System.Text.Json refuses to write a single value of more than 166,666,666
/// characters, which in base64 is a file of a little under 125 MB - and a
/// premium lambda may keep files larger than that, as resources and in its
/// workspace. Written in pieces there is no such limit. Reading never had one.
/// </remarks>
public sealed class LongStringConverter : JsonConverter<string>
{
    private const int Piece = 1024 * 1024;

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetString();

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        if (value.Length <= Piece)
        {
            writer.WriteStringValue(value);
            return;
        }

        var rest = value.AsSpan();

        while (rest.Length > Piece)
        {
            // never between the two halves of a surrogate pair
            var length = char.IsHighSurrogate(rest[Piece - 1]) ? Piece - 1 : Piece;

            writer.WriteStringValueSegment(rest[..length], false);

            rest = rest[length..];
        }

        writer.WriteStringValueSegment(rest, true);
    }

}
