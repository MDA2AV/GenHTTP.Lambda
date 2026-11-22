using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A page of a lambda's log, and where to carry on from.")]
public sealed record OwnerLogResponse(
    [Summary("The lines, oldest first")]
    IReadOnlyList<OwnerLogEntry> Lines,
    [Summary("The newest sequence held; pass it as `since` to read what comes after")]
    long Cursor,
    [Summary("Lines that were dropped before this reader got to them, because the ring went round or more matched than were asked for")]
    int Missed,
    [Summary("Whether what lambdas print is kept at all on this installation")]
    bool Capturing
);
