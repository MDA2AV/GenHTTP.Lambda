using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Source;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What a lambda's source is to be published under. What is left out stays as it is.")]
public sealed record SourceRequest(
    [Summary("The SPDX identifier of one of the licenses on offer; `MIT` when a source is first published without one")]
    string? License,
    [Summary("Who holds the copyright, as the license is to name them, on one line; empty names nobody")]
    string? Author
);
