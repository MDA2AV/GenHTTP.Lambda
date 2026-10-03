using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// The last few thousand things this process said, written by the logger and
/// by what the lambdas print, and read by the operator and the owners.
/// </summary>
public interface ILogBook
{

    /// <summary>
    /// How many lines are kept before the oldest is dropped.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Everything ever written, including what has since been dropped.
    /// </summary>
    long Written { get; }

    /// <summary>
    /// Writes a line and returns the sequence it was given, or zero where it
    /// was folded into one already written.
    /// </summary>
    long Append(string level, string source, string? lambda, string text, string? detail = null,
                string? client = null, string? agent = null, string? country = null, string? place = null,
                string? folding = null, long? lambdaId = null, string? domain = null, long? featureId = null);

    /// <summary>
    /// The lines written after a sequence, the sequence to ask from next, and
    /// how many fell out of the ring or over the limit before the reader got
    /// to them.
    /// </summary>
    (IReadOnlyList<LogLine> Lines, long Cursor, int Missed) Read(long since, string? lambda, LogLevel minimum, int limit,
                                                                 string? client = null, long? lambdaId = null,
                                                                 FeatureLines? feature = null);

    /// <summary>
    /// Every caller the ring still holds, with what they have been doing.
    /// </summary>
    IReadOnlyList<CallerSummary> Callers(int limit = 500);

}
