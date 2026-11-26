using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

[Summary("What undoing a run did, and what it left.")]
public sealed record UndoOutcome(
    [Summary("What was undone, as `undo.undoes` of the run names it")]
    IReadOnlyList<string> Undone,
    [Summary("What stays as it is, as `undo.keeps` of the run names it - with what could not be undone after all because it changed since, as `changed:online` and the like")]
    IReadOnlyList<string> Kept,
    [Summary("The version online afterwards")]
    int? Online
);
