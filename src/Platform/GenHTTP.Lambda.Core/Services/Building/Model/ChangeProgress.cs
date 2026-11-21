using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Building;

[Summary("A change the agent was asked for, and how it is getting on.")]
public sealed record ChangeProgress(
    [Summary("What the agent knows it by")]
    string Id,
    [Summary("`queued`, `running`, `done`, `failed` or `cancelled`")]
    string State,
    [Summary("What was asked for")]
    string Prompt,
    [Summary("Whether it was asked to merge the change and put it online")]
    bool Deploy,
    [Summary("Which of the offered models is making it, by its name")]
    string Model,
    [Summary("The version that was online when it was asked for")]
    int? Before,
    [Summary("What it has done so far")]
    IReadOnlyList<AgentStep> Steps,
    [Summary("How it ended, once it has")]
    ChangeResult? Result,
    [Summary("Its place in the queue; zero once it runs")]
    int Waiting,
    [Summary("How long it has been running, in seconds")]
    int Seconds,
    [Summary("How long it may run, in seconds; absent when there is no clock on it")]
    int? Limit,
    [Summary("The key of the feature it was asked to go on with; absent when it starts one of its own")]
    string? Feature = null,
    [Summary("The effort it is made at, by its name")]
    string? Effort = null
);
