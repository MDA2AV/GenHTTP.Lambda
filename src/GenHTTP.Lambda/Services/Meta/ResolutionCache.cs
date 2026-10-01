using System.Collections.Concurrent;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Remembers which lambda answers to what, so a request to a lambda does not
/// have to ask the database.
/// </summary>
/// <remarks>
/// Asked on every request a lambda or a preview serves. What it answers - the
/// lambda, the version it has online, its tier, whether its workspace is on -
/// only changes when something is written, so an answer is kept for as long
/// as nothing was (see <see cref="DatabaseChanges"/>) and read again on the
/// first request after. Requests run on the threads that do the engine's
/// I/O, and three queries per request was most of what serving a small lambda
/// cost.
///
/// That nothing answers to a key is kept as well: a scanner trying keys is
/// otherwise a query per guess. Bounded, because the keys come from whoever
/// sends requests - past the limit it starts over, which costs a query per
/// key that is asked for again.
/// </remarks>
internal sealed class ResolutionCache<TKey>(DatabaseChanges changes) where TKey : notnull
{
    private const int MostEntries = 10_000;

    private readonly ConcurrentDictionary<TKey, Entry> _entries = [];

    #region Functionality

    /// <summary>
    /// What answers to the key: as remembered, or as loaded now if anything
    /// was written since it was remembered.
    /// </summary>
    public ResolvedLambda? Resolve(TKey key, Func<TKey, ResolvedLambda?> load)
    {
        // read before loading, so a write landing while it loads leaves the
        // entry stale rather than current
        var generation = changes.Generation;

        if (_entries.TryGetValue(key, out var known) && known.Generation == generation)
        {
            return known.Lambda;
        }

        var loaded = load(key);

        if (_entries.Count >= MostEntries)
        {
            _entries.Clear();
        }

        _entries[key] = new Entry(generation, loaded);

        return loaded;
    }

    private sealed record Entry(long Generation, ResolvedLambda? Lambda);

    #endregion

}
