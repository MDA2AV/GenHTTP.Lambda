using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

[Summary("How a build is getting on.")]
public sealed record BuildProgress(
    [Summary("`queued`, `running`, `done` or `failed`")]
    string State,
    [Summary("What it has done so far, as facts a page can say in the visitor's language - the same steps the editor shows of a change")]
    IReadOnlyList<AgentStep> Steps,
    [Summary("How it ended, once it has")]
    BuildResult? Result,
    [Summary("Its place in the queue; zero once it runs")]
    int Waiting,
    [Summary("How long it has been running, in seconds")]
    int Seconds = 0,
    [Summary("How long it may run, in seconds; absent when there is no clock on it")]
    int? Limit = null,
    [Summary("What it is known by - the id it was started as")]
    string? Id = null,
    [Summary("What it waits for while it is not worked on; absent while it works or once it ended")]
    RunWait? Wait = null,
    [Summary("The value of each of the agent's options it runs on, by option")]
    IReadOnlyDictionary<string, string>? Options = null,
    [Summary("The files it was asked with")]
    IReadOnlyList<RunFile>? Attachments = null,
    [Summary("The files the agent handed back so far")]
    IReadOnlyList<RunFile>? Files = null
);
