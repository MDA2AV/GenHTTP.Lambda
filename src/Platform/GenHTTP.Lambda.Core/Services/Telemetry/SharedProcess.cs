namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// Reads nothing of a process that serves other installations as well.
/// </summary>
/// <remarks>
/// What the operator of one installation would be shown is what all of them
/// do together, under the heading of their own. The host that runs them looks
/// after the process. Not reading it is worth something too: a reading walks
/// every mapping of the process, which grow with every lambda of every
/// installation, so each of them taking one would cost more with each of them.
/// </remarks>
public sealed class SharedProcess : IProcessReadings
{

    public bool Shared => true;

    public int OpenConnections => 0;

    public ProcessReading Take() => default;

}
