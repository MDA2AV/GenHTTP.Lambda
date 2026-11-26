using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

[Summary("What a run waits for while it is not worked on.")]
public sealed record RunWait(
    [Summary("`queue` (its turn at the agent, which works on others), `overloaded` (the service behind the agent is busy), `rate-limited` (a limit of the account it runs on), `preparing` (the agent makes room for it), `unreachable` (the agent does not answer, and is asked again) or `other`")]
    string Reason,
    [Summary("Its place in the queue, one being next, while it waits there")]
    int? Position = null,
    [Summary("When it expects to go on, where that is known")]
    DateTime? Until = null
);
