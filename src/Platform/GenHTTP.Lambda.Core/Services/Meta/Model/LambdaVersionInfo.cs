using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// A stored version of the code of a lambda.
/// </summary>
/// <param name="Specification">What the user wanted and why, where whoever saved it said</param>
/// <param name="Change">What it changed, in a line</param>
/// <param name="Origin">Where it came from: template, api or agent</param>
public sealed record LambdaVersionInfo(int Version, DateTime Created, string? Specification = null, string? Change = null, string? Origin = null);
