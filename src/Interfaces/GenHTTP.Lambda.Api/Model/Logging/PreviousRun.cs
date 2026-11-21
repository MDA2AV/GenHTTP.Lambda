using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The end of the run of the server before this one, where it was not a clean one.")]
public sealed record PreviousRun(
    [Summary("When it started")]
    DateTime Started,
    [Summary("When it was last seen alive")]
    DateTime LastSeen,
    [Summary("How long it ran, in minutes")]
    double Minutes,
    [Summary("Whether anything asked it to stop. Nothing having asked points at the machine rather than at an operator or a deployment.")]
    bool Signalled,
    [Summary("The exception that ended it, where one did")]
    string? Fault,
    [Summary("Its working set when it was last seen, in bytes")]
    long WorkingSet,
    [Summary("How many requests it had answered")]
    long Requests,
    [Summary("How many connections were open when it was last seen")]
    int Sockets
);
