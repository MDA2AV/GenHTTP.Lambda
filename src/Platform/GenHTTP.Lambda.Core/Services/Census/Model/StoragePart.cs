using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Census;

/// <summary>
/// A part of what the data directory holds.
/// </summary>
[Summary("A part of what the data directory holds.")]
public sealed record StoragePart(
    [Summary("Which part: `versions`, `drafts`, `databases`, `workspaces`, `git`, `sources`, `compiled`, `resources`, `platform`, `analytics`, `locations`, `crawlers` or `other`")]
    string Kind,
    [Summary("What its files come to, in bytes")]
    long Bytes,
    [Summary("How many files it is")]
    long Files
);
