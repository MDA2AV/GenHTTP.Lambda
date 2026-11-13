using System.Collections.Concurrent;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Hands back one shared instance of a string that keeps recurring.
/// </summary>
/// <remarks>
/// A million log lines from a few dozen callers is a million references and a
/// few dozen strings, not a million strings - which is the difference between
/// the addresses costing eight bytes a line and costing sixty. The same goes
/// for user agents, of which a busy server sees a handful repeated endlessly.
///
/// Bounded, because the whole point is that the set is small: a caller that
/// turns out to be a million distinct addresses stops being pooled rather than
/// becoming a second copy of the log. Past the bound the string is simply
/// returned as it came, so the pool degrades into doing nothing rather than
/// into a leak.
/// </remarks>
public sealed class StringPool(int most = 4096)
{

    private readonly ConcurrentDictionary<string, string> _held = new(StringComparer.Ordinal);

    /// <summary>
    /// How many distinct values are being shared.
    /// </summary>
    public int Count => _held.Count;

    public string? Share(string? value)
    {
        if (value == null)
        {
            return null;
        }

        if (_held.TryGetValue(value, out var held))
        {
            return held;
        }

        if (_held.Count >= most)
        {
            return value;
        }

        _held.TryAdd(value, value);

        return value;
    }

}
