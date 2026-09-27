namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// One line from the run of the server.
/// </summary>
/// <param name="Domain">The lambda's own domain the request was addressed to, absent for the platform</param>
public sealed record LogEntry(long Seq, DateTime At, string Level, string Source, string? Lambda, string Text,
                              string? Detail, string? Client, string? Agent, string? Country, string? Place,
                              int Repeats, string? Domain);

/// <summary>
/// One caller the log still holds something about.
/// </summary>
public sealed record LogCaller(string Client, string? Place, string? Country, string? Agent,
                               long Lines, long Failed, DateTime First, DateTime Last);

/// <summary>
/// A page of the log, and where to carry on from.
/// </summary>
/// <param name="Cursor">The newest sequence held; ask from here next time</param>
/// <param name="Missed">
/// Lines that were dropped before the reader got to them, because the ring
/// wrapped or because more matched than were asked for
/// </param>
/// <param name="Capacity">How many lines the ring holds in total</param>
/// <param name="Written">Everything said since the server came up</param>
/// <param name="Capturing">Whether what lambdas print is being kept</param>
/// <param name="Addresses">Whether the address a request came from is being recorded</param>
/// <param name="Previous">
/// How the run before this one ended. Nothing on a first start - and on any
/// start where the run before stopped the way it meant to, since a clean stop
/// is not news.
/// </param>
public sealed record LogResponse(
    IReadOnlyList<LogEntry> Lines,
    long Cursor,
    int Missed,
    int Capacity,
    long Written,
    bool Capturing,
    bool Addresses,
    PreviousRun? Previous
);

/// <summary>
/// The end of the run before this one, where it was not a clean one.
/// </summary>
/// <param name="Signalled">
/// Whether anything asked it to stop. Nothing having asked is what points at
/// the machine rather than at an operator or a deployment.
/// </param>
public sealed record PreviousRun(
    DateTime Started,
    DateTime LastSeen,
    double Minutes,
    bool Signalled,
    string? Fault,
    long WorkingSet,
    long Requests,
    int Sockets
);
