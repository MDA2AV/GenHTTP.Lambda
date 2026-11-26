using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

/// <remarks>
/// Served for download, never as a page of the platform's own address: a
/// picture may be shown, everything else is saved.
/// </remarks>
[Summary("A file that travelled with a run: one the person attached, or one the agent handed back.")]
public sealed record RunFile(
    [Summary("What it is known by: `GET /attachments/{id}` answers with it")]
    string Id,
    [Summary("What it is called")]
    string Name,
    [Summary("What kind of file it is")]
    string Type,
    [Summary("How many bytes it holds")]
    long Size,
    [Summary("A line about it from the agent, for one it handed back")]
    string? Description = null
);
