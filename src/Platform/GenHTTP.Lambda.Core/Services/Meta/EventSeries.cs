using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Meta;

[Summary("How often one kind of event happened, day by day.")]
public sealed record EventSeries(
    [Summary("`created`, `saved`, `deployed`, `undeployed` or `deleted` - of lambdas")]
    string Kind,
    [Summary("How many happened on each day of the window")]
    IReadOnlyList<int> Counts,
    [Summary("How many have ever happened, beyond the window too")]
    int Total);
