using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The files of a published version, as the project it is packed into has them.")]
public sealed record SourceTreeResponse(
    [Summary("The public key of the lambda")]
    string PublicKey,
    [Summary("The number of the version")]
    int Version,
    [Summary("The folder the project is in, inside the zip")]
    string Root,
    [Summary("How large the zip is, in bytes")]
    long Bytes,
    [Summary("Every file, by its path below the project's folder")]
    IReadOnlyList<SourceFileResponse> Files,
    [Summary("Where the zip is downloaded")]
    string ZipPath
);
