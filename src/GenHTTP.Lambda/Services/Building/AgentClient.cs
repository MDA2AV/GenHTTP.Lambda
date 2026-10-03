using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Configuration;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// Speaks to the runner of the build agent (<c>docker/agent/builder.mjs</c>):
/// hands it builds and changes, asks how they are getting on, and stops them.
/// </summary>
/// <remarks>
/// The only thing here that holds the shared secret, knows the runner's
/// routes and reads its JSON. It decides nothing - whether a job may be asked
/// for, by whom and on which model is <see cref="BuildService"/>'s - and says
/// no more than the runner did: a refusal comes back with the runner's status
/// and in its words, a runner that cannot be reached as 503.
///
/// Without <see cref="LambdaOptions.AgentUrl"/> there is no runner to speak
/// to, and every call is answered with 404.
/// </remarks>
public sealed class AgentClient : IDisposable
{
    private static readonly JsonSerializerOptions Format = new(JsonSerializerDefaults.Web);

    private readonly HttpClient? _client;

    private readonly ILogger<AgentClient> _logger;

    public AgentClient(LambdaOptions options, ILogger<AgentClient> logger)
    {
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

    /// <summary>Whether this installation has an agent to speak to.</summary>
    public bool Available => _client != null;

    /// <summary>
    /// Refuses with 404 where this installation has no agent.
    /// </summary>
    public void Require() => Http();

    #region Builds

    /// <summary>
    /// Hands the runner a build.
    /// </summary>
    /// <returns>
    /// The build it took, or nothing where it took one but answered with
    /// nothing that could be read
    /// </returns>
    /// <exception cref="ProviderException">
    /// It did not take the build: it refused, or it could not be reached
    /// </exception>
    public ValueTask<BuildStarted?> StartBuildAsync(string prompt, string model)
        // no key travels with it: this endpoint only ever creates
        => StartAsync<BuildStarted>(new { prompt, model },
                                    "The build agent would not take that.",
                                    "The build agent is not answering. Try again in a moment.");

    /// <summary>
    /// How a build is getting on, or nothing where the runner has no build by that id.
    /// </summary>
    /// <remarks>
    /// The runner keeps changes by the same kind of id and answers for both
    /// at the same address. A change is not a build, so it is not answered here.
    /// </remarks>
    public async ValueTask<BuildProgress?> GetBuildAsync(string id)
    {
        var client = Http();

        try
        {
            using var response = await client.GetAsync($"build/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            var body = await ReadNodeAsync(response);

            if (body?["kind"]?.GetValue<string>() == "change")
            {
                return null;
            }

            return body?.Deserialize<BuildProgress>(Format)
                ?? throw new ProviderException(ResponseStatus.BadGateway, "The build agent answered with nothing.");
        }
        catch (HttpRequestException)
        {
            throw new ProviderException(ResponseStatus.ServiceUnavailable, "The build agent is not answering.");
        }
    }

    #endregion

    #region Changes

    /// <summary>
    /// Hands the runner a change of a lambda.
    /// </summary>
    /// <param name="lambda">The lambda, by the id the runner files the change under</param>
    /// <param name="privateKey">The editor key, which the agent needs to change anything</param>
    /// <param name="before">The version online now, to offer putting it back afterwards</param>
    /// <param name="deploy">Whether to merge the change and put it online once it works</param>
    /// <param name="language">The language of the control center, for the agent to fall back on</param>
    /// <param name="feature">The feature to go on with, by its key; a new one when left out</param>
    /// <returns>
    /// The change it took, or nothing where it took one but answered with
    /// nothing that could be read
    /// </returns>
    /// <exception cref="ProviderException">
    /// It did not take the change: it refused, or it could not be reached
    /// </exception>
    public ValueTask<ChangeProgress?> StartChangeAsync(long lambda, string privateKey, int? before, string prompt, bool deploy,
                                                       string model, string? language, string? feature)
        => StartAsync<ChangeProgress>(new
                                      {
                                          prompt,
                                          model,
                                          key = privateKey,
                                          lambda = lambda.ToString(CultureInfo.InvariantCulture),
                                          deploy,
                                          language,
                                          before,
                                          feature
                                      },
                                      "The agent would not take that.",
                                      "The agent is not answering. Try again in a moment.");

    /// <summary>
    /// The change under way for a lambda, or the last one while the runner
    /// still remembers it - an hour after it was asked for.
    /// </summary>
    public async ValueTask<ChangeProgress?> GetChangeAsync(long lambda)
    {
        var client = Http();

        try
        {
            using var response = await client.GetAsync($"lambda/{lambda}");

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

    /// <summary>
    /// Stops a change, and says how it was left - or nothing where the
    /// runner did not stop it.
    /// </summary>
    /// <remarks>
    /// The runner stops a change only for the lambda it is filed under, so a
    /// change of another lambda is not stopped by its id alone.
    /// </remarks>
    public async ValueTask<ChangeProgress?> StopChangeAsync(string id, long lambda)
    {
        var client = Http();

        try
        {
            using var response = await client.DeleteAsync($"build/{id}?lambda={lambda}");

            return response.IsSuccessStatusCode ? await ReadAsync<ChangeProgress>(response) : null;
        }
        catch (HttpRequestException)
        {
            throw new ProviderException(ResponseStatus.ServiceUnavailable, "The agent is not answering.");
        }
    }

    #endregion

    #region Protocol

    private HttpClient Http()
        => _client ?? throw new ProviderException(ResponseStatus.NotFound,
                                                  "This installation does not have a build agent.");

    /// <summary>
    /// Hands the runner a job, a build or a change.
    /// </summary>
    /// <remarks>
    /// Throws only where the runner did not take the job, so that whoever
    /// counts the jobs knows which ones to count: one the runner took is
    /// running, whatever it answered.
    /// </remarks>
    /// <param name="refused">What to say where the runner refused without saying why</param>
    /// <param name="unreachable">What to say where the runner could not be reached</param>
    private async ValueTask<T?> StartAsync<T>(object job, string refused, string unreachable) where T : class
    {
        var client = Http();

        try
        {
            using var response = await client.PostAsJsonAsync("build", job);

            if (!response.IsSuccessStatusCode)
            {
                // a refusal from the runner is not this server's fault, but it
                // is this server's job to say it in the same shape as the rest
                throw new ProviderException((ResponseStatus)(int)response.StatusCode,
                                            (await ReadAsync<Refusal>(response))?.Error ?? refused);
            }

            return await ReadAsync<T>(response);
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning(e, "Failed to reach build agent");

            throw new ProviderException(ResponseStatus.ServiceUnavailable, unreachable);
        }
    }

    private static async ValueTask<T?> ReadAsync<T>(HttpResponseMessage response) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), Format);
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
    /// What the runner says when it will not do something.
    /// </summary>
    private sealed record Refusal(string? Error);

    #endregion

    public void Dispose() => _client?.Dispose();

}
