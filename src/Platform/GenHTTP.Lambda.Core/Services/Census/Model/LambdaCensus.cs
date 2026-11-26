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
/// <param name="Expiring">How many do either, each counted once - what the listing's <c>expiring</c> lists</param>
/// <param name="Uses">How many use each of the platform's features</param>
/// <param name="Simple">How many open in the simple view, for an owner who does not write code</param>
/// <param name="Full">How many open in the full view</param>
/// <param name="Versions">How many versions are kept, all together</param>
/// <param name="Saved">Versions saved in the last thirty days, by where they came from</param>
/// <param name="AgentModels">Of those, what the installation's agent wrote, by the value of its option <c>model</c>: every value it offers by name</param>
/// <param name="AgentEfforts">The same, by the value of its option <c>effort</c></param>
/// <param name="AgentOptions">The same, by every value of every option it ran on, by option</param>
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
    int Expiring,
    CensusUses Uses,
    int Simple,
    int Full,
    int Versions,
    IReadOnlyDictionary<string, int> Saved,
    IReadOnlyDictionary<string, int> AgentModels,
    IReadOnlyDictionary<string, int> AgentEfforts,
    DateTime? RequestedSince,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>? AgentOptions = null
);
