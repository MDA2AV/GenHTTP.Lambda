using System.Collections.Concurrent;
using System.Diagnostics;

using GenHTTP.Lambda.Services.Storage;

namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// Measures the workspaces of the lambdas, and remembers what it measured for
/// a moment.
/// </summary>
/// <remarks>
/// A lambda's database and its workspace share one allowance, so a connection
/// to the database may grow only as far as the workspace leaves room for -
/// and a lambda opens one for nearly every request it answers. Walking the
/// workspace that often would cost more than the request, so what was
/// measured is kept until the workspace is written to, by the owner, an agent
/// or the lambda itself, and for a few seconds at most besides: what was
/// copied into a feature, or written some other way, is seen then.
/// </remarks>
public sealed class WorkspaceVault(IStorageService storage) : IWorkspaceVault
{
    private static readonly long Patience = Stopwatch.Frequency * 5;

    private readonly ConcurrentDictionary<(long LambdaId, long? FeatureId), (long Size, long At)> _measured = [];

    #region Functionality

    public long SizeOf(long lambdaId, long? featureId)
    {
        var key = (lambdaId, featureId);

        var now = Stopwatch.GetTimestamp();

        if (_measured.TryGetValue(key, out var known) && now - known.At < Patience)
        {
            return known.Size;
        }

        var size = Measure(storage.GetWorkspace(lambdaId, featureId));

        _measured[key] = (size, now);

        return size;
    }

    public void Changed(long lambdaId, long? featureId) => _measured.TryRemove((lambdaId, featureId), out _);

    /// <summary>
    /// The room everything in a folder takes, counted as the quota is: every
    /// file in whole blocks, at least one, and a block for every folder.
    /// </summary>
    public static long Measure(string root)
    {
        if (!Directory.Exists(root))
        {
            return 0;
        }

        var used = 0L;

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            used += WorkspaceLimits.Footprint(Size(file));
        }

        return used + Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories).LongCount() * WorkspaceLimits.Block;
    }

    #endregion

    #region Helpers

    private static long Size(string file)
    {
        try
        {
            return new FileInfo(file).Length;
        }
        catch (IOException)
        {
            // removed since the directory was read
            return 0;
        }
    }

    #endregion

}
