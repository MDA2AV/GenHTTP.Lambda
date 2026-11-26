using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

[Summary("One value of an option of the agent's that may be chosen.")]
public sealed record OfferedValue(
    [Summary("What to ask for it by, as the agent integration names it - `opus`, `high`")]
    string Name,
    [Summary("What to call it, with a version where it has one - `Opus 5.5`")]
    string Label,
    [Summary("Whether it needs the installation's password - and, with it, runs on the limits the operator set for such runs, none by default")]
    bool Password
);
