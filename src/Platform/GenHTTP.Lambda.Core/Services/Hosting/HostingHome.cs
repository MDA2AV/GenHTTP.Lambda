using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Services.Hosting;

/// <summary>
/// The hosting domain itself, which is nobody's lambda.
/// </summary>
public sealed record HostingHome(string Name) : LambdaHost(Name);
