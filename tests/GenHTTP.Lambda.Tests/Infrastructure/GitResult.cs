namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// What running git came to.
/// </summary>
internal sealed record GitResult(int ExitCode, string Output, string Error)
{

    public bool Success => ExitCode == 0;

    /// <summary>
    /// What git printed, both streams - a push says what the server said on the second.
    /// </summary>
    public string Said => Output + Error;

    public override string ToString() => $"exit code {ExitCode}\n--- stdout ---\n{Output}\n--- stderr ---\n{Error}";

}
