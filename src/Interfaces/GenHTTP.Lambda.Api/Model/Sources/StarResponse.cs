using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("How a source is starred now.")]
public sealed record StarResponse(
    [Summary("How many stars it has now")]
    int Stars,
    [Summary("Whether this address stars it now")]
    bool Starred,
    [Summary("Whether this request changed the count - it does not where the same was done from here before")]
    bool Counted
);
