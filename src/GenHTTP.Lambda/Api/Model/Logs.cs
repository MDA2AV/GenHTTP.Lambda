using GenHTTP.Api.Content.Services;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A line of the server's log, as the operator sees it.")]
public sealed record LogEntry(
    [Summary("Its sequence: counts up and never repeats, so a reader asks for what came after the last one it saw")]
    long Seq,
    [Summary("When it was said")]
    DateTime At,
    [Summary("`trace`, `debug`, `info`, `warn`, `error` or `critical`")]
    string Level,
    [Summary("Who said it: `Requests` for a request line, `stdout` or `stderr` for what a lambda printed, or the part of the server that spoke")]
    string Source,
    [Summary("The public key of the lambda it belongs to; absent for the platform itself")]
    string? Lambda,
    [Summary("What was said")]
    string Text,
    [Summary("The stack trace, where there is one")]
    string? Detail,
    [Summary("The address of the caller, where addresses are recorded - with the address a proxy claimed for it, where one did")]
    string? Client,
    [Summary("What the caller said it was - its user agent")]
    string? Agent,
    [Summary("The two letter code of the country the caller's address is registered in, where it is known")]
    string? Country,
    [Summary("The place the caller's address is registered at, where places are looked up")]
    string? Place,
    [Summary("How many identical lines in a row this one stands for")]
    int Repeats,
    [Summary("The lambda's own host the request was addressed to; absent for the platform")]
    string? Domain
);

[Summary("A caller the log still holds something about.")]
public sealed record LogCaller(
    [Summary("Its address")]
    string Client,
    [Summary("The place its address is registered at, where known")]
    string? Place,
    [Summary("The two letter code of the country its address is registered in, where known")]
    string? Country,
    [Summary("What it said it was, last")]
    string? Agent,
    [Summary("How many lines of the log are about it")]
    long Lines,
    [Summary("How many of its requests were answered with an error")]
    long Failed,
    [Summary("When it was first seen in what the log holds")]
    DateTime First,
    [Summary("When it was last seen")]
    DateTime Last
);

[Summary("A page of the server's log, and where to carry on from.")]
public sealed record LogResponse(
    [Summary("The lines, oldest first")]
    IReadOnlyList<LogEntry> Lines,
    [Summary("The newest sequence held; pass it as `since` to read what comes after")]
    long Cursor,
    [Summary("Lines that were dropped before the reader got to them, because the ring went round or more matched than were asked for")]
    int Missed,
    [Summary("How many lines the ring holds in all")]
    int Capacity,
    [Summary("How many lines were written since the server came up")]
    long Written,
    [Summary("Whether what lambdas print is kept")]
    bool Capturing,
    [Summary("Whether the address a request came from is recorded")]
    bool Addresses,
    [Summary("How the run before this one ended, where it did not end cleanly; absent on a first start and after a clean stop")]
    PreviousRun? Previous
);

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
