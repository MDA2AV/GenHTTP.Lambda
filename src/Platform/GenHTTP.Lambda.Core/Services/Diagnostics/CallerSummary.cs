namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// One caller, and everything the log still holds about them.
/// </summary>
/// <param name="Lines">
/// Counting folded lines by what they stand for rather than as one, so this is
/// requests and not rows.
/// </param>
public sealed record CallerSummary(
    string Client,
    string? Place,
    string? Country,
    string? Agent,
    long Lines,
    long Failed,
    DateTime First,
    DateTime Last
);
