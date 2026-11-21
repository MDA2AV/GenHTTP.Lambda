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
