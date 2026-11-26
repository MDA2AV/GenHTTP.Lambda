using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Integrations.Agents;

namespace GenHTTP.Lambda.Tests.Infrastructure;

/// <summary>
/// An agent integration played in the test's own process: it takes the runs
/// the platform hands it, and a test says what happens in each - what the
/// agent says, does beyond the platform, hands back and how it ends - while
/// what it does on the platform the test does through the platform's doors,
/// with the run's credential, as any agent would.
/// </summary>
/// <remarks>
/// Its options are named as no real integration's are, so a test proves the
/// platform offers, records and counts whatever an integration brings.
/// </remarks>
public sealed class FakeAgentProvider : IAgentProvider, IIntegrationBuilder<IAgentProvider>
{
    private readonly ConcurrentDictionary<string, FakeRun> _runs = new(StringComparer.Ordinal);

    /// <summary>What it says it can do; a test changes it to see the pages follow.</summary>
    public AgentCapabilities Capabilities { get; set; } = Everything;

    /// <summary>Every run it was handed, as it was handed.</summary>
    public ConcurrentQueue<AgentRun> Received { get; } = [];

    /// <summary>When set, a run is refused for this reason.</summary>
    public AgentRefusal? Refuse { get; set; }

    /// <summary>Whether it answers for the panel.</summary>
    public bool Reachable { get; set; } = true;

    /// <summary>The newest run it was handed.</summary>
    public FakeRun Last => _runs[Received.Last().Id];

    /// <summary>A run by the platform's id.</summary>
    public FakeRun Run(string id) => _runs[id];

    /// <summary>
    /// What it offers where a test does not say otherwise: an option that is
    /// no model, a value behind the password, files, the web, questions back.
    /// </summary>
    public static readonly AgentCapabilities Everything = new(
        new AgentDisclosure("Test agent", "Nobody in particular", [new AgentDestination("A test", "localhost", "Nothing leaves the test.")]),
        [
            new AgentOption("engine", "Engine", [new AgentOptionValue("swift", "Swift"), new AgentOptionValue("thorough", "Thorough", "Takes its time", Guarded: true)], "swift"),
            new AgentOption("tone", "Tone", [new AgentOptionValue("plain", "Plain"), new AgentOptionValue("warm", "Warm")], "plain")
        ],
        new AgentAttachmentPolicy(["image/*", "text/*", "application/pdf", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                                   "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"], 5 * 1024 * 1024, 3),
        ReturnsFiles: true,
        new AgentWebAccess(AgentWebScope.Any, true, "Anything"),
        Cancels: true,
        Asks: true,
        new AgentLimitSupport(true, true, new AgentLimits(TimeSpan.FromMinutes(5), 40)));

    #region Integration

    public IAgentProvider Build(IntegrationContext context) => this;

    public ValueTask PrepareAsync(CancellationToken cancellation = default) => ValueTask.CompletedTask;

    public IntegrationInfo Describe() => new(IntegrationKind.Agent, "Fake", [new IntegrationDetail("Runs", _runs.Count.ToString())]);

    public string? Guide(GuideContext context) => null;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    #endregion

    #region Runs

    public ValueTask<AgentAcceptance> StartAsync(AgentRun run, CancellationToken cancellation = default)
    {
        if (Refuse is { } reason)
        {
            throw new AgentRefusedException(reason, "Not now.");
        }

        Received.Enqueue(run);

        var fake = _runs.GetOrAdd(run.Id, _ => new FakeRun(run));

        fake.Queue(1);

        return ValueTask.FromResult(new AgentAcceptance(1));
    }

    public async IAsyncEnumerable<AgentEvent> FollowAsync(AgentRunHandle run, long after, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        if (!_runs.TryGetValue(run.Id, out var fake) || fake.Forgotten)
        {
            yield return new AgentEnded(long.MaxValue, DateTimeOffset.UtcNow, AgentEnding.Lost);
            yield break;
        }

        while (true)
        {
            var next = fake.Next;

            foreach (var happened in fake.Since(after))
            {
                yield return happened;

                after = happened.Sequence;

                if (happened is AgentEnded)
                {
                    yield break;
                }
            }

            if (fake.Forgotten)
            {
                yield return new AgentEnded(long.MaxValue, DateTimeOffset.UtcNow, AgentEnding.Lost);
                yield break;
            }

            await next.WaitAsync(cancellation);
        }
    }

    public ValueTask<bool> CancelAsync(AgentRunHandle run, CancellationToken cancellation = default)
    {
        if (!_runs.TryGetValue(run.Id, out var fake))
        {
            return ValueTask.FromResult(false);
        }

        fake.Cancelled = true;
        fake.End(AgentEnding.Cancelled);

        return ValueTask.FromResult(true);
    }

    public ValueTask<AgentLoad> LoadAsync(CancellationToken cancellation = default)
        => ValueTask.FromResult(Reachable
            ? new AgentLoad(true, 2, _runs.Values.Count(r => r.Working), _runs.Values.Count(r => r.Waiting), [new IntegrationDetail("Played by", "a test")])
            : AgentLoad.Unreachable);

    #endregion

}
