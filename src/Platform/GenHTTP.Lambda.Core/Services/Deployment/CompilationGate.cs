namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// Lets one lambda be compiled at a time, whichever platform of the process
/// it belongs to.
/// </summary>
/// <remarks>
/// Compiling is memory hungry, and the memory is the process's: several
/// platforms starting at once - each with lambdas to warm up and demos to
/// bring online - would otherwise compile as many at once as there are of them.
/// </remarks>
public sealed class CompilationGate : IDisposable
{
    private readonly SemaphoreSlim _compiling = new(1, 1);

    #region Functionality

    /// <summary>
    /// Waits for the turn to compile.
    /// </summary>
    /// <returns>The turn, held until it is disposed of</returns>
    public async ValueTask<IDisposable> EnterAsync(CancellationToken cancellation = default)
    {
        await _compiling.WaitAsync(cancellation);

        return new Turn(_compiling);
    }

    public void Dispose() => _compiling.Dispose();

    private sealed class Turn(SemaphoreSlim compiling) : IDisposable
    {
        private int _left;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _left, 1) == 0)
            {
                compiling.Release();
            }
        }
    }

    #endregion

}
