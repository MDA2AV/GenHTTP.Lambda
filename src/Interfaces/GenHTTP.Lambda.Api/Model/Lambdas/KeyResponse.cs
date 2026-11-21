using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Everything anybody may know about a public key.")]
public sealed record KeyResponse(
    [Summary("The key as it would be stored: trimmed and lower cased")]
    string PublicKey,
    [Summary("Whether it is spelled as a key may be: 3 to 40 characters of a-z, 0-9 and dashes, not starting or ending with a dash")]
    bool Valid,
    [Summary("Whether a new lambda could be created with it right now")]
    bool Available,
    [Summary("Whether a lambda has it")]
    bool Exists,
    [Summary("Whether that lambda is online")]
    bool Deployed,
    [Summary("Why it cannot be claimed, where it cannot - spelling, a reserved name, a key kept for the demos")]
    string? Reason
);
