using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

/// <remarks>
/// What the agent integration says of itself, for the operator's privacy
/// statement and for deciding what to offer.
/// </remarks>
[Summary("What the agent of the installation is, where it sends what, and what it does.")]
public sealed record AgentDescriptionResponse(
    [Summary("What the agent is")]
    string Agent,
    [Summary("Who provides the model or service behind it, where somebody does")]
    string? Vendor,
    [Summary("Everywhere a run sends something of what it is given or reads")]
    IReadOnlyList<AgentDestinationResponse> Destinations,
    [Summary("Whose web pages it reads: `none`, `named` (the sites the person names) or `any`")]
    string Web,
    [Summary("Whether it searches the web")]
    bool Searches,
    [Summary("How it reads them, where it reads any")]
    string? WebPolicy,
    [Summary("The kinds of file a message may come with; absent where it takes none")]
    IReadOnlyList<string>? Attachments,
    [Summary("Whether it may hand files back")]
    bool ReturnsFiles,
    [Summary("Whether a run may end with a question")]
    bool Asks,
    [Summary("Whether a run can be stopped")]
    bool Cancels
);
