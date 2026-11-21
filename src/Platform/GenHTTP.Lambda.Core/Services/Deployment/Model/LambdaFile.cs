using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// One file of a lambda: part of its code - C# to compile, or any other file
/// kept with it - or one of its resources, which it reads and serves while it
/// runs.
/// </summary>
[Summary("One file of a lambda.")]
[Remarks("`lambda.cs` and the other `.cs` files are compiled, files below `resources/` are served and read while the lambda runs, and every other file is kept with the version.")]
public sealed record LambdaFile(
    [Summary("Its path, with forward slashes: `lambda.cs`, `Store.cs`, `resources/web/index.html`, `docs/product.md`. Case sensitive, and also what the compiler's messages name.")]
    string Name,
    [Summary("Its content: the text itself, or base64 where `encoding` says so")]
    [property: JsonConverter(typeof(LongStringConverter))] string Code,
    [Summary("`base64` for a file that is not text - a picture, a font; left out for text. C# files are always text.")]
    string? Encoding = null)
{

    /// <summary>Whether this is C# the lambda is compiled from.</summary>
    /// <remarks>
    /// Not serialised, along with Bytes and the other kinds. This record is
    /// the API's shape as well as the storage one, and all of these are
    /// worked out from what is already there - so sending them put a second,
    /// base64 copy of every file into every response that carried one,
    /// roughly doubling it, to say something the caller could see for itself
    /// from the name.
    /// </remarks>
    [JsonIgnore]
    public bool IsCompiled => LambdaSource.IsCompiled(Name);

    /// <summary>Whether this belongs to the code rather than to the resources, compiled or not.</summary>
    [JsonIgnore]
    public bool IsCode => LambdaSource.IsCode(Name);

    /// <summary>Whether this is a resource: shipped to be read and served while the lambda runs.</summary>
    [JsonIgnore]
    public bool IsResource => LambdaSource.IsResource(Name);

    /// <summary>The bytes of a file, whatever it was sent as.</summary>
    [JsonIgnore]
    public byte[] Bytes => Encoding == "base64"
                         ? Convert.FromBase64String(Code)
                         : System.Text.Encoding.UTF8.GetBytes(Code);

}
