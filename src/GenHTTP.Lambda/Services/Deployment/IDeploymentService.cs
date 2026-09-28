using GenHTTP.Api.Content;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Workspace;

namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// Compiles the code of a lambda into a handler and keeps the result around, so
/// a lambda is built once and not on every request.
/// </summary>
/// <remarks>
/// The workspace limits are compiled into a lambda, so each of these takes the
/// ones of its tier - and a lambda compiled with other limits than the ones
/// asked for is compiled again, which is how a change of tier takes effect.
/// </remarks>
public interface IDeploymentService
{

    /// <summary>
    /// Compiles a snippet without running it, to tell the editor whether it would build.
    /// </summary>
    /// <param name="code">The code to be checked</param>
    /// <param name="lambdaId">The lambda the code belongs to, if it is already stored</param>
    /// <param name="limits">What the lambda may keep in its workspace, the standard limits if not given</param>
    ValueTask<CompilationOutcome> ValidateAsync(string code, long? lambdaId = null, WorkspaceLimits? limits = null, CancellationToken cancellation = default);

    /// <summary>
    /// Compiles and prepares the given version, as it was last saved, so it is
    /// ready to serve requests.
    /// </summary>
    /// <remarks>
    /// What was being served before goes on being served when this fails -
    /// the handler, and the assets it serves.
    /// </remarks>
    /// <param name="revision">Which save of the version that is, so a version saved over is built again</param>
    ValueTask<CompilationOutcome> ActivateAsync(long lambdaId, int version, int revision, WorkspaceLimits limits, CancellationToken cancellation = default);

    /// <summary>
    /// Returns the handler of what is online, compiling it if needed.
    /// </summary>
    /// <remarks>
    /// Built from the code that was deployed rather than the version as it is
    /// now, which differ once the version online has been saved over: nothing
    /// visitors get changes until it is deployed again, a restart included.
    /// </remarks>
    ValueTask<IHandler> ResolveAsync(long lambdaId, int version, int revision, WorkspaceLimits limits, CancellationToken cancellation = default);

    /// <summary>
    /// Drops the compiled handler of a lambda, if there is one.
    /// </summary>
    void Evict(long lambdaId);

}
