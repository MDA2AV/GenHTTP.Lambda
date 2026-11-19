using System.Collections.Concurrent;
using System.Diagnostics;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Files;

namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// Hands the lambdas their files, measures the workspaces and remembers what
/// it measured for a moment.
/// </summary>
/// <remarks>
/// A lambda's database and its workspace share one allowance, so a connection
/// to the database may grow only as far as the workspace leaves room for -
/// and a lambda opens one for nearly every request it answers. Listing the
/// workspace that often would cost more than the request, so what was
/// measured is kept until the workspace is written to, by the owner, an agent
/// or the lambda itself, and for a few seconds at most besides: what was
/// copied into a feature, or written some other way, is seen then.
/// </remarks>
public sealed class WorkspaceVault(IFileProvider files) : IWorkspaceVault
{
    private static readonly long Patience = Stopwatch.Frequency * 5;

    private readonly ConcurrentDictionary<DataScope, (long Size, long At)> _measured = [];

    #region Functionality

    public WorkspaceBinding Bind(long lambdaId, long? featureId, Func<long> room, Func<long> beside)
        => new LambdaWorkspace(files, this, new DataScope(lambdaId, featureId), room, beside).Bind();

    public long SizeOf(long lambdaId, long? featureId)
    {
        var scope = new DataScope(lambdaId, featureId);

        var now = Stopwatch.GetTimestamp();

        if (_measured.TryGetValue(scope, out var known) && now - known.At < Patience)
        {
            return known.Size;
        }

        var size = WorkspacePaths.Measure(files.List(scope));

        _measured[scope] = (size, now);

        return size;
    }

    public void Changed(long lambdaId, long? featureId) => _measured.TryRemove(new DataScope(lambdaId, featureId), out _);

    /// <remarks>
    /// A whole workspace, gigabytes in production: copied away from the
    /// reactor (see <see cref="Offload"/>).
    /// </remarks>
    public async ValueTask CopyAsync(long lambdaId, long featureId, CancellationToken cancellation = default)
    {
        await Offload.Run(() => files.Copy(new DataScope(lambdaId), new DataScope(lambdaId, featureId), cancellation), cancellation);

        Changed(lambdaId, featureId);
    }

    public void RemoveCopy(long lambdaId, long featureId)
    {
        files.Delete(new DataScope(lambdaId, featureId));

        Changed(lambdaId, featureId);
    }

    public void DeleteAll(long lambdaId)
    {
        files.DeleteAll(lambdaId);

        foreach (var scope in _measured.Keys.Where(s => s.LambdaId == lambdaId).ToList())
        {
            _measured.TryRemove(scope, out _);
        }
    }

    #endregion

}
