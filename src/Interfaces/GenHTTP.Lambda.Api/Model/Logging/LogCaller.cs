using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

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
