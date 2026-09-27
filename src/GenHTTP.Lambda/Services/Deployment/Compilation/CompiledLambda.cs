using GenHTTP.Api.Content;

using GenHTTP.Lambda.Services.Workspace;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// A lambda that has been compiled, built and prepared, ready to serve requests.
/// </summary>
/// <param name="Limits">The workspace limits compiled into it, which a change of tier makes stale</param>
internal sealed record CompiledLambda(int Version, WorkspaceLimits Limits, IHandler Handler);
