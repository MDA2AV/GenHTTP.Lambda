using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A value of bytes, told by its length.")]
public sealed record BlobValue(
    [Summary("How many bytes it holds")]
    long Blob
);
