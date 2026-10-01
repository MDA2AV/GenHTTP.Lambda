using System.Collections.Concurrent;

using GenHTTP.Lambda.Api.Model;

namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// What the overview of a lambda says about one of its versions: how large it
/// is, what its code reaches for and what its documentation says.
/// </summary>
public sealed record VersionFacts(int CodeFiles, int CodeLength, int AssetFiles, long AssetBytes, bool ServesAssets, bool ServesWorkspace,
                                  bool UsesWorkspace, bool UsesDatabase, DocumentationSummary Documentation);

/// <summary>
/// The facts of the versions whose overview was asked for lately.
/// </summary>
/// <remarks>
/// The overview is asked for every ten seconds by every editor that is open
/// on it, and reading these facts means reading and unpacking the whole
/// version - assets included, up to a hundred megabytes and more. A version
/// never changes once saved, so what is read off it once holds for good: kept
/// by the lambda's id, which is never handed out again, and the number of the
/// version. Bounded by starting over, like the other caches of requests.
/// </remarks>
public sealed class VersionFactsCache
{
    private const int MostEntries = 1024;

    private readonly ConcurrentDictionary<(long LambdaId, int Version), VersionFacts> _facts = [];

    public async ValueTask<VersionFacts> GetAsync(long lambdaId, int version, Func<ValueTask<VersionFacts?>> read)
    {
        if (_facts.TryGetValue((lambdaId, version), out var known))
        {
            return known;
        }

        var facts = await read();

        if (facts == null)
        {
            // a version that could not be read is not remembered as empty,
            // it is asked about again
            return Empty;
        }

        if (_facts.Count >= MostEntries)
        {
            _facts.Clear();
        }

        return _facts[(lambdaId, version)] = facts;
    }

    public static VersionFacts Empty { get; } = new(0, 0, 0, 0, false, false, false, false, new DocumentationSummary(null, false, false, false, 0, 0));

}
