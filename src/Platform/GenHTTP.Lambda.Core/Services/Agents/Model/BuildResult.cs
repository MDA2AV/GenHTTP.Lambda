using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

[Summary("How a build ended.")]
public sealed record BuildResult(
    [Summary("Whether a lambda was made")]
    bool Ok,
    [Summary("Whether it is online")]
    bool? Deployed,
    [Summary("The public key of the lambda that was made")]
    string? PublicKey,
    [Summary("Its editor key - handed out here, to whoever asked for the build, and nowhere else")]
    string? PrivateKey,
    [Summary("Where it answers")]
    string? Url,
    [Summary("Where its editor is")]
    string? EditorUrl,
    [Summary("What the agent said it made")]
    string? Summary,
    [Summary("What the person needs to be told about keeping the editor key")]
    string? Keep,
    [Summary("What went wrong, or - where it was declined - why, in the language of the request")]
    string? Error,
    [Summary("What went wrong, in the words of whatever it went wrong in")]
    string? Detail,
    [Summary("Whether the agent would not build what was asked for, because it is not an application or not one this platform is for; `error` says why")]
    bool? Declined = null,
    [Summary("Whether the agent needs to know something before it goes on: `question` is what it asks, and the next message to the app - or a new build - the answer")]
    bool? Asked = null,
    [Summary("What the agent asks back, where it does")]
    string? Question = null,
    [Summary("What the app waits for, and whom - from the platform's records, not the agent's words")]
    IReadOnlyList<RunBlocker>? WaitsFor = null,
    [Summary("What cut it short: `timeout`, `turns` (it took as many steps as it may), `unauthorised` (the agent could not sign in), `lost` (the agent forgot it), `cancelled` or `declined`")]
    string? Reason = null
);
