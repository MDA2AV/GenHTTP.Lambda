using System.Text.Json;
using System.Text.Json.Serialization;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// What a version - or a feature, or what its preview was deployed with -
/// holds, as it is written to disk: its files in their order, each with the
/// content it holds named by its SHA-256, how large it is, and whether it is
/// text.
/// </summary>
/// <remarks>
/// An entry per file, whatever the files weigh: the content is the store's,
/// once per lambda. Enough to describe a version without reading what it
/// holds - the length of a text file as well, which is what an agent is told
/// a file comes to.
///
/// <code>{"version":3,"files":[{"name":"lambda.cs","blob":"…","size":812,"length":812},{"name":"resources/logo.png","blob":"…","size":6400,"encoding":"base64"}]}</code>
///
/// Version 3 of what a lambda's files are written as: 1 and 2 were envelopes
/// holding every file whole, which are read as they were (see
/// <see cref="LambdaSource.Parse"/>) and moved into the store once as the
/// platform starts.
/// </remarks>
internal static class Manifest
{

    private const int Version = 3;

    private static readonly JsonSerializerOptions Format = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// A manifest of the given files, whose content the store keeps.
    /// </summary>
    public static byte[] Write(IReadOnlyList<VersionFile> files)
        => JsonSerializer.SerializeToUtf8Bytes(new Document(Version, [.. files.Select(f => new Entry(f.Name, f.Hash, f.Size, f.Binary ? null : f.Chars, f.Encoding))]), Format);

    /// <summary>
    /// The files a manifest names, in its order.
    /// </summary>
    /// <exception cref="InvalidDataException">It is no manifest of this version</exception>
    public static IReadOnlyList<Entry> Read(string text)
    {
        Document? document;

        try
        {
            document = JsonSerializer.Deserialize<Document>(text, Format);
        }
        catch (JsonException e)
        {
            throw new InvalidDataException("This is not a manifest of a version.", e);
        }

        if (document is not { Version: Version, Files: { Count: > 0 } files })
        {
            throw new InvalidDataException("This is not a manifest of a version.");
        }

        foreach (var file in files)
        {
            // it names a file below the store, so it is held to being a name
            if (string.IsNullOrEmpty(file.Name) || !IsHash(file.Blob) || file.Size < 0 || (file.Encoding is not (null or "base64")))
            {
                throw new InvalidDataException($"The manifest names '{file.Name}' in a way it cannot be read.");
            }
        }

        return files;
    }

    /// <summary>
    /// Whether a name is a SHA-256 in lowercase hex, which is all the store names content by.
    /// </summary>
    public static bool IsHash(string? value)
        => value is { Length: 64 } && value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');

    #region Types

    /// <param name="Name">The path of the file, as the lambda names it</param>
    /// <param name="Blob">The SHA-256 of its content</param>
    /// <param name="Size">How many bytes it holds</param>
    /// <param name="Length">Its length in characters, for a text file</param>
    /// <param name="Encoding"><c>base64</c> for a binary file, nothing for text</param>
    public sealed record Entry(string Name, string Blob, long Size, long? Length = null, string? Encoding = null);

    private sealed record Document(int Version, IReadOnlyList<Entry> Files);

    #endregion

}
