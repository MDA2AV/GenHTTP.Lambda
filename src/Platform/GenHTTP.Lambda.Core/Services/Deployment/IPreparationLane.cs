namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// Has what nobody asked for yet - the demos, the lambdas kept warm - built
/// one lambda after another, whichever platform of the process it belongs to.
/// </summary>
/// <remarks>
/// A platform does so one lambda at a time, so that a visitor whose lambda is
/// not built yet, and an owner putting a version online, wait for one of them
/// at most. Several platforms starting at once would each have one in front
/// of them.
/// </remarks>
public interface IPreparationLane
{

    /// <summary>
    /// Waits for the turn to have a lambda built. The next one has its turn
    /// after a while even if this one is not done.
    /// </summary>
    /// <returns>The turn, held until it is disposed of</returns>
    ValueTask<IDisposable> EnterAsync(CancellationToken cancellation = default);

}
