namespace GenHTTP.Lambda.Services.Telemetry;

/// <summary>
/// What an installation may say about the process it runs in.
/// </summary>
/// <remarks>
/// Everything where the process is its own (<see cref="OwnProcess"/>), and
/// nothing where it shares it with others (<see cref="SharedProcess"/>): the
/// memory and the connections of a process are those of everything in it.
/// </remarks>
public interface IProcessReadings
{

    /// <summary>
    /// Whether the process serves other installations as well.
    /// </summary>
    bool Shared { get; }

    /// <summary>
    /// How many connections are open right now.
    /// </summary>
    int OpenConnections { get; }

    /// <summary>
    /// Reads the process. What is counted between readings is counted since
    /// the one before, so only whoever keeps the series takes them.
    /// </summary>
    ProcessReading Take();

}
