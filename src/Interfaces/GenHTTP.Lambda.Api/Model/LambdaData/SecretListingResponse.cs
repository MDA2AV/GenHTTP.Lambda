using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The secrets of a lambda, or of a feature's copy of them, by name.")]
public sealed record SecretListingResponse(
    [Summary("Whether the lambda has its secrets switched on (`PUT …/data/secrets`)")]
    bool Enabled,
    [Summary("What is stored, by name")]
    List<SecretResponse> Secrets,
    [Summary("Every name the code reads with `Secret.ReadAsync(\"…\")` or `Secret.ExistsAsync(\"…\")` - or their plain forms - as a literal - in the version online and the newest one, or in the feature's files")]
    List<string> Used,
    [Summary("The names the code reads that have no value, and that it does not ask about with `Secret.ExistsAsync` first: what somebody still has to enter, or the code that reads them fails")]
    List<string> Missing,
    [Summary("The names the code asks about with `Secret.ExistsAsync` that have no value: it does without them")]
    List<string> Optional,
    [Summary("How many secrets a lambda may keep")]
    int Limit
);
