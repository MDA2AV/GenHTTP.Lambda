using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

/// <remarks>
/// Without the address of the visitor or the town it resolves to. The owner
/// wrote the code and may read what it said, but the people who called it
/// did not agree to be pointed at; the country and what the client called
/// itself are as far as this goes.
/// </remarks>
[Summary("A line of a lambda's log, as its owner sees it - never with a visitor's address.")]
public sealed record OwnerLogEntry(
    [Summary("Its sequence: counts up and never repeats, so a reader asks for what came after the last one it saw")]
    long Seq,
    [Summary("When it was said")]
    DateTime At,
    [Summary("`trace`, `debug`, `info`, `warn`, `error` or `critical`")]
    string Level,
    [Summary("Who said it: `Requests` for a request line, `stdout` or `stderr` for what the code printed, or the part of the server that spoke")]
    string Source,
    [Summary("What was said")]
    string Text,
    [Summary("The stack trace, where there is one")]
    string? Detail,
    [Summary("The two letter code of the country the caller's address is registered in, where it is known")]
    string? Country,
    [Summary("What the caller said it was - its user agent")]
    string? Agent,
    [Summary("How many identical lines in a row this one stands for; one is itself alone")]
    int Repeats,
    [Summary("The host the request was addressed to: the lambda's own address on the platform, or its own domain")]
    string? Domain
);
