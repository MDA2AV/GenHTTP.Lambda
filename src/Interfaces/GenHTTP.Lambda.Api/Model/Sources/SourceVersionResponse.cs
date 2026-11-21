using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A version of a published source.")]
public sealed record SourceVersionResponse(
    [Summary("Its number")]
    int Version,
    [Summary("When it was saved")]
    DateTime Created,
    [Summary("What it changed, in a line")]
    string? Change,
    [Summary("How it was saved: `template`, `api`, `agent` or `git`")]
    string? Origin,
    [Summary("Whether it is the version online")]
    bool Online,
    [Summary("Where it is downloaded as a project")]
    string ZipPath
);
