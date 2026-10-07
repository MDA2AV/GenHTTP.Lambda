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
/// <param name="Live">The version that is online, with its note, if one is</param>
/// <param name="Latest">The newest version, which may be ahead of what is online</param>
/// <param name="Versions">How many versions are kept</param>
/// <param name="RecentProblems">The last few warnings and errors, newest first</param>
/// <param name="Documentation">What the documentation of the version the storage is about says, and which of its pages it has</param>
public sealed record LambdaSummaryResponse(
    LambdaResponse Lambda,
    VersionResponse? Live,
    VersionResponse? Latest,
    int Versions,
    ActivationResponse? Activation,
    TrafficSummary Traffic,
    IReadOnlyList<OwnerLogEntry> RecentProblems,
    StorageSummary Storage,
    SummaryLimits Limits,
    DocumentationSummary Documentation
);

/// <summary>
/// How much a lambda is being used, and how well it is answering.
/// </summary>
/// <remarks>
/// Counted in memory since the server last started, which is
/// <paramref name="Since"/>: a restart begins these again.
/// </remarks>
/// <param name="HourRequests">Requests in the last hour</param>
/// <param name="HourFailed">Of those, answered with a server error</param>
/// <param name="DayRequests">Requests in the last day</param>
/// <param name="DayFailed">Of those, answered with a server error</param>
/// <param name="DayRejected">Of those, answered with a client error, not found included</param>
/// <param name="AverageMillis">Mean time to answer over the last day</param>
/// <param name="Upgrades">Connections upgraded to a websocket in the last day</param>
/// <param name="Hourly">Requests per hour over the last day, oldest first, for a sparkline</param>
/// <param name="LastSeen">When the last request arrived</param>
public sealed record TrafficSummary(
    long HourRequests,
    long HourFailed,
    long DayRequests,
    long DayFailed,
    long DayRejected,
    double AverageMillis,
    long Upgrades,
    IReadOnlyList<int> Hourly,
    DateTime? LastSeen,
    DateTime Since
);

/// <summary>
/// What a lambda keeps, and how much of each allowance it spends.
/// </summary>
/// <param name="Version">The version the code figures are about: the one online, else the newest</param>
/// <param name="CodeFiles">Files of its code: the C# it is compiled from and whatever else it is kept with, never served</param>
/// <param name="CodeBytes">What those weigh, towards the build allowance</param>
/// <param name="ResourceFiles">Its resources, which it reads and serves as they are when the code asks</param>
/// <param name="ResourceBytes">What those weigh, decoded, towards the build allowance</param>
/// <param name="WorkspaceFiles">Files in the workspace - the lambda's data, which it writes at runtime and every version shares</param>
/// <param name="WorkspaceBytes">The room those take, towards the data allowance</param>
/// <param name="ServesResources">Whether the code of that version reaches for Resources to serve them</param>
/// <param name="ServesWorkspace">Whether it serves the workspace, which makes those files public</param>
/// <param name="WorkspaceEnabled">Whether the owner left the workspace switched on</param>
/// <param name="UsesWorkspace">Whether the code of that version uses the workspace at all, and so fails where it does once it is off</param>
/// <param name="SecretsEnabled">Whether the lambda has secrets switched on</param>
/// <param name="Secrets">How many secrets it keeps - by name only, anywhere but in the lambda</param>
/// <param name="MissingSecrets">
/// The secrets its code reads that have no value yet - which is what its owner
/// is asked to set, because until then the code that reads them fails
/// </param>
/// <param name="DatabaseEnabled">Whether the lambda has its database switched on</param>
/// <param name="DatabaseTables">How many tables its database holds</param>
/// <param name="DatabaseBytes">The room its database takes, towards the data allowance</param>
/// <param name="UsesDatabase">Whether the code of that version connects to the database, and so fails where it does once it is off</param>
public sealed record StorageSummary(
    int? Version,
    int CodeFiles,
    long CodeBytes,
    int ResourceFiles,
    long ResourceBytes,
    int WorkspaceFiles,
    long WorkspaceBytes,
    bool ServesResources,
    bool ServesWorkspace,
    bool WorkspaceEnabled,
    bool UsesWorkspace,
    bool SecretsEnabled = false,
    int Secrets = 0,
    List<string>? MissingSecrets = null,
    bool DatabaseEnabled = false,
    int DatabaseTables = 0,
    long DatabaseBytes = 0,
    bool UsesDatabase = false
);

/// <summary>
/// The allowances a lambda on this installation has, in its tier.
/// </summary>
/// <remarks>
/// Nothing counts files, only what they come to: a version, its code and its
/// resources together; and the data, its database and its workspace together.
/// </remarks>
/// <param name="BuildBytes">How large a version may be, its code and its resources together</param>
/// <param name="DataBytes">The room its data may take, its database and its workspace together</param>
/// <param name="Features">How many features it may have open at once</param>
public sealed record SummaryLimits(
    long BuildBytes,
    long DataBytes,
    int Versions,
    int DeploymentLifetimeHours,
    int RetentionDays,
    int Features
);

/// <summary>
/// One line of a lambda's log, as its owner sees it.
/// </summary>
/// <remarks>
/// Without the address of the visitor or the town it resolves to. The owner
/// wrote the code and may read what it said, but the people who called it
/// did not agree to be pointed at; the country and what the client called
/// itself are as far as this goes.
/// </remarks>
/// <param name="Source">Requests, stdout, stderr, or the part of the server that spoke</param>
/// <param name="Domain">The lambda's own domain the request was addressed to, absent for its path on the platform</param>
public sealed record OwnerLogEntry(long Seq, DateTime At, string Level, string Source, string Text, string? Detail,
                                   string? Country, string? Agent, int Repeats, string? Domain);

/// <summary>
/// A page of a lambda's log, and where to carry on from.
/// </summary>
/// <param name="Cursor">The newest sequence held; ask from here next time</param>
/// <param name="Missed">Lines that were dropped before this reader reached them</param>
/// <param name="Capturing">Whether what the lambda prints is being kept at all</param>
public sealed record OwnerLogResponse(IReadOnlyList<OwnerLogEntry> Lines, long Cursor, int Missed, bool Capturing);
