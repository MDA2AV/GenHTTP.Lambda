using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Census;

/// <summary>
/// The lambdas of the installation, counted - what the operator's dashboards
/// are drawn from.
/// </summary>
/// <remarks>
/// The demos are counted apart (<paramref name="Demos"/>) and in nothing
/// else: every installation has the same ones, and their databases and
/// secrets would make each feature look used.
/// </remarks>
/// <param name="Taken">When it was counted</param>
/// <param name="Lambdas">How many there are, the demos left out</param>
/// <param name="Demos">How many demos the installation keeps</param>
/// <param name="Online">Of the lambdas, how many have a version online</param>
/// <param name="Offline">How many have none and are not blocked</param>
/// <param name="Blocked">How many the operator blocked</param>
/// <param name="Tiers">How many are in each tier, the demos' included</param>
/// <param name="CreatedDay">How many were created in the last day</param>
/// <param name="CreatedWeek">… in the last seven days</param>
/// <param name="CreatedMonth">… in the last thirty days</param>
/// <param name="ActiveMonth">How many were changed or visited in the last thirty days</param>
/// <param name="GoingOffline">How many go offline within <c>LambdaQuery.ExpiringWithin</c> unless they are used</param>
/// <param name="Removed">How many are removed within that time unless they are used</param>
/// <param name="Uses">How many use each of the platform's features</param>
/// <param name="Simple">How many open in the simple view, for an owner who does not write code</param>
/// <param name="Full">How many open in the full view</param>
/// <param name="Versions">How many versions are kept, all together</param>
/// <param name="Saved">Versions saved in the last thirty days, by where they came from</param>
/// <param name="RequestedSince">Since when the lambda that asks to connect to other servers the longest waits, or nothing while none waits</param>
public sealed record LambdaCensus(
    DateTime Taken,
    int Lambdas,
    int Demos,
    int Online,
    int Offline,
    int Blocked,
    IReadOnlyDictionary<LambdaTier, int> Tiers,
    int CreatedDay,
    int CreatedWeek,
    int CreatedMonth,
    int ActiveMonth,
    int GoingOffline,
    int Removed,
    CensusUses Uses,
    int Simple,
    int Full,
    int Versions,
    IReadOnlyDictionary<string, int> Saved,
    DateTime? RequestedSince
);

/// <summary>
/// How many lambdas use each of the platform's features.
/// </summary>
[Summary("How many lambdas use each of the platform's features, the demos left out.")]
public sealed record CensusUses(
    [Summary("How many have their database switched on")]
    int Database,
    [Summary("How many have their workspace switched on")]
    int Workspace,
    [Summary("How many have their secrets switched on")]
    int Secrets,
    [Summary("How many have their visits counted")]
    int Analytics,
    [Summary("How many are in the showcase")]
    int Showcase,
    [Summary("How many have their code published")]
    int Source,
    [Summary("How many have a domain of their own configured")]
    int Domain,
    [Summary("How many have a feature (a draft) open")]
    int Drafts,
    [Summary("How many may connect to other servers, by their tier's default or the operator's decision")]
    int Connects,
    [Summary("How many ask to, and wait for a decision")]
    int Requested
);

/// <summary>
/// The room the installation has and what takes it.
/// </summary>
/// <param name="Directory">The data directory, where everything the platform keeps is</param>
/// <param name="DiskBytes">How large the volume it is on is, where the system says</param>
/// <param name="FreeBytes">How much of that is free for the platform, read as it is asked for</param>
/// <param name="Measured">When what follows was measured; a walk over every file, so it is not measured on every request</param>
/// <param name="Parts">What the data directory holds, by part, largest first</param>
/// <param name="Lambdas">How many lambdas the measurement found something of</param>
/// <param name="Largest">The lambdas that take the most room, largest first</param>
/// <param name="Over">The lambdas whose data is beyond what their tier allows - kept, never written to again until it shrinks</param>
public sealed record StorageCensus(
    string Directory,
    long? DiskBytes,
    long? FreeBytes,
    DateTime? Measured,
    IReadOnlyList<StoragePart> Parts,
    int Lambdas,
    IReadOnlyList<LambdaStorage> Largest,
    IReadOnlyList<LambdaStorage> Over
);

/// <summary>
/// A part of what the data directory holds.
/// </summary>
[Summary("A part of what the data directory holds.")]
public sealed record StoragePart(
    [Summary("Which part: `versions`, `drafts`, `databases`, `workspaces`, `git`, `sources`, `compiled`, `resources`, `platform`, `analytics` or `other`")]
    string Kind,
    [Summary("What its files come to, in bytes")]
    long Bytes,
    [Summary("How many files it is")]
    long Files
);

/// <summary>
/// What one lambda takes of the room.
/// </summary>
/// <param name="DataBytes">Its database and its workspace, counted as its allowance counts them</param>
/// <param name="AllowedBytes">What its tier allows its data</param>
/// <param name="VersionBytes">Its versions, as they are kept</param>
/// <param name="DraftBytes">Its features (drafts), with their copies of the data</param>
/// <param name="OtherBytes">What is kept for it besides: its git history, its packed source, what it was compiled and unpacked to</param>
public sealed record LambdaStorage(string PublicKey, LambdaTier Tier, long DataBytes, long AllowedBytes, long VersionBytes, long DraftBytes, long OtherBytes)
{

    /// <summary>
    /// All of it together.
    /// </summary>
    public long TotalBytes => DataBytes + VersionBytes + DraftBytes + OtherBytes;

}
