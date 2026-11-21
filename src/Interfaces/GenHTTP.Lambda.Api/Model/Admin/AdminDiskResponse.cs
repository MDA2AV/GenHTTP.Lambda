using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The volume the data directory is on.")]
public sealed record AdminDiskResponse(
    [Summary("How large it is, in bytes")]
    long Size,
    [Summary("How much of it is free for the platform, in bytes")]
    long Free
);
