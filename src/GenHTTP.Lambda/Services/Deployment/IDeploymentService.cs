using GenHTTP.Api.Content;
using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// Compiles the code of a lambda into a handler and keeps the result around, so
/// a lambda is built once and not on every request.
/// </summary>
public interface IDeploymentService
{

    /// <summary>
    /// Compiles a snippet without running it, to tell the editor whether it would build.
    /// </summary>
    /// <param name="code">The code to be checked</param>
    /// <param name="lambdaId">The lambda the code belongs to, if it is already stored</param>
    ValueTask<CompilationOutcome> ValidateAsync(string code, long? lambdaId = null, CancellationToken cancellation = default);

    /// <summary>
    /// Compiles and prepares the given version so it is ready to serve requests.
    /// </summary>
    ValueTask<CompilationOutcome> ActivateAsync(long lambdaId, int version, CancellationToken cancellation = default);

    /// <summary>
    /// Returns the handler of the given version, compiling it if needed.
    /// </summary>
    ValueTask<IHandler> ResolveAsync(long lambdaId, int version, CancellationToken cancellation = default);

    /// <summary>
    /// Drops the compiled handler of a lambda, if there is one.
    /// </summary>
    void Evict(long lambdaId);

}
