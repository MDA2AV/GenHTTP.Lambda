using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// Everything worth knowing about a lambda at a glance, in one answer.
/// </summary>
/// <remarks>
/// What the control center opens on, and what an agent can ask for to see
/// whether what it deployed is actually working. One document rather than
/// five because the question it answers - is this all right? - is one
/// question.
/// </remarks>
[Summary("Everything worth knowing about a lambda at a glance.")]
public sealed record LambdaSummaryResponse(
    [Summary("The lambda")]
    LambdaResponse Lambda,
    [Summary("The version that is online, if one is")]
    VersionResponse? Live,
    [Summary("The newest version, which may be ahead of the one online")]
    VersionResponse? Latest,
    [Summary("How many versions are kept")]
    int Versions,
    [Summary("Since when the version online has been online; absent while none is")]
    ActivationResponse? Activation,
    [Summary("How much it is used, and how well it answers")]
    TrafficSummary Traffic,
    [Summary("The last few errors and warnings of its own log, newest first - not the requests that were merely refused")]
    IReadOnlyList<OwnerLogEntry> RecentProblems,
    [Summary("What it keeps, and how much of its allowances that takes")]
    StorageSummary Storage,
    [Summary("What its tier allows it")]
    SummaryLimits Limits,
    [Summary("What the documentation of the version the storage is about says, and which of its pages it has")]
    DocumentationSummary Documentation
);

/// <remarks>
/// Counted in memory since the server last started, which is
/// <paramref name="Since"/>: a restart begins these again.
/// </remarks>
[Summary("How much a lambda is used, and how well it answers.")]
public sealed record TrafficSummary(
    [Summary("Requests in the last hour")]
    long HourRequests,
    [Summary("Of those, answered with a server error")]
    long HourFailed,
    [Summary("Requests in the last day")]
    long DayRequests,
    [Summary("Of those, answered with a server error")]
    long DayFailed,
    [Summary("Of those, answered with a client error, not found included")]
    long DayRejected,
    [Summary("The mean time to answer over the last day, in milliseconds")]
    double AverageMillis,
    [Summary("Connections upgraded to a websocket in the last day")]
    long Upgrades,
    [Summary("Requests per hour over the last day, oldest first")]
    IReadOnlyList<int> Hourly,
    [Summary("When the last request arrived")]
    DateTime? LastSeen,
    [Summary("When the counting started: when the server came up")]
    DateTime Since
);

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

/// <remarks>
/// Nothing counts files, only what they come to: a version, its code and its
/// resources together; and the data, its database and its workspace together.
/// </remarks>
[Summary("What a lambda of its tier may have on this installation, as the operator set it.")]
public sealed record SummaryLimits(
    [Summary("What a version may come to, its code and its resources together, in bytes")]
    long BuildBytes,
    [Summary("The room its data may take, its database and its workspace together, in bytes")]
    long DataBytes,
    [Summary("How many versions are kept; the oldest beyond that go with every save, but never the one online or one a feature is based on")]
    int Versions,
    [Summary("How long a free lambda may go without a visit or an edit before it is taken offline, in hours")]
    int DeploymentLifetimeHours,
    [Summary("How long a free lambda may go without a visit or an edit before it is deleted, in days")]
    int RetentionDays,
    [Summary("How many features it may have open at once")]
    int Features
);

/// <remarks>
/// Without the address of the visitor or the town it resolves to. The owner
/// wrote the code and may read what it said, but the people who called it
/// did not agree to be pointed at; the country and what the client called
/// itself are as far as this goes.
/// </remarks>
[Summary("A line of a lambda's log, as its owner sees it - never with a visitor's address.")]
public sealed record OwnerLogEntry(
    [Summary("Its sequence: counts up and never repeats, so a reader asks for what came after the last one it saw")]
    long Seq,
    [Summary("When it was said")]
    DateTime At,
    [Summary("`trace`, `debug`, `info`, `warn`, `error` or `critical`")]
    string Level,
    [Summary("Who said it: `Requests` for a request line, `stdout` or `stderr` for what the code printed, or the part of the server that spoke")]
    string Source,
    [Summary("What was said")]
    string Text,
    [Summary("The stack trace, where there is one")]
    string? Detail,
    [Summary("The two letter code of the country the caller's address is registered in, where it is known")]
    string? Country,
    [Summary("What the caller said it was - its user agent")]
    string? Agent,
    [Summary("How many identical lines in a row this one stands for; one is itself alone")]
    int Repeats,
    [Summary("The host the request was addressed to: the lambda's subdomain of the hosting domain, or its own domain")]
    string? Domain
);

[Summary("A page of a lambda's log, and where to carry on from.")]
public sealed record OwnerLogResponse(
    [Summary("The lines, oldest first")]
    IReadOnlyList<OwnerLogEntry> Lines,
    [Summary("The newest sequence held; pass it as `since` to read what comes after")]
    long Cursor,
    [Summary("Lines that were dropped before this reader got to them, because the ring went round or more matched than were asked for")]
    int Missed,
    [Summary("Whether what lambdas print is kept at all on this installation")]
    bool Capturing
);
