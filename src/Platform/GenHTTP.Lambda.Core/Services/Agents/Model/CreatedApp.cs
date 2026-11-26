using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

/// <remarks>
/// Handed to whoever asked the message - the owner of the lambda it was
/// about - as a build hands over the lambda it made: nobody else holds the
/// editor key of what the agent made for them.
/// </remarks>
[Summary("A new lambda a message created beside the one it was about.")]
public sealed record CreatedApp(
    [Summary("Its public key")]
    string PublicKey,
    [Summary("Its editor key - handed out here, to the owner who asked, and nowhere else")]
    string PrivateKey,
    [Summary("Where it answers")]
    string? Url,
    [Summary("Where its editor is")]
    string? EditorUrl
);
