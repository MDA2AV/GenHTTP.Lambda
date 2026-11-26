using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

/// <remarks>
/// What a page offers beside the box: the options to choose from, whether a
/// file may be attached and which, and what the agent does - so nothing is
/// offered or promised the installation's agent does not do. Never the
/// password, only whether a value needs it.
///
/// <c>models</c>, <c>model</c>, <c>efforts</c> and <c>effort</c> are the
/// options <c>model</c> and <c>effort</c> as pages read them before the
/// options were the integration's; another integration may have neither.
/// </remarks>
[Summary("What the installation's agent may be asked for on, as its operator offers it, and what it does.")]
public sealed record AgentOffer(
    [Summary("The options a run may be asked for on, such as the model - the agent integration's own, in the order to list them")]
    IReadOnlyList<OfferedOption> Options,
    [Summary("The values of the option `model`, where there is one, in the order to list them")]
    IReadOnlyList<OfferedValue> Models,
    [Summary("The default of the option `model`, where there is one")]
    string? Model,
    [Summary("The values of the option `effort`, where there is one, the least first")]
    IReadOnlyList<string> Efforts,
    [Summary("The default of the option `effort`, where there is one")]
    string? Effort,
    [Summary("The files a message may come with; absent where the agent takes none, and no file is to be offered")]
    AttachmentPolicy? Attachments = null,
    [Summary("Whether the agent may hand files back to the person")]
    bool ReturnsFiles = false,
    [Summary("Whose web pages the agent reads: `none`, `named` (the sites the person names) or `any`")]
    string Web = "none",
    [Summary("Whether the agent searches the web")]
    bool Searches = false,
    [Summary("Whether a run can be stopped")]
    bool Cancels = true,
    [Summary("Whether a run may end with a question, which the next message answers")]
    bool Asks = false
);
