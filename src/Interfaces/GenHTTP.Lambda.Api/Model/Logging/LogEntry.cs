using GenHTTP.Api.Content.Documentation;

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
