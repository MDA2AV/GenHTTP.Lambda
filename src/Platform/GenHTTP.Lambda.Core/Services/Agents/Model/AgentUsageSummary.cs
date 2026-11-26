using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Agents;

[Summary("How the agent was used over some days: runs and the time they took, by option - never what they were about.")]
public sealed record AgentUsageSummary(
    [Summary("Over how many days, today included")]
    int Days,
    [Summary("How many runs there were")]
    int Runs,
    [Summary("How many of them built a new app")]
    int Builds,
    [Summary("How long they worked, together, in seconds")]
    long Seconds,
    [Summary("The runs and their time by each value of each option they ran on")]
    IReadOnlyList<OptionUsage> Options
);
