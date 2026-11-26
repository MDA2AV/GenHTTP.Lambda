using System.Text.Json.Nodes;

using GenHTTP.Lambda.Integrations.Agents;

namespace GenHTTP.Lambda.Integrations.ClaudeCode.Provider;

/// <summary>
/// What a runner from before it streamed its runs answers when it is asked
/// how one is getting on (<c>GET /build/{id}</c>) - the whole of it each
/// time - as the events of the contract.
/// </summary>
/// <remarks>
/// <para>
/// The same answer always makes the same events with the same positions, so
/// a platform that asks again after a restart, from the last position it
/// recorded, is given only what came after it: the queue is position 1, the
/// start 2, the n-th step 100 + 2n as it begins and one more once it is done,
/// the end the last of all.
/// </para>
/// <para>
/// Such a runner writes down every tool call as a step of its own, those of
/// the platform's tools included. The platform records those itself, from
/// the calls that carry the run's credential, so only what the runner alone
/// saw is told: what the agent said, the pages it read, and the tools that
/// are not the platform's.
/// </para>
/// </remarks>
internal static class RunnerSnapshots
{

    private const long Queued = 1, Started = 2, Steps = 100, Ended = long.MaxValue - 1;

    /// <summary>The kinds of step of the platform's own tools, which the platform records itself.</summary>
    private static readonly HashSet<string> Platforms =
    [
        "guide", "demos", "read", "logs", "create", "write", "check", "deploy", "upload", "delete", "list", "feature", "update",
        "merge", "discard", "data", "records", "secrets", "connectivity", "analytics", "source"
    ];

    /// <summary>
    /// The events an answer makes, in order.
    /// </summary>
    public static IEnumerable<AgentEvent> EventsOf(JsonNode job, DateTimeOffset now)
    {
        var state = Text(job["state"]);

        var seconds = Number(job["seconds"]) ?? 0;

        var started = now - TimeSpan.FromSeconds(seconds);

        yield return new AgentQueued(Queued, now, (int)Math.Max(1, Number(job["waiting"]) ?? 1));

        if (state == "queued")
        {
            yield break;
        }

        yield return new AgentStarted(Started, started);

        var steps = job["steps"] as JsonArray ?? [];

        for (var i = 0; i < steps.Count; i++)
        {
            if (steps[i] is not JsonObject step)
            {
                continue;
            }

            var at = started + TimeSpan.FromSeconds(Number(step["at"]) ?? 0);
            var kind = Text(step["kind"]) ?? "other";

            var begun = Steps + 2 * i;

            if (kind == "say")
            {
                if (Text(step["text"]) is { Length: > 0 } text)
                {
                    yield return new AgentSaid(begun, at, text);
                }

                continue;
            }

            if (Platforms.Contains(kind))
            {
                continue;
            }

            var activity = kind == "web"
                ? new AgentActivity(begun, at, $"step-{i}", AgentActivityKind.ReadsPage, Text(step["site"]))
                : new AgentActivity(begun, at, $"step-{i}", AgentActivityKind.Other, Text(step["tool"]) ?? kind);

            yield return activity;

            if (Flag(step["done"]) == true)
            {
                yield return activity with { Sequence = begun + 1, Done = true, Failed = Text(step["problem"]) != null };
            }
        }

        if (state is "done" or "failed" or "cancelled")
        {
            yield return EndOf(job, state, now);
        }
    }

    /// <summary>
    /// How a run ended, from what such a runner said of it - the build's
    /// result or the change's - by the reasons it names.
    /// </summary>
    private static AgentEnded EndOf(JsonNode job, string state, DateTimeOffset now)
    {
        var result = job["result"];

        var reason = Text(result?["reason"]);

        var summary = Text(result?["summary"]);
        var error = Text(result?["error"]);
        var detail = Text(result?["detail"]) ?? error;

        if (state == "cancelled" || Flag(result?["cancelled"]) == true)
        {
            return new AgentEnded(Ended, now, AgentEnding.Cancelled, summary);
        }

        if (Flag(result?["declined"]) == true || reason == "declined")
        {
            // a build said why in its error, a change in its summary
            return new AgentEnded(Ended, now, AgentEnding.Declined, summary ?? error);
        }

        var ending = reason switch
        {
            "timeout" => AgentEnding.TimedOut,
            "turns" => AgentEnding.OutOfSteps,
            "unauthorised" => AgentEnding.SignInFailed,
            _ when error != null && error.Contains("sign in", StringComparison.OrdinalIgnoreCase) => AgentEnding.SignInFailed,
            _ when state == "failed" && summary == null => AgentEnding.Failed,
            _ => AgentEnding.Completed
        };

        return new AgentEnded(Ended, now, ending, summary, ending == AgentEnding.Completed ? null : detail);
    }

    private static string? Text(JsonNode? node) => node is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    private static bool? Flag(JsonNode? node) => node is JsonValue value && value.TryGetValue<bool>(out var flag) ? flag : null;

    private static long? Number(JsonNode? node) => node is JsonValue value && value.TryGetValue<long>(out var number) ? number : null;

}
