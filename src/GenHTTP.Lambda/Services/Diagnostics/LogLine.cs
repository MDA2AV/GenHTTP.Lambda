namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// One line the server or a lambda has said.
/// </summary>
/// <param name="Seq">
/// Counts from one and never repeats. A reader asks for everything after the
/// last one it saw, which is what makes following the tail a poll rather than
/// a diff.
/// </param>
/// <param name="Source">
/// Where it came from: the last part of a logger category for the server,
/// <c>stdout</c> or <c>stderr</c> for a lambda.
/// </param>
/// <param name="Lambda">
/// The public key this belongs to, or nothing for the server itself. A server
/// line written while a lambda was being served carries its key too, so the
/// warning about a lambda that threw appears under that lambda as well as in
/// the general run.
/// </param>
/// <param name="Detail">
/// The stack trace, when there is one. Kept apart from the text so the list
/// stays one line per line and the trace is something you open.
/// </param>
public sealed record LogLine(
    long Seq,
    DateTime At,
    string Level,
    string Source,
    string? Lambda,
    string Text,
    string? Detail
);
