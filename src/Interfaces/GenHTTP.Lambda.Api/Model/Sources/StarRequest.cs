using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A star given, or taken back.")]
public sealed record StarRequest(
    [Summary("The `starTicket` the source was read with")]
    string? Ticket,
    [Summary("`true`, or left out, to star it; `false` to take the star back")]
    bool? Starred
);
