using System.Collections.Concurrent;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// Wakes whatever waits for a run to change - a page listening for its
/// events - as soon as something of it is recorded.
/// </summary>
/// <remarks>
/// <para>
/// What changed is read from the records; this only says that something did,
/// so a listener reads again rather than on a timer. One signal per run, and
/// one per lambda for the page that follows whatever run its app has.
/// </para>
/// <para>
/// In the memory of this server: a listener on another server of the same
/// installation is woken by its own server's follower only, which is known
/// state of the kind CLAUDE.md lists - the records hold everything, and a
/// listener reads them again at least every <see cref="Patience"/>.
/// </para>
/// </remarks>
public sealed class AgentSignals
{

    /// <summary>
    /// How long a listener waits for a signal before it reads again anyway.
    /// </summary>
    public static readonly TimeSpan Patience = TimeSpan.FromSeconds(20);

    private readonly ConcurrentDictionary<string, TaskCompletionSource> _waiting = new(StringComparer.Ordinal);

    /// <summary>
    /// What completes once the run changes next - taken before its records
    /// are read, so a change between reading and waiting is not missed.
    /// </summary>
    public Task NextOfRun(string run) => Next(RunKey(run));

    /// <summary>
    /// What completes once a run of the lambda changes next, or a new one starts.
    /// </summary>
    public Task NextOfLambda(long lambda) => Next(LambdaKey(lambda));

    /// <summary>
    /// Says that a run, and the lambda it works on, changed.
    /// </summary>
    public void Touch(string run, long? lambda)
    {
        Wake(RunKey(run));

        if (lambda is { } id)
        {
            Wake(LambdaKey(id));
        }
    }

    private Task Next(string key) => _waiting.GetOrAdd(key, _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)).Task;

    private void Wake(string key)
    {
        if (_waiting.TryRemove(key, out var waiting))
        {
            waiting.TrySetResult();
        }
    }

    private static string RunKey(string run) => "run:" + run;

    private static string LambdaKey(long lambda) => "lambda:" + lambda;

}
