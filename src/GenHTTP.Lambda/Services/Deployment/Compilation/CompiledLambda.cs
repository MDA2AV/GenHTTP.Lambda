using GenHTTP.Api.Content;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// A lambda that has been compiled, built and prepared, ready to serve requests.
/// </summary>
internal sealed record CompiledLambda(int Version, IHandler Handler);
