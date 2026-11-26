using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

[Summary("The files a message to the agent may come with.")]
public sealed record AttachmentPolicy(
    [Summary("The kinds of file, as media types or ranges of them: `image/png`, `image/*`, `text/*`")]
    IReadOnlyList<string> Types,
    [Summary("The most one file may take, in bytes")]
    long MaxBytes,
    [Summary("The most files one message may come with")]
    int MaxCount
);
