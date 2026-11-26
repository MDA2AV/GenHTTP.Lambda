using GenHTTP.Lambda.Integrations.Agents;
using GenHTTP.Lambda.Integrations.ClaudeCode.Provider;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Integrations.ClaudeCode;

/// <summary>
/// Configures the runner of the platform's own agent.
/// </summary>
public sealed class ClaudeCodeRunnerBuilder(string url) : IIntegrationBuilder<IAgentProvider>
{
    private string? _token;

    private TimeSpan _poll = TimeSpan.FromSeconds(1.5);

    #region Functionality

    /// <summary>
    /// The secret the runner takes requests with (<c>LAMBDA_AGENT_TOKEN</c>),
    /// which also says which of the installations it builds for is asking.
    /// </summary>
    public ClaudeCodeRunnerBuilder Token(string? token)
    {
        _token = string.IsNullOrWhiteSpace(token) ? null : token;
        return this;
    }

    /// <summary>
    /// How often a runner from before it streamed what its runs do is asked
    /// how one is getting on.
    /// </summary>
    public ClaudeCodeRunnerBuilder Poll(TimeSpan interval)
    {
        _poll = interval;
        return this;
    }

    public IAgentProvider Build(IntegrationContext context)
        => new ClaudeCodeAgent(new Uri(url.TrimEnd('/') + "/"), _token, _poll, context.Loggers.CreateLogger<ClaudeCodeAgent>());

    #endregion

}
