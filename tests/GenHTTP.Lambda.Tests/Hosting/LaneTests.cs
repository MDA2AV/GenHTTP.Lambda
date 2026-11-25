using GenHTTP.Lambda.Services.Deployment;

namespace GenHTTP.Lambda.Tests.Hosting;

/// <summary>
/// What is done one at a time in the process, and how long anybody waits for it.
/// </summary>
[TestClass]
public sealed class LaneTests
{

    [TestMethod]
    public async Task OneAtATime()
    {
        using var lane = new PreparationLane();

        var first = await lane.EnterAsync();

        var second = lane.EnterAsync().AsTask();

        await Task.Delay(100);

        Assert.IsFalse(second.IsCompleted);

        first.Dispose();

        using var _ = await second.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [TestMethod]
    public async Task WhoeverWaitsMayGiveUp()
    {
        using var lane = new PreparationLane();

        using var cancellation = new CancellationTokenSource();

        using var first = await lane.EnterAsync();

        var second = lane.EnterAsync(cancellation.Token).AsTask();

        await cancellation.CancelAsync();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => second);
    }

    [TestMethod]
    public async Task AHandlerThatIsNotDoneLosesItsTurnAndAnyOtherDoesNot()
    {
        using var gate = new CompilationGate();

        var preparing = await gate.PrepareAsync();

        // the code of a lambda may run in it, and does not let go of it
        using var compiling = await gate.CompileAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(12));

        var next = gate.PrepareAsync().AsTask();

        await Task.Delay(TimeSpan.FromSeconds(6));

        Assert.IsFalse(next.IsCompleted, "what compiles has its turn until it is done");

        // and giving back what was lost takes nothing from whoever has it now
        preparing.Dispose();
        preparing.Dispose();

        await Task.Delay(100);

        Assert.IsFalse(next.IsCompleted);

        compiling.Dispose();

        using var _ = await next.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [TestMethod]
    public async Task ATurnMayOutliveTheProcessesNeedOfIt()
    {
        var gate = new CompilationGate();

        var turn = await gate.CompileAsync();

        gate.Dispose();

        turn.Dispose();
    }

}
