namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// Lets one lambda be compiled at a time, whichever platform of the process
/// it belongs to.
/// </summary>
/// <remarks>
/// Compiling is memory hungry, and the memory is the process's: several
/// platforms starting at once - each with lambdas to warm up and demos to
/// bring online - would otherwise compile as many at once as there are of them.
///
/// The turn is for compiling and nothing else. A lambda is started between its
/// two compilations - Roslyn's of its code and GenHTTP's of its handlers - and
/// does so outside of it: that is the lambda's own code, which takes as long
/// as it likes, and another platform has no part in it.
/// </remarks>
public sealed class CompilationGate : IDisposable
{
    /// <summary>
    /// How long the others wait for the handler of a lambda to be prepared:
    /// GenHTTP is done in a moment, but a handler may be the lambda's own.
    /// </summary>
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(5);

    private readonly Lane _lane = new();

    #region Functionality

    /// <summary>
    /// Waits for the turn to compile the code of a lambda, which must not be
    /// run in it.
    /// </summary>
    public ValueTask<IDisposable> CompileAsync(CancellationToken cancellation = default) => _lane.EnterAsync(null, cancellation);

    /// <summary>
    /// Waits for the turn to have the handler of a lambda prepared.
    /// </summary>
    public ValueTask<IDisposable> PrepareAsync(CancellationToken cancellation = default) => _lane.EnterAsync(Patience, cancellation);

    public void Dispose() => _lane.Dispose();

    #endregion

}
