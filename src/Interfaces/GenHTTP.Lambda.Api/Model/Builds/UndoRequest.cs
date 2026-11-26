using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Which message to undo.")]
public sealed record UndoRequest(
    [Summary("The `id` of the message, as `job.id` of `GET …/agent` names it; left out, the newest one")]
    string? Run = null
);
