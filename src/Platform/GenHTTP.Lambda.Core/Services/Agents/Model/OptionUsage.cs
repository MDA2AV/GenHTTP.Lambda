using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

[Summary("How one option of the agent's was chosen.")]
public sealed record OptionUsage(
    [Summary("The option, by name")]
    string Option,
    [Summary("Each value chosen, with its runs and their time")]
    IReadOnlyList<ValueUsage> Values
);
