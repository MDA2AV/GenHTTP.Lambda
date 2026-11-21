using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Changes to make to some files, saved as the next version or into a feature.")]
public sealed record VersionChangeRequest(
    [Summary("Files to add, or to replace where one of that name exists")]
    IReadOnlyList<LambdaFile>? Files,
    [Summary("Names of files to remove. Each has to exist.")]
    IReadOnlyList<string>? Remove,
    [Summary("Replacements of text within files, applied after the files were added and removed")]
    IReadOnlyList<FileEdit>? Edits,
    [Summary("What the user wants from this change and why. Optional, and cut past 4000 characters.")]
    string? Specification = null,
    [Summary("What this change does, in one line for the owner. Optional, and cut past 500 characters.")]
    string? Change = null
);
