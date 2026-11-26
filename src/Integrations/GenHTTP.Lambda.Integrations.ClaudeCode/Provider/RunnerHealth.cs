using System.Text.Json.Nodes;

using GenHTTP.Lambda.Integrations.Agents;

namespace GenHTTP.Lambda.Integrations.ClaudeCode.Provider;

/// <summary>
/// What the runner says of itself (<c>GET /health</c>), as far as it says it.
/// </summary>
/// <remarks>
/// A runner of an earlier release says less: one from before the models
/// were chosen answers with a list of them rather than a map, one from
/// before several builds ran at once a flag rather than a count, and one
/// from before this protocol (<see cref="Protocol"/> one) takes no files,
/// asks nothing back, hands nothing back and streams nothing - it is asked
/// as it always was.
/// </remarks>
internal sealed record RunnerHealth(
    bool Reachable,
    int Protocol = 1,
    string? SignsInWith = null,
    IReadOnlyDictionary<string, string>? Models = null,
    IReadOnlyList<string>? Efforts = null,
    string? Cli = null,
    int Running = 0,
    int Queued = 0,
    AgentWebScope Web = AgentWebScope.None,
    bool Searches = false,
    int? Concurrency = null,
    int? Installations = null,
    int? Timeout = null,
    int? MaxTurns = null,
    AgentAttachmentPolicy? Attachments = null,
    bool Files = false,
    bool Asks = false)
{

    public static readonly RunnerHealth Unreachable = new(false);

    /// <summary>
    /// What a runner answered, read as far as it says anything.
    /// </summary>
    public static RunnerHealth Of(JsonNode? body)
    {
        var models = body?["models"] is JsonObject map
            ? map.Where(m => m.Value is JsonValue).ToDictionary(m => m.Key, m => m.Value!.ToString())
            : body?["models"] is JsonArray names
                ? names.OfType<JsonValue>().ToDictionary(n => n.ToString(), n => n.ToString())
                : null;

        var efforts = body?["efforts"] is JsonArray list ? list.OfType<JsonValue>().Select(e => e.ToString()).ToList() : null;

        // a runner from before named only whether it fetched the named sites
        var web = Text(body?["web"]) switch
        {
            "any" => AgentWebScope.Any,
            "named" => AgentWebScope.NamedSites,
            "off" => AgentWebScope.None,
            _ => Flag(body?["fetch"]) == true ? AgentWebScope.NamedSites : AgentWebScope.None
        };

        AgentAttachmentPolicy? attachments = null;

        if (body?["attachments"] is JsonObject taken && taken["types"] is JsonArray types && Number(taken["maxBytes"]) is { } bytes
            && Number(taken["maxCount"]) is { } count)
        {
            attachments = new AgentAttachmentPolicy([.. types.OfType<JsonValue>().Select(t => t.ToString())], bytes, (int)count);
        }

        return new RunnerHealth(
            true,
            Number(body?["protocol"]) is { } protocol ? (int)protocol : 1,
            Text(body?["auth"]),
            models,
            efforts,
            Text(body?["cli"]),
            Count(body?["quick"], "running") + Count(body?["long"], "running"),
            Count(body?["quick"], "queued") + Count(body?["long"], "queued"),
            web,
            Flag(body?["search"]) == true,
            Number(body?["concurrency"]) is { } concurrency and > 0 ? (int)concurrency : null,
            Number(body?["installations"]) is { } installations and > 0 ? (int)installations : null,
            Number(body?["timeout"]) is { } timeout and > 0 ? (int)timeout : null,
            Number(body?["maxTurns"]) is { } turns and > 0 ? (int)turns : null,
            attachments,
            Flag(body?["files"]) == true,
            Flag(body?["asks"]) == true);
    }

    private static string? Text(JsonNode? node) => node is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    private static bool? Flag(JsonNode? node) => node is JsonValue value && value.TryGetValue<bool>(out var flag) ? flag : null;

    private static long? Number(JsonNode? node) => node is JsonValue value && value.TryGetValue<long>(out var number) ? number : null;

    private static int Count(JsonNode? lane, string field)
        => lane?[field] is JsonValue value && value.TryGetValue<int>(out var number) ? number
         : lane?[field] is JsonValue flag && flag.TryGetValue<bool>(out var on) && on ? 1 : 0;

}
