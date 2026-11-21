using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A page of the server's log, and where to carry on from.")]
public sealed record LogResponse(
    [Summary("The lines, oldest first")]
    IReadOnlyList<LogEntry> Lines,
    [Summary("The newest sequence held; pass it as `since` to read what comes after")]
    long Cursor,
    [Summary("Lines that were dropped before the reader got to them, because the ring went round or more matched than were asked for")]
    int Missed,
    [Summary("How many lines the ring holds in all")]
    int Capacity,
    [Summary("How many lines were written since the server came up")]
    long Written,
    [Summary("Whether what lambdas print is kept")]
    bool Capturing,
    [Summary("Whether the address a request came from is recorded")]
    bool Addresses,
    [Summary("How the run before this one ended, where it did not end cleanly; absent on a first start and after a clean stop")]
    PreviousRun? Previous
);
