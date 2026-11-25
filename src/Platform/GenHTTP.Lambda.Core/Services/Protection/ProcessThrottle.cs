using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Services.Protection;

/// <summary>
/// How many lambda requests may run at the same time in a process several
/// platforms share, those of all of them together.
/// </summary>
/// <remarks>
/// Beside the slots of each platform (<see cref="LambdaThrottle"/>), which
/// keep one from taking them all. A platform that has its process for itself
/// is bounded by its own and passes no second gate.
/// </remarks>
public sealed class ProcessThrottle(RuntimeOptions options) : IDisposable
{

    #region Get-/Setters

    internal SemaphoreSlim Slots { get; } = new(options.MaxConcurrency, options.MaxConcurrency);

    #endregion

    #region Lifecycle

    public void Dispose() => Slots.Dispose();

    #endregion

}
