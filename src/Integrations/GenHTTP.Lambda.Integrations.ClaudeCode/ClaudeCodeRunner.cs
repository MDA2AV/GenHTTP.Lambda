namespace GenHTTP.Lambda.Integrations.ClaudeCode;

/// <summary>
/// The agent of the platform's own: Claude Code in a throwaway container per
/// run, driven by the runner in <c>docker/agent</c> - what an installation
/// builds with where its host names a runner and chooses no other agent.
/// </summary>
/// <example>
/// <code>
/// LambdaPlatform.Create().Agent(ClaudeCodeRunner.Create("http://agent:8401").Token(token))
/// </code>
/// </example>
public static class ClaudeCodeRunner
{

    /// <param name="url">Where the runner listens, as <c>LAMBDA_AGENT_URL</c> names it</param>
    public static ClaudeCodeRunnerBuilder Create(string url) => new(url);

}
