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
///
/// Besides what a lambda has online, each of its features can have a preview
/// online: built the same way from the feature's files, with the feature's
/// copy of the workspace and assets of its own, and kept apart from the lambda.
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
    /// Compiles and prepares the given version so it is ready to serve requests.
    /// </summary>
    /// <remarks>
    /// What was being served before goes on being served when this fails -
    /// the handler, and the assets it serves.
    /// </remarks>
    ValueTask<CompilationOutcome> ActivateAsync(long lambdaId, int version, WorkspaceLimits limits, CancellationToken cancellation = default);

    /// <summary>
    /// Returns the handler of the given version, compiling it if needed.
    /// </summary>
    ValueTask<IHandler> ResolveAsync(long lambdaId, int version, WorkspaceLimits limits, CancellationToken cancellation = default);

    /// <summary>
    /// Compiles and prepares the given code as the preview of a feature.
    /// </summary>
    /// <remarks>
    /// Like <see cref="ActivateAsync" />, what the preview served before goes
    /// on being served when this fails.
    /// </remarks>
    /// <param name="preview">Which deployment of the preview this is, so the next one is built again</param>
    ValueTask<CompilationOutcome> PreviewAsync(long lambdaId, long featureId, int preview, string code, WorkspaceLimits limits,
                                               CancellationToken cancellation = default);

    /// <summary>
    /// Returns the handler of a feature's preview, compiling what it was
    /// deployed with if needed.
    /// </summary>
    ValueTask<IHandler> ResolvePreviewAsync(long lambdaId, long featureId, int preview, WorkspaceLimits limits,
                                            CancellationToken cancellation = default);

    /// <summary>
    /// Drops the compiled handler of what a lambda has online, if there is
    /// one. The previews of its features are left as they are: taking the
    /// lambda offline says nothing about them.
    /// </summary>
    void Evict(long lambdaId);

    /// <summary>
    /// Drops every compiled handler of a lambda - what it has online and the
    /// previews of its features - for a lambda that is going away.
    /// </summary>
    void EvictAll(long lambdaId);

    /// <summary>
    /// Drops the compiled handler of a feature's preview, if there is one.
    /// </summary>
    void EvictPreview(long lambdaId, long featureId);

}
