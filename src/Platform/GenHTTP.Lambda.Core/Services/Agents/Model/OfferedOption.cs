using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

/// <remarks>
/// The options are the agent integration's, so another integration brings
/// others; the platform says what the operator offers of them and never
/// what they mean.
/// </remarks>
[Summary("Something of the agent's a run may be asked for on, such as the model, with the values offered.")]
public sealed record OfferedOption(
    [Summary("What it is called, as the agent integration names it - `model`, `effort`")]
    string Name,
    [Summary("What the pages call it, in English")]
    string Label,
    [Summary("The values that may be chosen, in the order to list them")]
    IReadOnlyList<OfferedValue> Values,
    [Summary("The value a run takes where none is asked for: offered to everybody")]
    string Default
);
