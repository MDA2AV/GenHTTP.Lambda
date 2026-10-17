using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Every file of a lambda, to be saved as its next version, and why it was written.")]
public sealed record VersionRequest(
    [Summary("Every file of the lambda, `lambda.cs` first")]
    IReadOnlyList<LambdaFile>? Files,
    [Summary("What the user wants from this version and why - their requirements, in their own words where possible. Optional, and cut past 4000 characters.")]
    string? Specification = null,
    [Summary("What this version changes, in one line for the owner. Optional, and cut past 500 characters.")]
    string? Change = null
);

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

[Summary("A version, without its files.")]
public sealed record VersionResponse(
    [Summary("Its number, counting from 1")]
    int Version,
    [Summary("When it was saved")]
    DateTime Created,
    [Summary("What the user wanted and why, where whoever saved it said")]
    string? Specification,
    [Summary("What it changed, where whoever saved it said")]
    string? Change,
    [Summary("How it was saved: `template` (what the lambda started as), `api`, `agent`, `git`, `admin` or `system`")]
    string? Origin
);

[Summary("A version that was just saved.")]
public sealed record SavedVersionResponse(
    [Summary("Its number")]
    int Version,
    [Summary("When it was saved")]
    DateTime Created,
    [Summary("What the user wanted and why, as it was kept")]
    string? Specification,
    [Summary("What it changes, as it was kept")]
    string? Change,
    [Summary("How it was saved")]
    string? Origin,
    [Summary("What came of putting it online, where that was asked for (`?deploy=true`); absent otherwise")]
    DeploymentOutcomeResponse? Deployment
);

[Summary("A version with its files.")]
public sealed record VersionContentResponse(
    [Summary("Its number")]
    int Version,
    [Summary("When it was saved")]
    DateTime Created,
    [Summary("What the user wanted and why, where whoever saved it said")]
    string? Specification,
    [Summary("What it changed, where whoever saved it said")]
    string? Change,
    [Summary("How it was saved: `template`, `api`, `agent`, `git`, `admin` or `system`")]
    string? Origin,
    [Summary("Its files, `lambda.cs` first - all of them, or those of the folder asked for")]
    IReadOnlyList<LambdaFile> Files
);
