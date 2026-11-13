using GenHTTP.Lambda.Services.Workspace;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// What a lambda is compiled with besides its code: what it may keep in its
/// workspace, and whether it may connect to other servers.
/// </summary>
/// <remarks>
/// Both are part of the build rather than checked while it runs - the room
/// as a constant of the generated workspace class, connectivity as the assemblies
/// the compiler is handed and what the code guard lets through - so a lambda
/// built with other settings than the ones asked for is built again. That is
/// how a change of tier, a workspace switched off or the operator's decision
/// about connectivity takes effect: on the next request, without a deploy.
/// </remarks>
/// <param name="Workspace">What it may keep in its workspace</param>
/// <param name="Connectivity">Whether it may connect to other servers - its tier includes it, or the operator allowed it</param>
public sealed record BuildSettings(WorkspaceLimits Workspace, bool Connectivity = false)
{

    /// <summary>
    /// What a lambda nobody knows anything about yet is checked with.
    /// </summary>
    public static BuildSettings Standard { get; } = new(WorkspaceLimits.Standard);

}
