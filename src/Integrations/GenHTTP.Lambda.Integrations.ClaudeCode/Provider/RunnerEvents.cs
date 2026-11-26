using System.Globalization;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Integrations.Agents;

namespace GenHTTP.Lambda.Integrations.ClaudeCode.Provider;

/// <summary>
/// One line of what a runner of this protocol streams of a run
/// (<c>GET /build/{id}/events?after={position}</c>), as an event of the
/// contract - or nothing for a line it does not know, which a later runner
/// may well send.
/// </summary>
/// <remarks>
/// A line is a JSON object with its position (<c>seq</c>), when it happened
/// (<c>at</c>) and what (<c>type</c>): <c>queued</c> with its
/// <c>position</c>, <c>started</c>, <c>waiting</c> with a <c>reason</c>,
/// <c>until</c> and <c>detail</c>, <c>said</c> with <c>text</c>,
/// <c>activity</c> with an <c>id</c>, its <c>kind</c> (<c>page</c>,
/// <c>search</c>, <c>other</c>), <c>subject</c>, <c>done</c> and
/// <c>failed</c>, <c>returned</c> with a <c>name</c>, a <c>type</c> of file
/// (<c>mediaType</c>), its <c>content</c> in base64 and a
/// <c>description</c>, <c>spent</c> with <c>inputTokens</c>,
/// <c>outputTokens</c>, <c>cost</c> and <c>currency</c>, and <c>ended</c>
/// with its <c>ending</c>, <c>message</c> and <c>detail</c>.
/// </remarks>
internal static class RunnerEvents
{

    public static AgentEvent? Of(JsonNode? line)
    {
        if (line is not JsonObject happened || Number(happened["seq"]) is not { } sequence)
        {
            return null;
        }

        var at = Text(happened["at"]) is { } text && DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed
            : DateTimeOffset.UtcNow;

        return Text(happened["type"]) switch
        {
            "queued" => new AgentQueued(sequence, at, (int)(Number(happened["position"]) ?? 1)),
            "started" => new AgentStarted(sequence, at),
            "waiting" => new AgentWaiting(sequence, at, WaitOf(Text(happened["reason"])),
                                          Text(happened["until"]) is { } until && DateTimeOffset.TryParse(until, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var when) ? when : null,
                                          Text(happened["detail"])),
            "said" when Text(happened["text"]) is { } said => new AgentSaid(sequence, at, said),
            "activity" when Text(happened["id"]) is { } id => new AgentActivity(sequence, at, id, ActivityOf(Text(happened["kind"])), Text(happened["subject"]),
                                                                                 Flag(happened["done"]) == true, Flag(happened["failed"]) == true),
            "returned" when Text(happened["name"]) is { } name && Content(happened["content"]) is { } content
                => new AgentReturned(sequence, at, new AgentFile(name, Text(happened["mediaType"]) ?? "application/octet-stream", content, Text(happened["description"]))),
            "spent" => new AgentSpent(sequence, at, new AgentUsage(Number(happened["inputTokens"]), Number(happened["outputTokens"]),
                                                                   Decimal(happened["cost"]), Text(happened["currency"]))),
            "ended" => new AgentEnded(sequence, at, EndingOf(Text(happened["ending"])), Text(happened["message"]), Text(happened["detail"])),
            _ => null
        };
    }

    private static AgentWaitReason WaitOf(string? reason) => reason switch
    {
        "overloaded" => AgentWaitReason.Overloaded,
        "rate-limited" => AgentWaitReason.RateLimited,
        "preparing" => AgentWaitReason.Preparing,
        _ => AgentWaitReason.Other
    };

    private static AgentActivityKind ActivityOf(string? kind) => kind switch
    {
        "page" => AgentActivityKind.ReadsPage,
        "search" => AgentActivityKind.Searches,
        _ => AgentActivityKind.Other
    };

    /// <summary>
    /// An ending by the name the runner gives it; one this does not know is
    /// a failure, which claims nothing.
    /// </summary>
    private static AgentEnding EndingOf(string? ending) => ending switch
    {
        "completed" => AgentEnding.Completed,
        "asked" => AgentEnding.Asked,
        "declined" => AgentEnding.Declined,
        "cancelled" => AgentEnding.Cancelled,
        "timed-out" => AgentEnding.TimedOut,
        "out-of-steps" => AgentEnding.OutOfSteps,
        "sign-in-failed" => AgentEnding.SignInFailed,
        "lost" => AgentEnding.Lost,
        _ => AgentEnding.Failed
    };

    private static ReadOnlyMemory<byte>? Content(JsonNode? node)
    {
        if (Text(node) is not { } base64)
        {
            return null;
        }

        try
        {
            return Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static string? Text(JsonNode? node) => node is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    private static bool? Flag(JsonNode? node) => node is JsonValue value && value.TryGetValue<bool>(out var flag) ? flag : null;

    private static long? Number(JsonNode? node) => node is JsonValue value && value.TryGetValue<long>(out var number) ? number : null;

    private static decimal? Decimal(JsonNode? node) => node is JsonValue value && value.TryGetValue<decimal>(out var number) ? number : null;

}
