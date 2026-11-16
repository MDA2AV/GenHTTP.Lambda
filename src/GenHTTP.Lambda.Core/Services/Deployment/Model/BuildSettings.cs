namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// What a lambda is compiled with besides its code: whether it has a
/// workspace, and whether it may connect to other servers.
/// </summary>
/// <remarks>
/// Both are part of the build rather than checked while it runs - the
/// workspace as the body of the generated workspace class, connectivity as
/// the assemblies the compiler is handed and what the code guard lets
/// through - so a lambda built with other settings than the ones asked for is
/// built again, which runs its startup again. That is how a workspace switched
/// off or the operator's decision about connectivity takes effect: on the next
/// request, without a deploy.
///
/// How much room its data may take is not among them: the workspace reads it
/// while it runs (<c>IDatabaseVault.DataRoomFor</c>), so moving a lambda to
/// another tier, or changing what a tier allows, leaves it running as it is.
/// Nor is a lambda built again for being allowed to connect where it was
/// built without (<c>DeploymentService.IsDeployed</c>): it compiled without
/// the means to, so it does not connect - only taking connectivity away
/// builds it again.
/// </remarks>
/// <param name="Workspace">Whether its owner left the workspace switched on</param>
/// <param name="Connectivity">Whether it may connect to other servers - the operator allowed it, or its tier does by default</param>
public sealed record BuildSettings(bool Workspace = true, bool Connectivity = false)
{

    /// <summary>
    /// What a lambda nobody knows anything about yet is checked with.
    /// </summary>
    public static BuildSettings Standard { get; } = new();

}
