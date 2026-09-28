using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

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
/// talks to it: it holds the shared secret, counts how many jobs an address
/// has asked for today, and does nothing else.
///
/// Two kinds of job go through it. A build makes something new for whoever
/// typed into the box on /build. A change works on a lambda that exists, for
/// whoever holds its editor key, and is asked for from the control center.
/// They share the queue and the daily allowance, since both spend the same
/// subscription.
///
/// Nothing about a change is kept here. The agent files it under the lambda
/// and is asked for it by lambda, so a change goes on being reported when
/// this server is redeployed halfway through it.
///
/// Off unless LambdaOptions.AgentUrl is set, so an installation without an
/// agent simply does not have the feature rather than having a broken one.
/// </remarks>
public sealed partial class BuildService : IDisposable
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

    #region Building

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

        var wanted = Prompt(prompt, "Say what you would like built.");

        var wantedModel = Model(model, password);

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
                Refund(caller);

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
            Refund(caller);

            _logger.LogWarning(e, "The build agent could not be reached");

            throw new ProviderException(ResponseStatus.ServiceUnavailable,
                                        "The build agent is not answering. Try again in a moment.");
        }
    }

    /// <summary>
    /// How a build is getting on.
    /// </summary>
    /// <remarks>
    /// Anybody holding the id of a build may ask, which is what the build page
    /// does. A change is not answered here even though the agent keeps it by
    /// the same kind of id: it belongs to whoever holds the editor key, and is
    /// asked for with that key, under the lambda.
    /// </remarks>
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

            var body = await ReadNodeAsync(response);

            if (body?["kind"]?.GetValue<string>() == "change")
            {
                throw new ProviderException(ResponseStatus.NotFound, "There is no build by that name any more.");
            }

            return body?.Deserialize<BuildProgress>(AgentFormat)
                ?? throw new ProviderException(ResponseStatus.BadGateway, "The build agent answered with nothing.");
        }
        catch (HttpRequestException)
        {
            throw new ProviderException(ResponseStatus.ServiceUnavailable, "The build agent is not answering.");
        }
    }

    #endregion

    #region Changing

    /// <summary>
    /// What the Change section of a lambda shows: whether it can ask for a
    /// change, how many it has left, and the change under way or the last one.
    /// </summary>
    /// <param name="lambda">The lambda, by the id it is filed under</param>
    public async ValueTask<AgentState> StateAsync(long lambda, IPAddress? caller)
    {
        if (_client == null)
        {
            return new AgentState(false, PerDay, 0, HasSecondModel, null);
        }

        return new AgentState(true, PerDay, Left(caller), HasSecondModel, await CurrentAsync(lambda));
    }

    /// <summary>
    /// Asks the agent to change a lambda, if this caller has any left today
    /// and no other change of it is under way.
    /// </summary>
    /// <param name="lambda">The lambda, by the id the agent files the change under</param>
    /// <param name="privateKey">The editor key, which the agent needs to change anything</param>
    /// <param name="before">The version online now, to offer putting it back afterwards</param>
    /// <param name="deploy">Whether to put the change online once it compiles</param>
    /// <param name="language">The language of the control center, for the agent to fall back on</param>
    public async ValueTask<AgentState> ChangeAsync(long lambda, string privateKey, int? before, string? prompt, bool deploy,
                                                   string? model, string? password, string? language, IPAddress? caller)
    {
        var agent = Required();

        var wanted = Prompt(prompt, "Say what should be different.");

        var wantedModel = Model(model, password);

        // asked before anything is spent: a second change of the same lambda
        // is refused, and refusing it should not cost the owner one
        if (await CurrentAsync(lambda) is { State: "queued" or "running" })
        {
            throw new ProviderException(ResponseStatus.Conflict,
                "A change of this lambda is already under way. Wait for it, or stop it first.");
        }

        if (caller != null && !Spend(caller))
        {
            throw new ProviderException(ResponseStatus.TooManyRequests,
                $"That is all {_options.AgentBuildsPerDay} builds and changes for today. It starts again at midnight UTC.");
        }

        try
        {
            using var response = await agent.PostAsJsonAsync("build", new
            {
                prompt = wanted,
                model = wantedModel,
                key = privateKey,
                lambda = lambda.ToString(System.Globalization.CultureInfo.InvariantCulture),
                deploy,
                language = Language(language),
                before
            });

            if (!response.IsSuccessStatusCode)
            {
                Refund(caller);

                throw new ProviderException((ResponseStatus)(int)response.StatusCode,
                                            (await ReadAsync<AgentRefusal>(response))?.Error ?? "The agent would not take that.");
            }

            var job = await ReadAsync<ChangeProgress>(response)
                   ?? throw new ProviderException(ResponseStatus.BadGateway, "The agent answered with nothing.");

            // the key is never logged: it is the only thing standing between
            // somebody reading this log and the ability to change the lambda
            _logger.LogInformation("Change {Id} of lambda {Lambda} started", job.Id, lambda);

            return new AgentState(true, PerDay, Left(caller), HasSecondModel, job);
        }
        catch (HttpRequestException e)
        {
            Refund(caller);

            _logger.LogWarning(e, "The build agent could not be reached");

            throw new ProviderException(ResponseStatus.ServiceUnavailable,
                                        "The agent is not answering. Try again in a moment.");
        }
    }

    /// <summary>
    /// Stops the change of a lambda that is under way. Whatever it saved so
    /// far stays saved: versions are only ever added.
    /// </summary>
    public async ValueTask<AgentState> StopAsync(long lambda, IPAddress? caller)
    {
        var agent = Required();

        var current = await CurrentAsync(lambda);

        if (current is { State: "queued" or "running" })
        {
            try
            {
                using var response = await agent.DeleteAsync($"build/{current.Id}?lambda={lambda}");

                if (response.IsSuccessStatusCode)
                {
                    current = await ReadAsync<ChangeProgress>(response) ?? current;
                }
            }
            catch (HttpRequestException)
            {
                throw new ProviderException(ResponseStatus.ServiceUnavailable, "The agent is not answering.");
            }
        }

        return new AgentState(true, PerDay, Left(caller), HasSecondModel, current);
    }

    /// <summary>
    /// The change under way for a lambda, or the last one while the agent
    /// still remembers it - an hour after it was asked for.
    /// </summary>
    private async ValueTask<ChangeProgress?> CurrentAsync(long lambda)
    {
        var agent = Required();

        try
        {
            using var response = await agent.GetAsync($"lambda/{lambda}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return await ReadAsync<ChangeProgress>(response);
        }
        catch (HttpRequestException)
        {
            throw new ProviderException(ResponseStatus.ServiceUnavailable, "The agent is not answering.");
        }
    }

    #endregion

    #region Helpers

    private HttpClient Required()
        => _client ?? throw new ProviderException(ResponseStatus.NotFound,
                                                  "This installation does not have a build agent.");

    /// <summary>What was asked for, cut to what the agent takes.</summary>
    private static string Prompt(string? prompt, string missing)
    {
        var wanted = (prompt ?? "").Trim();

        if (wanted.Length < 3)
        {
            throw new ProviderException(ResponseStatus.BadRequest, missing);
        }

        return wanted.Length > 2000 ? wanted[..2000] : wanted;
    }

    /// <summary>The model asked for, where the caller may have it.</summary>
    private string Model(string? model, string? password)
    {
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

        return wantedModel;
    }

    /// <summary>A language code as the pages send one, or nothing where it is not.</summary>
    private static string? Language(string? language)
    {
        var code = (language ?? "").Trim().ToLowerInvariant();

        return LanguageCode().IsMatch(code) ? code : null;
    }

    [GeneratedRegex("^[a-z]{2}(-[a-z]{2})?$")]
    private static partial Regex LanguageCode();

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

    private static async ValueTask<JsonNode?> ReadNodeAsync(HttpResponseMessage response)
    {
        try
        {
            return JsonNode.Parse(await response.Content.ReadAsStringAsync());
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Counts one job against an address, and says whether it was allowed.
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

    /// <summary>
    /// Gives back what <see cref="Spend"/> took, for a job the agent never
    /// took on: a full queue or an agent that is down is not something the
    /// caller should pay for.
    /// </summary>
    private void Refund(IPAddress? caller)
    {
        if (caller == null || !_asked.TryGetValue(caller, out var tally))
        {
            return;
        }

        lock (tally)
        {
            if (tally.Day == DateTime.UtcNow.Date && tally.Count > 0)
            {
                tally.Count--;
            }
        }
    }

    /// <summary>How many jobs an address has left today.</summary>
    private int Left(IPAddress? caller)
    {
        if (caller == null || !_asked.TryGetValue(caller, out var tally))
        {
            return PerDay;
        }

        lock (tally)
        {
            return tally.Day == DateTime.UtcNow.Date ? Math.Max(0, PerDay - tally.Count) : PerDay;
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
