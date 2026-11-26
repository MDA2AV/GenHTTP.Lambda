using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;

using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Integrations.Agents;
using GenHTTP.Lambda.Integrations.Files;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// Follows the runs that wait or work: takes what the agent system tells of
/// each, as it tells it, into the platform's records - which is where every
/// page reads a run from.
/// </summary>
/// <remarks>
/// <para>
/// One loop per run, for as long as it has not ended. A loop that loses the
/// agent system asks again from the last event it recorded, a while later
/// and then less often; the run says meanwhile that the agent does not
/// answer. As the platform starts, every run its records say has not ended
/// is followed again (<see cref="ResumeAsync"/>), so a restart loses nothing
/// the agent system still has. A run the agent system knows nothing of ends
/// as lost, and so does one that is long overdue - what it did is in the
/// records either way.
/// </para>
/// <para>
/// Exactly one server of an installation follows a run: it claims the run in
/// the records before it follows and renews the claim while it does - by
/// asking again from where it got to every <see cref="Renewal"/> - and lets
/// go as it goes away. A server that went away without is taken over once its
/// claim lapsed (<see cref="Lease"/>), as every server looks for runs nobody
/// follows (<see cref="AgentFollowJob"/>).
/// </para>
/// </remarks>
public sealed class AgentFollower(AgentRunRecords records, IFileProvider files, ILogger<AgentFollower> logger, IAgentProvider? provider = null)
    : IAsyncDisposable
{

    /// <summary>The most a file the agent hands back may take.</summary>
    public const int LongestFile = 10 * 1024 * 1024;

    /// <summary>How long past its limit - or a day, without one - a run nothing is heard of is given up as lost.</summary>
    private static readonly TimeSpan Overdue = TimeSpan.FromMinutes(15);

    private static readonly TimeSpan Longest = TimeSpan.FromDays(1);

    /// <summary>How long handing a run to the agent system may take before it is given up.</summary>
    private static readonly TimeSpan HandOver = TimeSpan.FromMinutes(5);

    /// <summary>How long a server's claim to follow a run holds without being renewed.</summary>
    public static readonly TimeSpan Lease = TimeSpan.FromMinutes(2);

    /// <summary>How often a claim is renewed while the run is followed.</summary>
    private static readonly TimeSpan Renewal = TimeSpan.FromSeconds(40);

    /// <summary>What this platform is known by in the claims: random, as nothing else tells servers apart.</summary>
    private readonly string _me = Convert.ToHexStringLower(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8));

    private readonly ConcurrentDictionary<string, Task> _following = new(StringComparer.Ordinal);

    private readonly ConcurrentDictionary<string, DateTime> _unreachable = new(StringComparer.Ordinal);

    private readonly CancellationTokenSource _stopping = new();

    private int _disposed;

    #region Functionality

    /// <summary>
    /// Since when the agent system has not answered for a run, while it does not.
    /// </summary>
    public DateTime? UnreachableSince(string run) => _unreachable.TryGetValue(run, out var since) ? since : null;

    /// <summary>
    /// Begins following a run the agent system took, unless it is followed already.
    /// </summary>
    public void Follow(AgentRunEntity run)
    {
        if (provider == null || _stopping.IsCancellationRequested)
        {
            return;
        }

        // a loop for the life of the run rather than a hop a request waits
        // for, so not through Offload: whatever bounds the hops one day must
        // not have these in its slots
#pragma warning disable RS0030
        _following.GetOrAdd(run.Id, _ => Task.Run(() => LoopAsync(run)));
#pragma warning restore RS0030
    }

    /// <summary>
    /// Follows every run the records say has not ended - or, on an
    /// installation that has no agent any more, ends them as lost.
    /// </summary>
    public async ValueTask ResumeAsync(CancellationToken cancellation = default)
    {
        foreach (var run in await records.ActiveAsync(cancellation))
        {
            if (provider == null)
            {
                await EndAsync(run.Id, AgentEnding.Lost, "This installation has no agent any more.", cancellation);
                continue;
            }

            // one being handed over is the server's that hands it over, which
            // follows it once the agent system took it; one that stayed there
            // was never taken - its server went away in between
            if (run.State == AgentRunState.Starting)
            {
                if (DateTime.UtcNow - run.Created > HandOver)
                {
                    await EndAsync(run.Id, AgentEnding.Lost, "The run was never handed to the agent.", cancellation);
                }

                continue;
            }

            Follow(run);
        }
    }

    #endregion

    #region Following

    private async Task LoopAsync(AgentRunEntity run)
    {
        var stopping = _stopping.Token;

        var after = run.Followed;
        var pause = TimeSpan.FromSeconds(1);

        try
        {
            while (!stopping.IsCancellationRequested)
            {
                // another server follows it: it is that one's to record
                if (!await records.ClaimAsync(run.Id, _me, DateTime.UtcNow + Lease, stopping))
                {
                    return;
                }

                using var turn = CancellationTokenSource.CreateLinkedTokenSource(stopping);

                turn.CancelAfter(Renewal);

                try
                {
                    await foreach (var happened in provider!.FollowAsync(new AgentRunHandle(run.Id, run.Reference), after, turn.Token))
                    {
                        _unreachable.TryRemove(run.Id, out _);

                        if (happened.Sequence <= after)
                        {
                            continue;
                        }

                        await ApplyAsync(run.Id, happened, stopping);

                        after = happened.Sequence;
                        pause = TimeSpan.FromSeconds(1);

                        if (happened is AgentEnded)
                        {
                            return;
                        }
                    }
                }
                catch (OperationCanceledException) when (stopping.IsCancellationRequested)
                {
                    return;
                }
                catch (OperationCanceledException) when (turn.IsCancellationRequested)
                {
                    // time to renew the claim, and asked again from where it got
                    // to - unless the run went silent for longer than it may take,
                    // which the agent system telling nothing must not hide
                    var silent = await records.GetAsync(run.Id, stopping);

                    if (silent == null || silent.State == AgentRunState.Ended)
                    {
                        return;
                    }

                    if (IsOverdue(silent))
                    {
                        await provider!.CancelAsync(new AgentRunHandle(run.Id, run.Reference), CancellationToken.None);
                        await EndAsync(run.Id, AgentEnding.Lost, "Nothing came of the run for longer than it may take.", stopping);
                        return;
                    }

                    continue;
                }
                catch (Exception e)
                {
                    if (_unreachable.TryAdd(run.Id, DateTime.UtcNow))
                    {
                        logger.LogWarning(e, "Lost the agent while following run {Run}", run.Id);

                        // the page says the agent does not answer
                        await records.UpdateAsync(run.Id, _ => { }, CancellationToken.None);
                    }
                }

                // the stream ended without the run ending, or broke off: ask
                // again from where it got to, unless it is long overdue
                var current = await records.GetAsync(run.Id, stopping);

                if (current == null || current.State == AgentRunState.Ended)
                {
                    return;
                }

                if (IsOverdue(current))
                {
                    await EndAsync(run.Id, AgentEnding.Lost, "Nothing was heard of the run for longer than it may take.", stopping);
                    return;
                }

                await Task.Delay(pause, stopping);

                pause = pause * 2 > TimeSpan.FromSeconds(30) ? TimeSpan.FromSeconds(30) : pause * 2;
            }
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested)
        {
            // the platform is going away; the run is followed again as it starts
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to follow run {Run}", run.Id);
        }
        finally
        {
            _following.TryRemove(run.Id, out _);
            _unreachable.TryRemove(run.Id, out _);
        }
    }

    private static bool IsOverdue(AgentRunEntity run)
    {
        var since = run.Started ?? run.Created;

        var may = run.TimeLimit is { } seconds ? TimeSpan.FromSeconds(seconds) : Longest;

        return DateTime.UtcNow - since > may + Overdue;
    }

    #endregion

    #region Recording

    /// <summary>
    /// Takes one event of the agent system's into the records.
    /// </summary>
    private async ValueTask ApplyAsync(string id, AgentEvent happened, CancellationToken cancellation)
    {
        var at = happened.At.UtcDateTime;

        switch (happened)
        {
            case AgentQueued queued:
                await records.UpdateAsync(id, run =>
                {
                    if (run.State is AgentRunState.Starting or AgentRunState.Queued)
                    {
                        run.State = AgentRunState.Queued;
                        run.Position = Math.Max(0, queued.Position);
                    }

                    run.Followed = happened.Sequence;
                }, cancellation);
                break;

            case AgentStarted started:
                await records.UpdateAsync(id, run =>
                {
                    run.Followed = happened.Sequence;

                    // what comes late of a run that is over changes nothing of it
                    if (run.State == AgentRunState.Ended)
                    {
                        return;
                    }

                    run.State = AgentRunState.Working;
                    run.Started ??= at;
                    run.Position = 0;
                    run.WaitReason = null;
                    run.WaitUntil = null;

                    // the agent may hold a run to less than the platform set,
                    // never to more: the platform's limit is what it watches
                    if (started.Limits is { } limits)
                    {
                        if (limits.Time is { } time && (run.TimeLimit == null || time.TotalSeconds < run.TimeLimit))
                        {
                            run.TimeLimit = (int)time.TotalSeconds;
                        }

                        if (limits.Steps is { } steps && (run.StepLimit == null || steps < run.StepLimit))
                        {
                            run.StepLimit = steps;
                        }
                    }

                    run.Followed = happened.Sequence;
                }, cancellation);
                break;

            case AgentWaiting waiting:
                await records.UpdateAsync(id, run =>
                {
                    run.Followed = happened.Sequence;

                    if (run.State == AgentRunState.Ended)
                    {
                        return;
                    }

                    run.State = AgentRunState.Waiting;
                    run.Started ??= at;
                    run.WaitReason = waiting.Reason;
                    run.WaitUntil = waiting.Until?.UtcDateTime;
                    run.Detail = Clip(waiting.Detail, 400) ?? run.Detail;
                    run.Followed = happened.Sequence;
                }, cancellation);
                break;

            case AgentSaid said when !string.IsNullOrWhiteSpace(said.Text):
            {
                // recorded before the position moves on, and once by the
                // agent's number - so a crash in between neither loses it nor,
                // asked again from the position before, records it twice
                var run = await records.GetAsync(id, cancellation);

                if (!await HasAsync(id, Told(happened), cancellation))
                {
                    await records.AppendAsync(id, run?.LambdaId, AgentRunEventKind.Said, new AgentStep(0, "say", Text: Clip(said.Text.Trim(), 400)),
                                              Told(happened), at, cancellation);
                }

                await MarkAsync(id, happened.Sequence, cancellation);
                break;
            }

            case AgentActivity activity:
            {
                var run = await MarkAsync(id, happened.Sequence, cancellation);

                var step = StepOf(activity);

                if (activity.Done && await HasAsync(id, activity.Id, cancellation))
                {
                    await records.CompleteAsync(id, run?.LambdaId, null, activity.Id, step, cancellation);
                }
                else
                {
                    await records.AppendAsync(id, run?.LambdaId, AgentRunEventKind.Activity, step, activity.Id, at, cancellation);
                }

                break;
            }

            case AgentReturned returned:
                await KeepAsync(id, returned, at, cancellation);
                break;

            case AgentSpent spent:
                await records.UpdateAsync(id, run =>
                {
                    run.InputTokens = spent.Usage.InputTokens ?? run.InputTokens;
                    run.OutputTokens = spent.Usage.OutputTokens ?? run.OutputTokens;
                    run.Cost = spent.Usage.Cost is { } cost ? (double)cost : run.Cost;
                    run.Currency = spent.Usage.Currency ?? run.Currency;
                    run.Followed = happened.Sequence;
                }, cancellation);
                break;

            case AgentEnded ended:
                await records.UpdateAsync(id, run =>
                {
                    // a run the platform ended itself - stopped while it waited -
                    // keeps the ending it was given
                    run.Ending = run.State == AgentRunState.Ended ? run.Ending ?? ended.Ending : ended.Ending;
                    run.State = AgentRunState.Ended;
                    run.Message = Clip(ended.Message?.Trim(), 4000);
                    run.Detail = Clip(ended.Detail?.Trim(), 1000);
                    run.Ended ??= at;
                    run.Position = 0;
                    run.WaitReason = null;
                    run.WaitUntil = null;
                    run.Followed = happened.Sequence;
                }, cancellation);
                break;

            default:
                await MarkAsync(id, happened.Sequence, cancellation);
                break;
        }
    }

    /// <summary>What an event of the agent system's is recorded under, so it is recorded once.</summary>
    private static string Told(AgentEvent happened) => $"#{happened.Sequence.ToString(CultureInfo.InvariantCulture)}";

    private ValueTask<AgentRunEntity?> MarkAsync(string id, long sequence, CancellationToken cancellation)
        => records.UpdateAsync(id, run =>
        {
            run.Followed = sequence;

            // a run that says something works, whatever it said of its turn before
            if (run.State is AgentRunState.Starting or AgentRunState.Queued)
            {
                run.State = AgentRunState.Working;
                run.Started ??= DateTime.UtcNow;
                run.Position = 0;
            }
        }, cancellation);

    private async ValueTask<bool> HasAsync(string id, string key, CancellationToken cancellation)
        => (await records.EventsAsync(id, cancellation)).Any(e => e.Key == key);

    /// <summary>
    /// What the agent did beyond the platform, as a step the pages have words for.
    /// </summary>
    private static AgentStep StepOf(AgentActivity activity) => activity.Kind switch
    {
        AgentActivityKind.ReadsPage => new AgentStep(0, "web", Site: Clip(activity.Subject, 253), Done: activity.Done ? true : null,
                                                     Problem: activity.Failed ? "failed" : null),
        AgentActivityKind.Searches => new AgentStep(0, "search", Done: activity.Done ? true : null, Problem: activity.Failed ? "failed" : null),
        _ => new AgentStep(0, "other", Tool: Clip(activity.Subject, 80), Done: activity.Done ? true : null, Problem: activity.Failed ? "failed" : null)
    };

    /// <summary>
    /// Keeps a file the agent handed back, with the run, and says so as a step.
    /// </summary>
    private async ValueTask KeepAsync(string id, AgentReturned returned, DateTime at, CancellationToken cancellation)
    {
        var run = await records.GetAsync(id, cancellation);

        var file = returned.File;

        if (file.Content.Length is 0 or > LongestFile || await HasAsync(id, Told(returned), cancellation))
        {
            if (file.Content.Length is 0 or > LongestFile)
            {
                logger.LogWarning("Left out a file of run {Run} of {Size} bytes", id, file.Content.Length);
            }

            await MarkAsync(id, returned.Sequence, cancellation);
            return;
        }

        var name = AgentAttachments.TidyName(file.Name);
        var type = AgentAttachments.TidyType(file.MediaType, name);

        var key = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));

        await files.WriteAsync(FileStore.AgentFiles, key, file.Content, cancellation);

        await records.AddFileAsync(new AgentFileEntity
        {
            Id = key,
            Direction = AgentFileDirection.Returned,
            RunId = id,
            LambdaId = run?.LambdaId,
            Name = name,
            MediaType = type,
            Size = file.Content.Length,
            Sha256 = Convert.ToHexStringLower(SHA256.HashData(file.Content.Span)),
            Description = Clip(file.Description, 400),
            Created = at
        }, cancellation);

        await records.AppendAsync(id, run?.LambdaId, AgentRunEventKind.Returned, new AgentStep(0, "file", Files: [name], Path: key, Done: true),
                                  Told(returned), at, cancellation);

        await MarkAsync(id, returned.Sequence, cancellation);

        logger.LogInformation("Kept file '{Name}' of run {Run} type {Type} size {Size}", name, id, type, file.Content.Length);
    }

    /// <summary>
    /// Ends a run the platform gave up on.
    /// </summary>
    public async ValueTask EndAsync(string id, AgentEnding ending, string detail, CancellationToken cancellation = default)
    {
        await records.UpdateAsync(id, run =>
        {
            if (run.State == AgentRunState.Ended)
            {
                return;
            }

            run.State = AgentRunState.Ended;
            run.Ending = ending;
            run.Detail = detail;
            run.Ended = DateTime.UtcNow;
            run.Position = 0;
            run.WaitReason = null;
        }, cancellation);
    }

    private static string? Clip(string? text, int longest)
        => text == null ? null : text.Length > longest ? text[..longest] + "…" : text;

    #endregion

    #region Lifecycle

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        await _stopping.CancelAsync();

        try
        {
            await Task.WhenAll(_following.Values).WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (Exception)
        {
            // loops that did not stop in time must not hold up the shutdown
        }

        // another server - or this one, started again - takes them over at once
        try
        {
            using var patience = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            await records.ReleaseAsync(_me, patience.Token);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to let go of the runs this server followed; they are taken over once their claims lapse");
        }

        _stopping.Dispose();
    }

    #endregion

}
