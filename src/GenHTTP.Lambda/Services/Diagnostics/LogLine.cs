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
/// <param name="Client">
/// Who was being answered when this was said. The peer address, or where the
/// request came through a proxy that said so, the address it claimed followed
/// by the peer it actually arrived from - a claim and its provenance, since
/// the header is written by whoever sent it.
/// </param>
/// <param name="Agent">
/// What they said they were. Worth as much as any other thing a client says
/// about itself, which is to say it tells a crawler from a browser and proves
/// nothing.
/// </param>
/// <param name="Country">
/// The two letter code the caller's range is registered under, where it is
/// known. The registration of the range, not the location of the person - see
/// <see cref="GeoTable"/>.
/// </param>
/// <param name="Repeats">
/// How many identical lines this one stands for. One is itself alone.
/// </param>
/// <param name="LambdaId">
/// The identity of the lambda the line belongs to, beside its public key. The
/// key is what a person reads; this is what an owner's view filters by. A key
/// can be given up and claimed by somebody else, and lines written under it
/// before that belong to the lambda that wrote them, not to whoever holds the
/// name now.
/// </param>
/// <param name="Domain">
/// The lambda's own domain the request was addressed to, when it was not the
/// platform. Without it a request line of a lambda reached at its domain
/// reads like one of the platform's paths.
/// </param>
public sealed record LogLine(
    long Seq,
    DateTime At,
    string Level,
    string Source,
    string? Lambda,
    string Text,
    string? Detail,
    string? Client = null,
    string? Agent = null,
    string? Country = null,
    string? Place = null,
    int Repeats = 1,
    long? LambdaId = null,
    string? Domain = null
);

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
