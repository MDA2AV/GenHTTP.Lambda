using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.SharedFiles;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The files shared across the installation.")]
public sealed record SharedFilesResponse(
    [Summary("The files, by name")]
    IReadOnlyList<SharedFileResponse> Files,
    [Summary("How large they are together, in bytes")]
    long Bytes,
    [Summary("What a shared file may be, and how many there may be")]
    SharedFileLimitsResponse Limits
)
{

    internal static SharedFilesResponse Of(IReadOnlyList<SharedFileUsage> files)
        => new([.. files.Select(SharedFileResponse.Of)], files.Sum(f => (long)f.File.Bytes), SharedFileLimitsResponse.Fixed);

}
