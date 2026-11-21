using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A published source with its history.")]
public sealed record SourceProjectResponse(
    [Summary("The source, as the listing shows it")]
    SourceEntryResponse Source,
    [Summary("Who holds the copyright, where the owner named somebody")]
    string? Author,
    [Summary("Who the license names as holding it: the author, or the authors of the lambda")]
    string Holder,
    [Summary("The version online, if one is")]
    int? ActiveVersion,
    [Summary("When the lambda was created")]
    DateTime Created,
    [Summary("Every version kept, newest first: what each changed, never what was asked for")]
    IReadOnlyList<SourceVersionResponse> Versions,
    [Summary("What starring it is sent with (`POST …/star`): good from a second after it was handed out, for a day")]
    string StarTicket,
    [Summary("Where it is cloned with git on this site - every version, read only")]
    string GitPath
);
