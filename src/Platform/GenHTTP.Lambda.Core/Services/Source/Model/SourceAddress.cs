using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment;

namespace GenHTTP.Lambda.Services.Source;

/// <summary>
/// A name for somebody who shares the address of a published source.
/// </summary>
/// <param name="Updated">When its newest version was saved, for the sitemap</param>
public sealed record SourceAddress(string PublicKey, DateTime? Updated);
