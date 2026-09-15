using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// Hands a sentence somebody typed to the build agent, and reports back.
/// </summary>
/// <remarks>
/// The agent is a container of its own with no route off the machine and no
/// tools beyond this server's own MCP, which is the whole reason it is safe
/// to point a public text box at it. This class is the only thing here that
/// talks to it: it holds the shared secret, counts how many builds an address
/// has asked for today, and does nothing else.
///
/// Off unless LambdaOptions.AgentUrl is set, so an installation without an
/// agent simply does not have the feature rather than having a broken one.
/// </remarks>
public sealed class BuildService : IDisposable
{
    private readonly LambdaOptions _options;
    private readonly ILogger<BuildService> _logger;
    private readonly HttpClient? _client;

    private readonly ConcurrentDictionary<IPAddress, Tally> _asked = [];

    public BuildService(LambdaOptions options, ILogger<BuildService> logger)
    {
        _options = options;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(options.AgentUrl))
        {
            return;
        }

        _client = new HttpClient
        {
            BaseAddress = new Uri(options.AgentUrl.TrimEnd('/') + "/"),
            // a build is minutes of work, but every call here either starts
            // one or asks after it, and both of those are instant
            Timeout = TimeSpan.FromSeconds(20)
        };

        if (!string.IsNullOrWhiteSpace(options.AgentToken))
        {
            _client.DefaultRequestHeaders.Add("X-Agent-Token", options.AgentToken);
        }
    }

    /// <summary>Whether this installation has an agent to build with.</summary>
    public bool Available => _client != null;

    /// <summary>How many builds one address is allowed in a day.</summary>
    public int PerDay => _options.AgentBuildsPerDay;

    #region Functionality

    /// <summary>
    /// Starts a build, if this caller has any left today.
    /// </summary>
    /// <param name="editor">
    /// An editor link or key, to change something that already exists rather
    /// than make something new.
    /// </param>
    public async ValueTask<JsonObject> StartAsync(string? prompt, string? editor, IPAddress? caller)
    {
        var agent = Required();

        var wanted = (prompt ?? "").Trim();

        if (wanted.Length < 3)
        {
            throw new ProviderException(ResponseStatus.BadRequest, "Say what you would like built.");
        }

        if (wanted.Length > 2000)
        {
            wanted = wanted[..2000];
        }

        var key = KeyOf(editor);

        if (editor != null && editor.Trim().Length > 0 && key == null)
        {
            throw new ProviderException(ResponseStatus.BadRequest,
                "That does not look like an editor link. It is the address you were given to change "
              + "this with, ending in a long string of letters and numbers.");
        }

        if (caller != null && !Spend(caller))
        {
            throw new ProviderException(ResponseStatus.TooManyRequests,
                $"That is {_options.AgentBuildsPerDay} builds today, which is all this offers for now. "
              + "The editor is still there, and so is the MCP if you have an agent of your own.");
        }

        try
        {
            using var response = await agent.PostAsJsonAsync("build", new { prompt = wanted, key });

            var body = await ReadAsync(response);

            if (!response.IsSuccessStatusCode)
            {
                // a refusal from the agent is not this server's fault, but it
                // is this server's job to say it in the same shape as the rest
                throw new ProviderException((ResponseStatus)(int)response.StatusCode,
                                            body?["error"]?.GetValue<string>() ?? "The build agent would not take that.");
            }

            // the key is never logged: it is the only thing standing between
            // somebody and the ability to change what was built
            _logger.LogInformation("Build {Id} started, changing {Changing}", body?["id"], key != null);

            return body ?? [];
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning(e, "The build agent could not be reached");

            throw new ProviderException(ResponseStatus.ServiceUnavailable,
                                        "The build agent is not answering. Try again in a moment.");
        }
    }

    /// <summary>
    /// How a build is getting on.
    /// </summary>
    public async ValueTask<JsonObject> ProgressAsync(string id)
    {
        var agent = Required();

        if (!Guid.TryParse(id, out _))
        {
            throw new ProviderException(ResponseStatus.BadRequest, "That is not a build.");
        }

        try
        {
            using var response = await agent.GetAsync($"build/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ProviderException(ResponseStatus.NotFound, "There is no build by that name any more.");
            }

            return await ReadAsync(response) ?? [];
        }
        catch (HttpRequestException)
        {
            throw new ProviderException(ResponseStatus.ServiceUnavailable, "The build agent is not answering.");
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// The editor key inside whatever somebody pasted, or nothing.
    /// </summary>
    /// <remarks>
    /// People paste the whole address far more often than the key on its own,
    /// so both are accepted and anything that is neither is refused rather
    /// than sent on to be refused less clearly somewhere else.
    /// </remarks>
    private static string? KeyOf(string? editor)
    {
        var text = (editor ?? "").Trim();

        if (text.Length == 0)
        {
            return null;
        }

        // the last thing that looks like a key wins, so a full URL, a URL with
        // a trailing slash and a bare key all arrive at the same place
        var candidate = text.TrimEnd('/').Split('/', '?', '#').LastOrDefault() ?? "";

        return candidate.Length is >= 8 and <= 64 && candidate.All(c => c is >= 'a' and <= 'z' or >= '0' and <= '9')
             ? candidate
             : null;
    }

    private HttpClient Required()
        => _client ?? throw new ProviderException(ResponseStatus.NotFound,
                                                  "This installation does not have a build agent.");

    private static async ValueTask<JsonObject?> ReadAsync(HttpResponseMessage response)
    {
        try
        {
            return JsonNode.Parse(await response.Content.ReadAsStringAsync()) as JsonObject;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Counts one build against an address, and says whether it was allowed.
    /// </summary>
    /// <remarks>
    /// A day rather than an hour, because what is being defended is somebody's
    /// subscription rather than server load, and because a handful of builds
    /// is more than enough to decide whether you like this.
    /// </remarks>
    private bool Spend(IPAddress caller)
    {
        var today = DateTime.UtcNow.Date;

        if (_asked.Count > 20_000)
        {
            Forget(today);
        }

        var tally = _asked.GetOrAdd(caller, _ => new Tally(today));

        lock (tally)
        {
            if (tally.Day != today)
            {
                tally.Day = today;
                tally.Count = 0;
            }

            return ++tally.Count <= _options.AgentBuildsPerDay;
        }
    }

    private void Forget(DateTime today)
    {
        foreach (var (address, tally) in _asked)
        {
            if (tally.Day != today)
            {
                _asked.TryRemove(address, out _);
            }
        }
    }

    private sealed class Tally(DateTime day)
    {
        public DateTime Day = day;
        public int Count;
    }

    #endregion

    public void Dispose() => _client?.Dispose();

}
