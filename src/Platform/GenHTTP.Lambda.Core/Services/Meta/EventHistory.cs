using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>Counts per day, one series per kind of event.</summary>
[Summary("What happened on the platform per day, one series per kind of event.")]
public sealed record EventHistory(
    [Summary("The days covered, as `yyyy-MM-dd` in UTC, oldest first")]
    IReadOnlyList<string> Days,
    [Summary("One series per kind of event, its counts in the order of `days`")]
    IReadOnlyList<EventSeries> Series);
