using GenHTTP.Api.Content.Documentation;
using GenHTTP.Lambda.Services.Census;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The lambdas of the installation, counted. The demos are counted apart and in nothing else.")]
public sealed record AdminCensusResponse(
    [Summary("When they were counted")]
    DateTime Taken,
    [Summary("How many there are, the demos left out")]
    int Total,
    [Summary("How many demos the installation keeps")]
    int Demos,
    [Summary("How many have a version online")]
    int Online,
    [Summary("How many have none and are not blocked")]
    int Offline,
    [Summary("How many the operator blocked")]
    int Blocked,
    [Summary("How many are in each tier, by its name - `Sandbox`, `Production`, `System` and `Demo`")]
    IReadOnlyDictionary<string, int> Tiers,
    [Summary("How many were created in the last day")]
    int CreatedDay,
    [Summary("How many were created in the last seven days")]
    int CreatedWeek,
    [Summary("How many were created in the last thirty days")]
    int CreatedMonth,
    [Summary("How many were changed or visited in the last thirty days")]
    int ActiveMonth,
    [Summary("How many go offline within seven days unless they are used - in a tier that is not kept while unused")]
    int GoingOffline,
    [Summary("How many are removed within seven days unless they are used")]
    int Removed,
    [Summary("How many do either, each counted once - what `GET /admin/lambdas?state=expiring` lists")]
    int Expiring,
    [Summary("How many use each of the platform's features")]
    CensusUses Uses,
    [Summary("How many open in the simple view, for an owner who does not write code")]
    int Simple,
    [Summary("How many open in the full view")]
    int Full,
    [Summary("How many versions are kept, all together")]
    int Versions,
    [Summary("Versions saved in the last thirty days, by where they came from: `agent` (MCP), `api`, `git`, `template`, `admin` or `system`")]
    IReadOnlyDictionary<string, int> Saved
)
{

    public static AdminCensusResponse Of(LambdaCensus census) => new(
        census.Taken, census.Lambdas, census.Demos, census.Online, census.Offline, census.Blocked,
        census.Tiers.ToDictionary(t => t.Key.ToString(), t => t.Value),
        census.CreatedDay, census.CreatedWeek, census.CreatedMonth, census.ActiveMonth, census.GoingOffline, census.Removed, census.Expiring,
        census.Uses, census.Simple, census.Full, census.Versions, census.Saved);

}
