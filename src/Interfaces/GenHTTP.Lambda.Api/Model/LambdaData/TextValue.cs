using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A text too long to send whole: its beginning, and how long it is.")]
public sealed record TextValue(
    [Summary("Its first 2000 characters")]
    string Text,
    [Summary("How many characters it has")]
    int Length
);
