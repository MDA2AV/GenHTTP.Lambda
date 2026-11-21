using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What a lambda keeps, and how much of each allowance it takes.")]
public sealed record StorageSummary(
    [Summary("The version the figures about code are about: the one online, else the newest")]
    int? Version,
    [Summary("Files of its code: the C# it is compiled from and whatever else it is kept with, never served")]
    int CodeFiles,
    [Summary("What those weigh, towards what a version may come to")]
    long CodeBytes,
    [Summary("Its resources, which it reads and serves while it runs")]
    int ResourceFiles,
    [Summary("What those weigh, decoded, towards what a version may come to")]
    long ResourceBytes,
    [Summary("Files in the workspace, which it writes while it runs and every version shares")]
    int WorkspaceFiles,
    [Summary("The room those take, towards the data allowance")]
    long WorkspaceBytes,
    [Summary("Whether the code of that version serves its resources")]
    bool ServesResources,
    [Summary("Whether it serves the workspace, which makes those files public")]
    bool ServesWorkspace,
    [Summary("Whether the workspace is switched on")]
    bool WorkspaceEnabled,
    [Summary("Whether the code of that version uses the workspace at all, and so fails where it does once it is switched off")]
    bool UsesWorkspace,
    [Summary("Whether secrets are switched on")]
    bool SecretsEnabled = false,
    [Summary("How many secrets it keeps")]
    int Secrets = 0,
    [Summary("The secrets its code reads that have no value yet - what its owner still has to enter, because until then the code that reads them fails")]
    List<string>? MissingSecrets = null,
    [Summary("Whether the database is switched on")]
    bool DatabaseEnabled = false,
    [Summary("How many tables the database holds")]
    int DatabaseTables = 0,
    [Summary("The room the database takes, towards the data allowance")]
    long DatabaseBytes = 0,
    [Summary("Whether the code of that version connects to the database, and so fails where it does once it is switched off")]
    bool UsesDatabase = false
);
