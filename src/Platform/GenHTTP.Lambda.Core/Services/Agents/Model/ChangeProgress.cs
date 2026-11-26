using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

[Summary("A change the agent was asked for, and how it is getting on.")]
public sealed record ChangeProgress(
    [Summary("What the agent knows it by")]
    string Id,
    [Summary("`queued`, `running`, `done`, `failed` or `cancelled`; a run that asked back is `done`, with `result.asked`")]
    string State,
    [Summary("What was asked for")]
    string Prompt,
    [Summary("Whether it was asked to merge the change and put it online")]
    bool Deploy,
    [Summary("The value of the option `model` it is made on, where the agent has one")]
    string? Model,
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
    [Summary("The value of the option `effort` it is made at, where the agent has one")]
    string? Effort = null,
    [Summary("The value of each of the agent's options it is made on, by option")]
    IReadOnlyDictionary<string, string>? Options = null,
    [Summary("What it waits for while it is not worked on; absent while it works or once it ended")]
    RunWait? Wait = null,
    [Summary("The files it was asked with")]
    IReadOnlyList<RunFile>? Attachments = null,
    [Summary("The files the agent handed back so far")]
    IReadOnlyList<RunFile>? Files = null,
    [Summary("The run it carries on from - the one whose question it answers, or whose wait is over - where it does")]
    string? Continues = null,
    [Summary("When it was asked for")]
    DateTime? Created = null
);
