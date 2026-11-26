using GenHTTP.Lambda.Integrations.Agents;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// One run of the fake agent, which a test drives.
/// </summary>
public sealed class FakeRun(AgentRun run)
{
    private readonly List<AgentEvent> _events = [];

    private TaskCompletionSource _next = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private long _sequence;

    /// <summary>What the platform handed it.</summary>
    public AgentRun Handed { get; } = run;

    /// <summary>The credential its calls carry.</summary>
    public string Credential => Handed.Access.Credential;

    public bool Cancelled { get; internal set; }

    /// <summary>Whether the agent system forgot it, as one restarted does.</summary>
    public bool Forgotten { get; private set; }

    public bool Working { get; private set; }

    public bool Waiting { get; private set; } = true;

    internal Task Next
    {
        get
        {
            lock (_events)
            {
                return _next.Task;
            }
        }
    }

    internal IReadOnlyList<AgentEvent> Since(long after)
    {
        lock (_events)
        {
            return [.. _events.Where(e => e.Sequence > after)];
        }
    }

    private FakeRun Add(Func<long, AgentEvent> make)
    {
        TaskCompletionSource woken;

        lock (_events)
        {
            _events.Add(make(++_sequence));

            woken = _next;
            _next = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        woken.TrySetResult();

        return this;
    }

    public FakeRun Queue(int position) => Add(s => new AgentQueued(s, DateTimeOffset.UtcNow, position));

    public FakeRun Start()
    {
        Working = true;
        Waiting = false;

        return Add(s => new AgentStarted(s, DateTimeOffset.UtcNow));
    }

    public FakeRun Wait(AgentWaitReason reason, DateTimeOffset? until = null) => Add(s => new AgentWaiting(s, DateTimeOffset.UtcNow, reason, until, "busy"));

    public FakeRun Say(string text) => Add(s => new AgentSaid(s, DateTimeOffset.UtcNow, text));

    public FakeRun Activity(string id, AgentActivityKind kind, string? subject, bool done = false, bool failed = false)
        => Add(s => new AgentActivity(s, DateTimeOffset.UtcNow, id, kind, subject, done, failed));

    public FakeRun Return(string name, string type, byte[] content, string? description = null)
        => Add(s => new AgentReturned(s, DateTimeOffset.UtcNow, new AgentFile(name, type, content, description)));

    public FakeRun Spend(long input, long output, decimal cost) => Add(s => new AgentSpent(s, DateTimeOffset.UtcNow, new AgentUsage(input, output, cost, "USD")));

    public FakeRun End(AgentEnding ending, string? message = null, string? detail = null)
    {
        Working = false;
        Waiting = false;

        return Add(s => new AgentEnded(s, DateTimeOffset.UtcNow, ending, message, detail));
    }

    /// <summary>Forgets the run, as an agent system that was restarted has.</summary>
    public void Forget()
    {
        Forgotten = true;

        TaskCompletionSource woken;

        lock (_events)
        {
            woken = _next;
            _next = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        woken.TrySetResult();
    }

}
