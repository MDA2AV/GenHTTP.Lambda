namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// The workspaces of the lambdas as the other services keep them in step with
/// a lambda: what a compiled lambda reads and writes its files through, the
/// copy a feature works on, the room one takes - which the database of the
/// lambda shares its allowance with - and what goes when a lambda does.
/// </summary>
/// <remarks>
/// Apart from <see cref="IWorkspaceService"/>, which reads and writes files
/// for the owner and the agents. This one is asked whenever a lambda opens a
/// connection to its database, so it remembers what it measured. Where the
/// files are kept is the files integration's.
/// </remarks>
public interface IWorkspaceVault
{

    /// <summary>
    /// What a compiled lambda - or the preview of a feature - reads and
    /// writes its files through.
    /// </summary>
    /// <param name="room">The room of the lambda's data, as its tier allows it now</param>
    /// <param name="beside">The room the rest of its data - its database - takes</param>
    WorkspaceBinding Bind(long lambdaId, long? featureId, Func<CancellationToken, ValueTask<long>> room, Func<CancellationToken, ValueTask<long>> beside);

    /// <summary>
    /// The room the workspace of a lambda - or a feature's copy of it -
    /// takes, counted in blocks as its quota is.
    /// </summary>
    /// <remarks>
    /// Measured a moment ago at most: walking a workspace of many files is
    /// not something to do for every request.
    /// </remarks>
    ValueTask<long> SizeOfAsync(long lambdaId, long? featureId, CancellationToken cancellation = default);

    /// <summary>
    /// Forgets what was measured of a workspace, which was just written to.
    /// </summary>
    void Changed(long lambdaId, long? featureId);

    /// <summary>
    /// Replaces a feature's copy of the workspace with a copy of the lambda's.
    /// </summary>
    ValueTask CopyAsync(long lambdaId, long featureId, CancellationToken cancellation = default);

    /// <summary>
    /// Removes a feature's copy of the workspace.
    /// </summary>
    ValueTask RemoveCopyAsync(long lambdaId, long featureId, CancellationToken cancellation = default);

    /// <summary>
    /// Removes the workspace of a lambda and every copy of its features.
    /// </summary>
    ValueTask DeleteAllAsync(long lambdaId, CancellationToken cancellation = default);

}
