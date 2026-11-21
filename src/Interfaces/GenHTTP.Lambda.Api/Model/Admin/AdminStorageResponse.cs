using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Services.Census;
using GenHTTP.Lambda.Services.Integrations;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Optimization;
using GenHTTP.Lambda.Services.QuietHours;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Telemetry;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The room the installation has, and what takes it.")]
public sealed record AdminStorageResponse(
    [Summary("The data directory, where everything the platform keeps is")]
    string Directory,
    [Summary("The volume it is on, read as this was asked for; absent where the system does not say")]
    AdminDiskResponse? Disk,
    [Summary("When what follows was measured - a walk over every file, done every hour, not on every request")]
    DateTime? Measured,
    [Summary("What the data directory holds, by part, the largest first: `versions`, `drafts`, `databases`, `workspaces`, `git`, `sources` (packed for `/source`), `compiled` and `resources` (what the lambdas were compiled and unpacked to, made again when missing), `platform` and `analytics` (the platform's two databases), `locations` and `crawlers` (the lists the log and the analytics read, downloaded again when missing), and `other`")]
    IReadOnlyList<StoragePart> Parts,
    [Summary("How many lambdas something was found of")]
    int Lambdas,
    [Summary("The lambdas that take the most room, the largest first")]
    IReadOnlyList<AdminLambdaStorage> Largest,
    [Summary("The lambdas whose data is beyond what their tier allows, the furthest first - kept as it is, and written to again only once it is within it. A lambda gets there when it is moved to a tier that allows less, or the operator lowers the limit.")]
    IReadOnlyList<AdminLambdaStorage> Over
)
{

    public static AdminStorageResponse Of(StorageCensus census) => new(
        census.Directory,
        census.DiskBytes is { } size && census.FreeBytes is { } free ? new AdminDiskResponse(size, free) : null,
        census.Measured,
        census.Parts,
        census.Lambdas,
        [.. census.Largest.Select(AdminLambdaStorage.Of)],
        [.. census.Over.Select(AdminLambdaStorage.Of)]);

}
