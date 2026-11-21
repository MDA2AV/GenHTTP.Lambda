using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Building;

/// <remarks>
/// What a page offers in its two lists, and what it starts on. Never which
/// identifier a model runs as, which is the runner's, nor the password,
/// only whether a model needs it.
/// </remarks>
[Summary("The models and efforts the build agent may be asked for, as the operator of the installation offers them.")]
public sealed record AgentOffer(
    [Summary("The models on offer, in the order to list them")]
    IReadOnlyList<OfferedModel> Models,
    [Summary("The model a job runs on where none is asked for, by its name: offered to everybody")]
    string Model,
    [Summary("The efforts on offer, the least first: `low`, `medium`, `high`, `xhigh` or `max`")]
    IReadOnlyList<string> Efforts,
    [Summary("The effort a job runs at where none is asked for")]
    string Effort
);
