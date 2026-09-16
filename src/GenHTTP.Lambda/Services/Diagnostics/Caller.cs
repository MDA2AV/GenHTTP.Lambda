using System.Net;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// Who the current piece of work is being done for.
/// </summary>
/// <remarks>
/// Set once at the outside of every request and read back by anything that
/// writes a line while it is being served - so a lambda's print, an error the
/// server logged and the request line itself all name the same caller without
/// any of them having to be handed it.
/// </remarks>
public static class Caller
{

    private static readonly AsyncLocal<CallerInfo?> Current = new();

    /// <summary>
    /// The request being served here, or nothing.
    /// </summary>
    public static CallerInfo? Ambient => Current.Value;

    public static IDisposable Enter(CallerInfo caller)
    {
        var previous = Current.Value;

        Current.Value = caller;

        return new Leave(previous);
    }

    private sealed class Leave(CallerInfo? previous) : IDisposable
    {
        public void Dispose() => Current.Value = previous;
    }

}

/// <summary>
/// The parts of a request worth keeping against every line it causes.
/// </summary>
public sealed record CallerInfo(string? Client, string? Agent, string Method, string Path)
{

    /// <summary>
    /// Reads them off a request, sharing the repeated ones.
    /// </summary>
    /// <remarks>
    /// A forwarded address is recorded as what was claimed and what it arrived
    /// from, both, because the header is written by the sender: taking it at
    /// face value would let anyone put any address in this log, and ignoring
    /// it would name the proxy on every line of a proxied installation.
    /// </remarks>
    public static CallerInfo From(IRequest request, StringPool pool, bool addresses)
    {
        string? client = null;

        if (addresses)
        {
            var peer = Format(request.Client.Address);

            var claimed = First(request.Header.Headers.GetEntry("X-Forwarded-For"));

            client = claimed == null || claimed == peer ? peer : $"{claimed} via {peer}";
        }

        return new CallerInfo(
            pool.Share(client),
            pool.Share(Trim(request.Header.Headers.GetEntry("User-Agent"), 200)),
            request.Header.Method.ToString(),
            request.Header.Path.ToString()
        );
    }

    private static string? Format(IPAddress? address)
    {
        if (address == null)
        {
            return null;
        }

        var text = address.ToString();

        // an IPv4 client reaching a dual stack socket arrives wearing a v6
        // mapping, which is the same address written in a way nobody searches
        // their logs for
        return text.StartsWith("::ffff:", StringComparison.Ordinal) ? text[7..] : text;
    }

    private static string? First(string? forwarded)
    {
        if (string.IsNullOrWhiteSpace(forwarded))
        {
            return null;
        }

        // the leftmost is the original client, the rest are the hops
        var cut = forwarded.IndexOf(',');

        return Trim(cut < 0 ? forwarded : forwarded[..cut], 60);
    }

    private static string? Trim(string? value, int most)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        value = value.Trim();

        return value.Length <= most ? value : value[..most];
    }

}
