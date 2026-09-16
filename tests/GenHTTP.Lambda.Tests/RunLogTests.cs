using GenHTTP.Lambda.Services.Diagnostics;

namespace GenHTTP.Lambda.Tests;

/// <summary>
/// The note one run leaves for the next about how it ended.
/// </summary>
[TestClass]
public sealed class RunLogTests
{

    [TestMethod]
    public void TheFirstRunHasNothingToReportAbout()
    {
        using var room = new Room();

        Assert.IsNull(new RunLog(room.Path).Previous);
    }

    [TestMethod]
    public void ARunThatStoppedProperlySaysSo()
    {
        using var room = new Room();

        var first = new RunLog(room.Path);

        first.Beat(1024, 512, 10, 2);
        first.Stopped();

        var second = new RunLog(room.Path);

        Assert.IsNotNull(second.Previous);
        Assert.IsTrue(second.Previous.Clean);
        Assert.IsFalse(second.Previous.Signalled);
    }

    [TestMethod]
    public void ARunThatVanishedLeavesWhatItWasCarrying()
    {
        using var room = new Room();

        // a heartbeat and then nothing, which is what a process that is killed
        // between one beat and the next leaves behind
        var first = new RunLog(room.Path);

        first.Beat(4L * 1024 * 1024 * 1024, 2L * 1024 * 1024 * 1024, 981_233, 512);

        var second = new RunLog(room.Path);

        Assert.IsNotNull(second.Previous);
        Assert.IsFalse(second.Previous.Clean, "nothing stamped it as stopped");
        Assert.IsFalse(second.Previous.Signalled, "and nothing asked it to");
        Assert.AreEqual(4L * 1024 * 1024 * 1024, second.Previous.WorkingSet);
        Assert.AreEqual(981_233, second.Previous.Requests);
        Assert.AreEqual(512, second.Previous.Sockets);
    }

    [TestMethod]
    public void BeingAskedToStopAndNotFinishingIsItsOwnAnswer()
    {
        using var room = new Room();

        var first = new RunLog(room.Path);

        first.Beat(2048, 1024, 5, 1);
        first.Signalled();
        // and then killed before the shutdown got anywhere

        var second = new RunLog(room.Path);

        Assert.IsNotNull(second.Previous);
        Assert.IsFalse(second.Previous.Clean);
        Assert.IsTrue(second.Previous.Signalled, "which points at whoever asked, not at the machine");
    }

    [TestMethod]
    public void WhatEndedItIsCarriedOverToTheNextRun()
    {
        using var room = new Room();

        var first = new RunLog(room.Path);

        first.Faulted(new OutOfMemoryException("there was not enough of it"));

        var second = new RunLog(room.Path);

        Assert.IsNotNull(second.Previous?.Fault);
        Assert.Contains("OutOfMemoryException", second.Previous.Fault);
        Assert.Contains("there was not enough of it", second.Previous.Fault);
    }

    [TestMethod]
    public void AnUnreadableNoteIsNoWorseThanNoNote()
    {
        using var room = new Room();

        File.WriteAllText(Path.Combine(room.Path, "run.json"), "{ this is not json");

        // a server that cannot start because it could not parse its own note
        // would be a worse failure than the one it is there to explain
        Assert.IsNull(new RunLog(room.Path).Previous);
    }

    private sealed class Room : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                                                             "genhttp-runlog-tests", Guid.NewGuid().ToString("n"));

        public Room() => Directory.CreateDirectory(Path);

        public void Dispose()
        {
            try { Directory.Delete(Path, true); } catch (Exception) { }
        }
    }

}
