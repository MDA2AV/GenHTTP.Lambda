using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Text.Json;

using GenHTTP.Api.Content;
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

    /// <summary>Whether the second model is on offer at all.</summary>
    public bool HasSecondModel => !string.IsNullOrWhiteSpace(_options.AgentFablePassword);

    #region Functionality

    /// <summary>
    /// Starts a build, if this caller has any left today.
    /// </summary>
    /// <param name="model">
    /// Which of the offered models to use. The second one needs the password.
    /// </param>
    /// <param name="password">The password for the second model, where one was asked for.</param>
    public async ValueTask<BuildStarted> StartAsync(string? prompt, string? model, string? password, IPAddress? caller)
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

        var wantedModel = (model ?? "").Trim().ToLowerInvariant();

        if (wantedModel is not ("" or "opus" or "fable"))
        {
            throw new ProviderException(ResponseStatus.BadRequest, "There is no such model here.");
        }

        if (wantedModel == "fable")
        {
            /*
             * Compared in full rather than short-circuiting on the first wrong
             * character. It is a soft gate rather than a secret, but a
             * comparison that returns faster for a closer guess is one anybody
             * can walk a character at a time, and constant time costs nothing
             * here.
             */
            var expected = _options.AgentFablePassword;

            if (string.IsNullOrWhiteSpace(expected) ||
                !CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(expected),
                    Encoding.UTF8.GetBytes(password ?? "")))
            {
                throw new ProviderException(ResponseStatus.Forbidden,
                                            "That password is not right.");
            }
        }

        if (caller != null && !Spend(caller))
        {
            throw new ProviderException(ResponseStatus.TooManyRequests,
                $"That is {_options.AgentBuildsPerDay} builds today, which is all this offers for now. "
              + "The editor is still there, and so is the MCP if you have an agent of your own.");
        }

        try
        {
            // no key travels with it: this endpoint only ever creates
            using var response = await agent.PostAsJsonAsync("build", new { prompt = wanted, model = wantedModel });

            if (!response.IsSuccessStatusCode)
            {
                // a refusal from the agent is not this server's fault, but it
                // is this server's job to say it in the same shape as the rest
                throw new ProviderException((ResponseStatus)(int)response.StatusCode,
                                            (await ReadAsync<AgentRefusal>(response))?.Error ?? "The build agent would not take that.");
            }

            var started = await ReadAsync<BuildStarted>(response)
                       ?? throw new ProviderException(ResponseStatus.BadGateway, "The build agent answered with nothing.");

            _logger.LogInformation("Build {Id} started", started.Id);

            return started;
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
    public async ValueTask<BuildProgress> ProgressAsync(string id)
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

            return await ReadAsync<BuildProgress>(response)
                ?? throw new ProviderException(ResponseStatus.BadGateway, "The build agent answered with nothing.");
        }
        catch (HttpRequestException)
        {
            throw new ProviderException(ResponseStatus.ServiceUnavailable, "The build agent is not answering.");
        }
    }

    #endregion

    #region Helpers

    private HttpClient Required()
        => _client ?? throw new ProviderException(ResponseStatus.NotFound,
                                                  "This installation does not have a build agent.");

    private static readonly JsonSerializerOptions AgentFormat = new(JsonSerializerDefaults.Web);

    private static async ValueTask<T?> ReadAsync<T>(HttpResponseMessage response) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), AgentFormat);
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

    /// <summary>
    /// What the agent says when it will not do something.
    /// </summary>
    private sealed record AgentRefusal(string? Error);

}
