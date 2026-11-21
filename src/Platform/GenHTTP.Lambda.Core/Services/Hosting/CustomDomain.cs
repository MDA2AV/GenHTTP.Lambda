using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// A domain of its own the request was addressed to, and the lambda it
/// belonged to when it arrived.
/// </summary>
public sealed record CustomDomain(string Name, long LambdaId) : LambdaHost(Name);
