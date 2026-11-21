using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Hosting;

namespace GenHTTP.Lambda.Services.Features;

/// <summary>
/// What a new feature is to be.
/// </summary>
/// <param name="Name">What it is, in a few words</param>
/// <param name="Specification">What the user wants from it and why</param>
/// <param name="Base">The version to branch off; the newest when left out</param>
/// <param name="Origin">Which door it came through</param>
/// <param name="Branch">The branch it is to be in the lambda's git repository; left out, one named after it</param>
public sealed record FeatureDraft(string? Name, string? Specification = null, int? Base = null, string? Origin = null, string? Branch = null);
