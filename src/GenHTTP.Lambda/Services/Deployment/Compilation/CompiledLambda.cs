using GenHTTP.Api.Content;

using GenHTTP.Lambda.Services.Workspace;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// A lambda that has been compiled, built and prepared, ready to serve requests.
/// </summary>
/// <param name="Revision">Which save of the version it was built from</param>
/// <param name="Limits">The workspace limits compiled into it, which a change of tier - or of the workspace being on - makes stale</param>
internal sealed record CompiledLambda(int Version, int Revision, WorkspaceLimits Limits, IHandler Handler);
