using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Building;

[Summary("One model the build agent may be asked for.")]
public sealed record OfferedModel(
    [Summary("What to ask for it by: `opus`, `sonnet`, `haiku` or `fable`")]
    string Name,
    [Summary("What to call it, with its version: `Opus 5.5`")]
    string Label,
    [Summary("Whether it needs the installation's password - and, with it, runs without a clock and without a limit on its steps")]
    bool Password
);
