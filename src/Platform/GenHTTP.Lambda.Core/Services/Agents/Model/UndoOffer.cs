using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

[Summary("What undoing a run would do, and what it would leave.")]
public sealed record UndoOffer(
    [Summary("Whether there is anything to undo")]
    bool Possible,
    [Summary("What would be undone: `online` (the version online before goes back online), `draft` (a draft it started is discarded), `connectivity` (its request is withdrawn), `source` (the source it published is taken down), `showcase` (it leaves the showcase), `view` (the editor opens as before)")]
    IReadOnlyList<string> Undoes,
    [Summary("What stays as it is, and cannot be undone here: `versions` (they stay in the history), `lambda` (a new app), `data` (a kind of data switched on - switching it off deletes it), `analytics` (switching it off deletes what was counted), `workspace` (files it put into what the app saved), `draft` (what it changed in a draft it did not start), `undone` (it was undone already)")]
    IReadOnlyList<string> Keeps
);
