using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment;

namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// One page of the listing.
/// </summary>
/// <param name="Total">How many match altogether</param>
public sealed record SourceListing(IReadOnlyList<SourceEntry> Entries, int Total);
