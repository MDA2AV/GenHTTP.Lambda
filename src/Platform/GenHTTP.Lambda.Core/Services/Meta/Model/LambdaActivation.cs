using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// One stretch of time a version was online.
/// </summary>
/// <param name="Origin">Who put it online: api, agent or admin</param>
/// <param name="Ended">When it went offline, or nothing while it is online</param>
/// <param name="EndedBy">replaced, stopped, expired or admin</param>
public sealed record LambdaActivation(int Version, DateTime Started, string? Origin, DateTime? Ended, string? EndedBy);
