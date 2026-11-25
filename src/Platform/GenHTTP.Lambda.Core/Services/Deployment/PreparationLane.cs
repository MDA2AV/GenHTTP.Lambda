namespace GenHTTP.Lambda.Services.Deployment;

/// <inheritdoc cref="IPreparationLane" />
public sealed class PreparationLane : IPreparationLane, IDisposable
{
    /// <summary>
    /// How long the others wait for a lambda that takes its time to start.
    /// </summary>
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(15);

    private readonly Lane _lane = new();

    #region Functionality

    public ValueTask<IDisposable> EnterAsync(CancellationToken cancellation = default) => _lane.EnterAsync(Patience, cancellation);

    public void Dispose() => _lane.Dispose();

    #endregion

}
