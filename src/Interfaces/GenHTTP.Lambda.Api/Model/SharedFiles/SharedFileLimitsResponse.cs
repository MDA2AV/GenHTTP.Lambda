using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.SharedFiles;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What a shared file may be, and how many there may be.")]
public sealed record SharedFileLimitsResponse(
    [Summary("How many files there may be")]
    int Files,
    [Summary("How large one may be, in bytes")]
    int Bytes,
    [Summary("How large they may be together, in bytes")]
    int TotalBytes,
    [Summary("How many characters a name may have")]
    int Name,
    [Summary("What a name may end in, by what the file is for: `picture`, `font`, `style`, `text`")]
    IReadOnlyDictionary<string, IReadOnlyList<string>> Types
)
{

    internal static readonly SharedFileLimitsResponse Fixed = new(SharedFileChecks.MaxFiles, SharedFileChecks.MaxBytes, SharedFileChecks.MaxTotalBytes, SharedFileChecks.MaxName,
        SharedFileType.All.GroupBy(t => SharedFileKindNames.Name(t.Value.Kind)).ToDictionary(g => g.Key, g => (IReadOnlyList<string>)[.. g.Select(t => t.Key)]));

}
