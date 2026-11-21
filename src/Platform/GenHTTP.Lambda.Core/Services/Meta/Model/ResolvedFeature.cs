using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// The preview of a feature a request was addressed to.
/// </summary>
/// <param name="Id">The identity its files and data are kept under</param>
/// <param name="Key">What it is addressed by</param>
/// <param name="Preview">Which deployment of the preview is online, so a preview deployed again is built again</param>
public sealed record ResolvedFeature(long Id, string Key, int Preview);
