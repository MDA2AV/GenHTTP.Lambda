using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Something a new lambda can be started from.")]
public sealed record StarterResponse(
    [Summary("What to pass as `template` when creating the lambda")]
    string Id,
    [Summary("What somebody would want to build, in their words")]
    string Title,
    [Summary("What they would get")]
    string Description,
    [Summary("Where the demo it copies answers, to look at first; absent for the empty lambda")]
    string? Demo
);
