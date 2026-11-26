using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

using GenHTTP.Lambda.Integrations.Agents;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Integrations.ClaudeCode.Provider;

/// <summary>
/// Speaks to the runner of the platform's own agent (<c>docker/agent/builder.mjs</c>):
/// hands it runs, follows them and stops them.
/// </summary>
/// <remarks>
/// <para>
/// The runner keeps its jobs in memory and runs each in a throwaway
/// container of its own, Claude Code with the platform's MCP and nothing
/// else; its health says what it can do (<see cref="RunnerHealth"/>), and
/// that is what this offers. A runner of this protocol streams what its runs
/// do; one from before is asked how a run is getting on every so often, and
/// what it answers is told as the events it stands for
/// (<see cref="RunnerSnapshots"/>). Either way the platform is pushed the
/// events, and an older runner goes on working after the platform was
/// upgraded - as a newer one goes on working for an older platform, whose
/// routes it still answers.
/// </para>
/// <para>
/// The runner is configured with where each installation it builds for is,
/// and connects a run there whatever it is told - so this hands it neither
/// the platform's addresses nor which header carries the credential, which
/// it knows. What it is handed is never logged: it carries the editor key
/// and the credential.
/// </para>
/// </remarks>
internal sealed class ClaudeCodeAgent : IAgentProvider
{

    /// <summary>
    /// How a job is written for the runner: the words as they are, rather
    /// than every letter outside ASCII as six characters of an escape.
    /// </summary>
    /// <remarks>
    /// The runner takes a body of a bounded size, and a request in Japanese
    /// with the conversation before it came to several times that once
    /// escaped, and was dropped.
    /// </remarks>
    private static readonly JsonSerializerOptions Sent = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>How long the panel waits for the runner to say how it is.</summary>
    private static readonly TimeSpan HealthTimeout = TimeSpan.FromSeconds(3);

    /// <summary>How long what the runner said of itself is taken as it is before it is asked again.</summary>
    private static readonly TimeSpan HealthAge = TimeSpan.FromMinutes(1);

    private readonly HttpClient _calls;

    private readonly HttpClient _streams;

    private readonly Uri _url;

    private readonly TimeSpan _poll;

    private readonly ILogger _logger;

    private volatile RunnerHealth? _health;

    private DateTime _asked;

    private int _asking;

    public ClaudeCodeAgent(Uri url, string? token, TimeSpan poll, ILogger logger)
    {
        _url = url;
        _poll = poll;
        _logger = logger;

        // a run is minutes of work, but every call here either starts one,
        // stops one or asks after it, and those are instant
        _calls = new HttpClient { BaseAddress = url, Timeout = TimeSpan.FromSeconds(20) };

        // what a run does is streamed for as long as it runs
        _streams = new HttpClient { BaseAddress = url, Timeout = Timeout.InfiniteTimeSpan };

        if (token != null)
        {
            _calls.DefaultRequestHeaders.Add("X-Agent-Token", token);
            _streams.DefaultRequestHeaders.Add("X-Agent-Token", token);
        }
    }

    #region Describing it

    /// <summary>
    /// Asks the runner what it can do, once; one that does not answer now is
    /// asked again later, and keeps nothing from starting.
    /// </summary>
    public async ValueTask PrepareAsync(CancellationToken cancellation = default)
    {
        await RefreshAsync(cancellation);
    }

    public IntegrationInfo Describe() => new(IntegrationKind.Agent, "Claude Code runner",
                                             [
                                                 new IntegrationDetail("Runner", _url.ToString().TrimEnd('/')),
                                                 new IntegrationDetail("Protocol", (_health?.Protocol ?? 1).ToString(CultureInfo.InvariantCulture))
                                             ]);

    /// <remarks>
    /// The agent reads the guide like any other client; it adds nothing to it.
    /// </remarks>
    public string? Guide(GuideContext context) => null;

    public AgentCapabilities Capabilities
    {
        get
        {
            var health = Health();

            var v2 = health.Protocol >= 2;

            var destinations = new List<AgentDestination>
            {
                new("Anthropic", "api.anthropic.com",
                    "What was asked, the conversation before it and the files attached, the platform's guide, and whatever the agent reads through the platform - the app's code, documentation, records and logs - to run the model on.")
            };

            if (health.Web != AgentWebScope.None)
            {
                destinations.Add(new AgentDestination("Web sites", health.Web == AgentWebScope.Any ? "any public site" : "the sites a request names",
                                                      "Requests for their pages, GET and HEAD only, made by the runner's proxy; what was asked is not sent there."));
            }

            if (health.Searches)
            {
                destinations.Add(new AgentDestination("Anthropic's web search", "api.anthropic.com", "What the agent searches the web for."));
            }

            return new AgentCapabilities(
                new AgentDisclosure("Claude Code", "Anthropic", destinations),
                RunnerCatalog.Options(health.Models?.Keys.ToList(), health.Efforts),
                v2 ? health.Attachments : null,
                v2 && health.Files,
                new AgentWebAccess(health.Web, health.Searches,
                                   health.Web == AgentWebScope.None ? null : "GET and HEAD only, to public addresses, made by the runner's proxy"),
                Cancels: true,
                Asks: v2 && health.Asks,
                new AgentLimitSupport(v2, v2, new AgentLimits(TimeSpan.FromSeconds(health.Timeout ?? 600), health.MaxTurns ?? 60)));
        }
    }

    /// <summary>
    /// What the runner said of itself last, asked again in the background
    /// once it is old - so reading it never waits for the runner.
    /// </summary>
    private RunnerHealth Health()
    {
        if (DateTime.UtcNow - _asked > HealthAge && Interlocked.Exchange(ref _asking, 1) == 0)
        {
            // asked beside the request that read it, which does not wait for it
#pragma warning disable RS0030
            _ = Task.Run(async () =>
            {
                try
                {
                    await RefreshAsync(CancellationToken.None);
                }
                finally
                {
                    Interlocked.Exchange(ref _asking, 0);
                }
            });
#pragma warning restore RS0030
        }

        return _health ?? RunnerHealth.Unreachable;
    }

    private async ValueTask<RunnerHealth> RefreshAsync(CancellationToken cancellation)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

        timeout.CancelAfter(HealthTimeout);

        RunnerHealth health;

        try
        {
            using var response = await _calls.GetAsync("health", timeout.Token);

            health = response.IsSuccessStatusCode ? RunnerHealth.Of(await ReadNodeAsync(response, timeout.Token)) : RunnerHealth.Unreachable;
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException && !cancellation.IsCancellationRequested)
        {
            health = RunnerHealth.Unreachable;
        }

        _asked = DateTime.UtcNow;

        // one that answered once is offered as it was while it does not answer,
        // rather than as one that can do nothing
        if (health.Reachable || _health == null)
        {
            _health = health;
        }

        return health;
    }

    public async ValueTask<AgentLoad> LoadAsync(CancellationToken cancellation = default)
    {
        var health = await RefreshAsync(cancellation);

        if (!health.Reachable)
        {
            return AgentLoad.Unreachable;
        }

        var facts = new List<IntegrationDetail>
        {
            new("Signs in with", health.SignsInWith switch
            {
                "subscription" => "a Claude subscription",
                "api-key" => "an API key",
                _ => "nothing - every run fails to sign in"
            })
        };

        if (health.Cli is { } cli)
        {
            facts.Add(new IntegrationDetail("Claude Code", cli));
        }

        foreach (var (name, model) in health.Models ?? new Dictionary<string, string>())
        {
            facts.Add(new IntegrationDetail($"Model {name}", model));
        }

        facts.Add(new IntegrationDetail("Reads pages", health.Web switch
        {
            AgentWebScope.Any => "of any public site",
            AgentWebScope.NamedSites => "of the sites a request names",
            _ => "of no site"
        }));

        if (health.Installations is { } installations)
        {
            facts.Add(new IntegrationDetail("Installations", installations.ToString(CultureInfo.InvariantCulture)));
        }

        // a runner from before several builds ran at once does not say, and runs one
        return new AgentLoad(true, health.Concurrency, health.Running, health.Queued, facts);
    }

    #endregion

    #region Runs

    public async ValueTask<AgentAcceptance> StartAsync(AgentRun run, CancellationToken cancellation = default)
    {
        var health = _health ?? await RefreshAsync(cancellation);

        var v2 = health.Protocol >= 2;

        var tag = run.Lambda is { } lambda ? TagOf(lambda.EditorKey) : null;

        run.Options.TryGetValue(RunnerCatalog.Model, out var model);
        run.Options.TryGetValue(RunnerCatalog.Effort, out var effort);

        var job = new JsonObject
        {
            ["prompt"] = run.Message,
            ["model"] = model,
            ["effort"] = effort,
            // a runner from before knows a run without a clock only as this
            ["unbounded"] = run.Limits.Unbounded,
            ["ticket"] = run.Access.Credential,
            // what a runner of this release files the job under; one from
            // before files it under an id of its own, which it answers with
            ["run"] = run.Id
        };

        if (run.Lambda is { } target)
        {
            job["key"] = target.EditorKey;
            job["lambda"] = tag;
            job["deploy"] = target.PutOnline;
            job["before"] = target.Online;
            job["feature"] = target.Draft;

            if (run.Conversation.Count > 0)
            {
                job["history"] = History(run.Conversation);
            }
        }

        job["language"] = run.Audience.Language;

        if (v2)
        {
            job["protocol"] = 2;
            job["run"] = run.Id;
            job["view"] = run.Audience.View == AgentView.Simple ? "simple" : "full";
            job["installation"] = run.Audience.Installation switch
            {
                AgentInstallationKind.Public => "public",
                AgentInstallationKind.Enterprise => "enterprise",
                _ => "home"
            };
            job["installationName"] = run.Audience.InstallationName;
            job["timeout"] = run.Limits.Time is { } time ? (int)time.TotalSeconds : null;
            job["maxTurns"] = run.Limits.Steps;
            job["attachments"] = new JsonArray([.. run.Attachments.Select(a => (JsonNode)new JsonObject
            {
                ["id"] = a.Id,
                ["name"] = a.Name,
                ["type"] = a.MediaType,
                ["size"] = a.Size,
                ["earlier"] = a.Earlier
            })]);

            if (run.Task == AgentTask.Build && run.Conversation.Count > 0)
            {
                job["history"] = History(run.Conversation);
            }

            if (run.Continues is { } continues)
            {
                job["continues"] = new JsonObject
                {
                    ["kind"] = continues.Kind switch
                    {
                        AgentContinuationKind.Answer => "answer",
                        AgentContinuationKind.Unblocked => "unblocked",
                        _ => "retry"
                    },
                    ["asked"] = continues.Asked,
                    ["question"] = continues.Question,
                    ["done"] = continues.Done
                };
            }
        }

        try
        {
            using var response = await _calls.PostAsJsonAsync("build", job, Sent, cancellation);

            var body = await ReadNodeAsync(response, cancellation);

            if (!response.IsSuccessStatusCode)
            {
                var said = body?["error"] is JsonValue error && error.TryGetValue<string>(out var message) ? message : "The agent would not take that.";

                throw new AgentRefusedException(response.StatusCode switch
                {
                    HttpStatusCode.BadRequest => AgentRefusal.Invalid,
                    HttpStatusCode.ServiceUnavailable or HttpStatusCode.Conflict or HttpStatusCode.TooManyRequests => AgentRefusal.Busy,
                    _ => AgentRefusal.Unavailable
                }, said);
            }

            var id = body?["id"] is JsonValue taken && taken.TryGetValue<string>(out var named) ? named : run.Id;

            var position = body?["queued"] is JsonValue place && place.TryGetValue<int>(out var queued) ? queued : 1;

            // a change is stopped by the tag it is filed under, which the
            // platform keeps with the reference
            return new AgentAcceptance(Math.Max(0, position), tag != null ? $"{id}|{tag}" : id);
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning(e, "Failed to reach the agent's runner");

            throw new AgentRefusedException(AgentRefusal.Unavailable, "The agent is not answering. Try again in a moment.", e);
        }
        catch (TaskCanceledException e) when (!cancellation.IsCancellationRequested)
        {
            throw new AgentRefusedException(AgentRefusal.Unavailable, "The agent is not answering. Try again in a moment.", e);
        }
    }

    public IAsyncEnumerable<AgentEvent> FollowAsync(AgentRunHandle run, long after, CancellationToken cancellation = default)
        => (_health?.Protocol ?? 1) >= 2 ? StreamAsync(run, after, cancellation) : PollAsync(run, after, cancellation);

    /// <summary>
    /// What a runner of this protocol streams of a run, line by line, as it
    /// happens.
    /// </summary>
    private async IAsyncEnumerable<AgentEvent> StreamAsync(AgentRunHandle run, long after, [EnumeratorCancellation] CancellationToken cancellation)
    {
        var (id, _) = Split(run);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"build/{id}/events?after={after.ToString(CultureInfo.InvariantCulture)}");

        using var response = await _streams.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            yield return new AgentEnded(long.MaxValue, DateTimeOffset.UtcNow, AgentEnding.Lost);
            yield break;
        }

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellation);

        using var reader = new StreamReader(stream, Encoding.UTF8);

        while (await reader.ReadLineAsync(cancellation) is { } line)
        {
            if (line.Length == 0)
            {
                continue;
            }

            JsonNode? parsed;

            try
            {
                parsed = JsonNode.Parse(line);
            }
            catch (JsonException)
            {
                continue;
            }

            if (RunnerEvents.Of(parsed) is { } happened)
            {
                yield return happened;

                if (happened is AgentEnded)
                {
                    yield break;
                }
            }
        }
    }

    /// <summary>
    /// What a runner from before answers of a run, asked every so often until
    /// it ended - told as the events it stands for, each once.
    /// </summary>
    private async IAsyncEnumerable<AgentEvent> PollAsync(AgentRunHandle run, long after, [EnumeratorCancellation] CancellationToken cancellation)
    {
        var (id, _) = Split(run);

        var told = after;

        while (!cancellation.IsCancellationRequested)
        {
            using var response = await _calls.GetAsync($"build/{id}", cancellation);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                yield return new AgentEnded(long.MaxValue, DateTimeOffset.UtcNow, AgentEnding.Lost);
                yield break;
            }

            response.EnsureSuccessStatusCode();

            var job = await ReadNodeAsync(response, cancellation);

            if (job != null)
            {
                foreach (var happened in RunnerSnapshots.EventsOf(job, DateTimeOffset.UtcNow))
                {
                    if (happened.Sequence <= told)
                    {
                        continue;
                    }

                    told = happened.Sequence;

                    yield return happened;

                    if (happened is AgentEnded)
                    {
                        yield break;
                    }
                }
            }

            await Task.Delay(_poll, cancellation);
        }
    }

    public async ValueTask<bool> CancelAsync(AgentRunHandle run, CancellationToken cancellation = default)
    {
        var (id, tag) = Split(run);

        try
        {
            using var response = await _calls.DeleteAsync($"build/{id}?lambda={tag}", cancellation);

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            throw new AgentRefusedException(AgentRefusal.Unavailable, "The agent is not answering.");
        }
    }

    #endregion

    #region Helpers

    private static JsonArray History(IReadOnlyList<AgentTurn> conversation)
        => [.. conversation.Select(t => (JsonNode)new JsonObject { ["prompt"] = t.Message, ["reply"] = t.Reply })];

    /// <summary>
    /// The runner's id of a run and the tag it filed a change under, as the
    /// reference keeps them.
    /// </summary>
    private static (string Id, string Tag) Split(AgentRunHandle run)
    {
        var reference = run.Reference ?? run.Id;

        var bar = reference.IndexOf('|', StringComparison.Ordinal);

        return bar < 0 ? (reference, "") : (reference[..bar], reference[(bar + 1)..]);
    }

    /// <summary>
    /// What the runner files the changes of a lambda under, so that one runs
    /// at a time: a number, as it takes one, that stands for the lambda's
    /// editor key - which does not change when its address does.
    /// </summary>
    private static string TagOf(string editorKey)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(editorKey));

        var number = BitConverter.ToUInt64(hash, 0) & 0x00FF_FFFF_FFFF_FFFF;

        return number.ToString(CultureInfo.InvariantCulture);
    }

    private static async ValueTask<JsonNode?> ReadNodeAsync(HttpResponseMessage response, CancellationToken cancellation)
    {
        try
        {
            return JsonNode.Parse(await response.Content.ReadAsStringAsync(cancellation));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public ValueTask DisposeAsync()
    {
        _calls.Dispose();
        _streams.Dispose();

        return ValueTask.CompletedTask;
    }

    #endregion

}
