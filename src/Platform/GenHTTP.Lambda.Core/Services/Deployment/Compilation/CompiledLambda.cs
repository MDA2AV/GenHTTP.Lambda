using GenHTTP.Api.Content;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// A lambda that has been compiled, built and prepared, ready to serve requests.
/// </summary>
/// <param name="Stamp">Which build it is: the version of a lambda, or the deployment of a feature's preview</param>
/// <param name="Settings">What it was compiled with, which a change of tier, of the workspace being on, or of its connectivity makes stale</param>
internal sealed record CompiledLambda(int Stamp, BuildSettings Settings, IHandler Handler);
