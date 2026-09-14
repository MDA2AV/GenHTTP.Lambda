namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// Persists the source code of the lambdas. Backed by the file system for now,
/// a git repository would be a natural next step.
/// </summary>
public interface IStorageService
{

    /// <summary>
    /// Stores the code of a newly created version.
    /// </summary>
    ValueTask WriteAsync(long lambdaId, int version, string code, CancellationToken cancellation = default);

    /// <summary>
    /// Reads the code of the given version, if it exists.
    /// </summary>
    ValueTask<string?> ReadAsync(long lambdaId, int version, CancellationToken cancellation = default);

    /// <summary>
    /// Removes a single version of a lambda.
    /// </summary>
    ValueTask DeleteVersionAsync(long lambdaId, int version, CancellationToken cancellation = default);

    /// <summary>
    /// Removes everything stored for the given lambda, including its workspace.
    /// </summary>
    ValueTask DeleteAsync(long lambdaId, CancellationToken cancellation = default);

    /// <summary>
    /// The directory a deployed lambda may read and write files in. Created on demand.
    /// </summary>
    string GetWorkspace(long lambdaId);

    /// <summary>
    /// The directory the assemblies generated for a lambda are written to.
    /// </summary>
    string GetAssemblyDirectory(long lambdaId);

    /// <summary>
    /// The directory the assets shipped with a lambda are written to.
    /// </summary>
    /// <remarks>
    /// Rewritten from the deployed version every time one goes online, so it
    /// holds what that version shipped and nothing a previous one did. The
    /// lambda may read it and not write it - what it writes goes in the
    /// workspace, which outlives a deployment.
    /// </remarks>
    string GetAssetDirectory(long lambdaId);

}
