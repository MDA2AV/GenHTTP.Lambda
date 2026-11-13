namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// The workspaces of the lambdas as the other services measure them: the
/// room one takes, which the database of the lambda shares its allowance
/// with.
/// </summary>
/// <remarks>
/// Apart from <see cref="IWorkspaceService"/>, which reads and writes files
/// for the owner and the agents. This one is asked whenever a lambda opens a
/// connection to its database, so it remembers what it measured.
/// </remarks>
public interface IWorkspaceVault
{

    /// <summary>
    /// The room the workspace of a lambda - or a feature's copy of it -
    /// takes, counted in blocks as its quota is.
    /// </summary>
    /// <remarks>
    /// Measured a moment ago at most: walking a workspace of many files is
    /// not something to do for every request.
    /// </remarks>
    long SizeOf(long lambdaId, long? featureId);

    /// <summary>
    /// Forgets what was measured of a workspace, which was just written to.
    /// </summary>
    void Changed(long lambdaId, long? featureId);

}
