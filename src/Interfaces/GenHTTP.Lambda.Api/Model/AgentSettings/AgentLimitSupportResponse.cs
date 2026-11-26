using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Which limits the agent holds a run to, and to what by default.")]
public sealed record AgentLimitSupportResponse(
    [Summary("Whether it stops a run once its time is up - and so whether the operator may set how long")]
    bool Time,
    [Summary("Whether it stops a run once it took as many steps as it may")]
    bool Steps,
    [Summary("How long it gives a run where nobody decided, in seconds; absent for no limit")]
    int? DefaultSeconds,
    [Summary("How many steps it gives a run where nobody decided; absent for no limit")]
    int? DefaultSteps
);
