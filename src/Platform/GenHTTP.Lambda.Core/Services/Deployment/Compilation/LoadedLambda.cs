using System.Reflection;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// A lambda that is compiled and in the process, and none of whose code has
/// run yet for the build it is wanted for.
/// </summary>
/// <param name="Scope">The namespace the generated classes are in</param>
internal sealed record LoadedLambda(Assembly Assembly, string Scope);
