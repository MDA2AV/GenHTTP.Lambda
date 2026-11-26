using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

using GenHTTP.Lambda.Services.Agents;

using GenHTTP.Modules.Mcp;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// What a tool call of a run of the agent is, as a step the pages can say in
/// their visitor's language: what it is about as it begins, and how it went
/// once the tool has answered.
/// </summary>
/// <remarks>
/// <para>
/// The platform records these itself, from the calls that carry a run's
/// credential, so they are the same whichever agent integration runs the
/// run - and one that knows nothing of the platform's tools still has its
/// steps shown in every language. Facts rather than sentences: the kind of
/// step, the files, the version, whether it went online, how many errors.
/// </para>
/// <para>
/// How it went is read off the answer the tool gives - what the agent reads
/// as well - never off what the agent says about it.
/// </para>
/// </remarks>
internal static class RunSteps
{

    private static readonly JsonSerializerOptions Format = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>The tools whose answer says a version - or a feature's preview - went online.</summary>
    private static readonly HashSet<string> Deploying = ["write_code", "change_code", "deploy", "merge_feature"];

    #region Beginning

    public static AgentStep Of(string kind, string? feature = null, IReadOnlyList<string>? files = null, IReadOnlyList<string>? removed = null,
                               bool? whole = null, string? path = null, int? version = null, string? data = null, string? tool = null, string? named = null)
        => new(0, kind, Tool: tool, Files: files is { Count: > 0 } ? files : null, Removed: removed is { Count: > 0 } ? removed : null, Whole: whole,
               Path: path, Version: version, Feature: named, Preview: string.IsNullOrEmpty(feature) ? null : true, Data: data);

    public static IReadOnlyList<string> Names(IEnumerable<string?>? names) => [.. (names ?? []).Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n!).Distinct()];

    #endregion

    #region Finishing

    /// <summary>
    /// The step once the tool answered: done, with what its answer says.
    /// </summary>
    public static AgentStep Finish(string tool, AgentStep step, object answer)
    {
        var (body, error) = Read(answer);

        var done = step with { Done = true };

        if (body == null || (error && body["diagnostics"] is not JsonArray))
        {
            var problem = error ? Problem(answer, body) : null;

            return problem != null ? done with { Problem = Clip(problem, 300) } : done;
        }

        if (body["feature"] is JsonObject feature && Text(feature["name"]) is { } name)
        {
            done = done with { Feature = Clip(name, 80) };
        }

        var ok = Flag(body["ok"]) == true;

        switch (tool)
        {
            case "write_code":
            case "change_code":
            case "deploy":
            case "merge_feature":
                if (Number(body["version"]) is { } version)
                {
                    done = done with { Version = (int)version };
                }

                if (WentOnline(tool, body) || (ok && body["previewUrl"] != null))
                {
                    done = done with { Online = true };
                }

                if (body["diagnostics"] is JsonArray diagnostics)
                {
                    done = done with { Errors = ErrorsIn(diagnostics), Online = Flag(body["ok"]) == false ? false : done.Online };
                }

                break;

            case "create_feature":
                if (body["feature"] is JsonObject started && Number(started["base"]) is { } based)
                {
                    done = done with { Version = (int)based };
                }

                break;

            case "check_code":
                done = done with { Errors = body["diagnostics"] is JsonArray found ? ErrorsIn(found) : 0 };
                break;

            case "read_lambda":
                if (Number(body["version"]) is { } read)
                {
                    done = done with { Version = (int)read };
                }

                break;

            case "read_logs":
                if (body["lines"] is JsonArray lines)
                {
                    done = done with
                    {
                        Problems = lines.Count(l => Text(l?["level"]) is { } level && (level.Contains("error", StringComparison.OrdinalIgnoreCase) || level.Contains("critical", StringComparison.OrdinalIgnoreCase)))
                    };
                }

                break;

            case "read_attachment":
                if (Text(body["name"]) is { } file)
                {
                    done = done with { Files = [file] };
                }

                break;
        }

        return done;
    }

    /// <summary>
    /// Whether an answer says a version went online: <c>online: true</c> in
    /// the answers of a deploy and of a save or a merge with deploy - only
    /// those, because reading a lambda says online about the lambda as it is.
    /// </summary>
    private static bool WentOnline(string tool, JsonNode body)
        => Deploying.Contains(tool) && Flag(body["ok"]) == true && (Flag(body["online"]) == true || body["onlineUntil"] != null);

    private static int ErrorsIn(JsonArray diagnostics)
        => diagnostics.Count(d => Text(d?["severity"]) is { } severity && severity.Contains("error", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// What a tool answered, as data, and whether it answered with an error.
    /// </summary>
    private static (JsonNode? Body, bool Error) Read(object answer)
    {
        try
        {
            if (answer is ToolResult result)
            {
                return (result.Structured == null ? null : JsonSerializer.SerializeToNode(result.Structured, Format), result.IsError);
            }

            return (JsonSerializer.SerializeToNode(answer, Format), false);
        }
        catch (Exception e) when (e is JsonException or NotSupportedException or InvalidOperationException)
        {
            return (null, answer is ToolResult { IsError: true });
        }
    }

    private static string? Problem(object answer, JsonNode? body)
    {
        if (body != null && Text(body["problem"] ?? body["error"]) is { } said)
        {
            return said;
        }

        return answer is ToolResult result ? string.Join(' ', result.Content?.OfType<TextContent>().Select(t => t.Text) ?? []).Trim() is { Length: > 0 } text ? text : null : null;
    }

    private static string? Text(JsonNode? node) => node is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    private static bool? Flag(JsonNode? node) => node is JsonValue value && value.TryGetValue<bool>(out var flag) ? flag : null;

    private static long? Number(JsonNode? node) => node is JsonValue value && value.TryGetValue<long>(out var number) ? number : null;

    private static string Clip(string text, int longest) => text.Length > longest ? text[..longest] + "…" : text;

    #endregion

}
