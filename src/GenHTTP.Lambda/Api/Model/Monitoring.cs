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
public sealed record LambdaSummaryResponse(
    LambdaResponse Lambda,
    VersionResponse? Live,
    VersionResponse? Latest,
    int Versions,
    ActivationResponse? Activation,
    TrafficSummary Traffic,
    IReadOnlyList<OwnerLogEntry> RecentProblems,
    StorageSummary Storage,
    SummaryLimits Limits
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
/// <param name="CodeFiles">C# files, which are compiled and never served</param>
/// <param name="CodeCharacters">Characters of C#, which is what the code budget counts</param>
/// <param name="Assets">Files saved with the code that are not C#, served as they are when the code asks</param>
/// <param name="AssetBytes">What those weigh, decoded</param>
/// <param name="WorkspaceFiles">Files in the workspace, which the lambda writes at runtime</param>
/// <param name="WorkspaceBytes">What those weigh</param>
/// <param name="ServesAssets">Whether the code of that version reaches for Assets to serve them</param>
/// <param name="ServesWorkspace">Whether it serves the workspace, which makes those files public</param>
public sealed record StorageSummary(
    int? Version,
    int CodeFiles,
    int CodeCharacters,
    int Assets,
    long AssetBytes,
    int WorkspaceFiles,
    long WorkspaceBytes,
    bool ServesAssets,
    bool ServesWorkspace
);

/// <summary>
/// The allowances a lambda on this installation has.
/// </summary>
public sealed record SummaryLimits(
    int CodeCharacters,
    int CodeFiles,
    long AssetBytes,
    int Assets,
    long WorkspaceBytes,
    int WorkspaceFiles,
    int WorkspaceFileBytes,
    int Versions,
    int DeploymentLifetimeHours,
    int RetentionDays
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
