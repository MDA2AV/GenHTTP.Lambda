using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Integrations.Agents;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Meta.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// Hands what somebody wrote to the installation's agent, and keeps what came
/// of it.
/// </summary>
/// <remarks>
/// <para>
/// Two kinds of run go through it. A build makes something new for whoever
/// typed into the box on /build. A message about a lambda comes from whoever
/// holds its editor key - in the workspace or the editor's Change section -
/// and is a question to answer, a setting to change or a change to make.
/// Both share the daily allowance of an address, since both spend what the
/// agent runs on.
/// </para>
/// <para>
/// Which agent runs them is the installation's agent integration
/// (<see cref="IAgentProvider"/>); this decides what may be asked for, on
/// which options and in which order things are checked, and keeps every run
/// in the platform's records - so a page closed and opened again, or a
/// platform restarted, finds the run where it was. The units it orchestrates:
/// <see cref="AgentGate"/> says who may have which values of the options,
/// <see cref="BuildAllowance"/> counts the runs of each address,
/// <see cref="AgentAttachments"/> keeps the files, <see cref="AgentContext"/>
/// puts together what the agent is handed, <see cref="AgentFollower"/> takes
/// what it tells into the records, <see cref="AgentOutcome"/> reads what came
/// of a run from them and <see cref="AgentUndo"/> undoes it.
/// </para>
/// <para>
/// Without an integration the installation has no agent: the boxes are not
/// offered and every call here is answered with 404. With one, the operator
/// can still take either box away in the panel.
/// </para>
/// </remarks>
public sealed class AgentService(AgentSettingsStore settings, AgentGate gate, BuildAllowance allowance, UploadAllowance uploads, AgentCredentials credentials,
                                 AgentRunRecords records,
                                 AgentAttachments attachments, AgentContext context, AgentFollower follower, AgentOutcome outcome, AgentUndo undo,
                                 AgentSignals signals, IMetaService meta, ILogger<AgentService> logger,
                                 IAgentProvider? provider = null) : IAgentService
{

    #region What there is

    public bool Available => provider != null;

    public AgentCapabilities? Capabilities => provider?.Capabilities;

    public int PerDay => allowance.PerDay;

    public bool PasswordSet => gate.PasswordSet;

    public bool BuildsOffered() => Available && settings.Get().Builds;

    public bool ChangesOffered() => Available && settings.Get().Changes;

    public AgentOffer Offer()
    {
        if (provider?.Capabilities is not { } capabilities)
        {
            return new AgentOffer([], [], null, [], null, Cancels: false);
        }

        var offered = gate.Offer(capabilities);

        var models = offered.FirstOrDefault(o => o.Name == AgentModelNames.ModelOption);
        var efforts = offered.FirstOrDefault(o => o.Name == AgentModelNames.EffortOption);

        var policy = capabilities.Attachments is { } taken
            ? new AttachmentPolicy(taken.MediaTypes, Math.Min(taken.MaxBytes, AgentAttachments.LongestFile), taken.MaxCount)
            : null;

        return new AgentOffer(offered, models?.Values ?? [], models?.Default, [.. efforts?.Values.Select(v => v.Name) ?? []], efforts?.Default, policy,
                              capabilities.ReturnsFiles, WebNameOf(capabilities.Web.Reads), capabilities.Web.Searches, capabilities.Cancels,
                              capabilities.Asks);
    }

    private static string WebNameOf(AgentWebScope scope) => scope switch
    {
        AgentWebScope.NamedSites => "named",
        AgentWebScope.Any => "any",
        _ => "none"
    };

    private IAgentProvider Require()
        => provider ?? throw new ProviderException(ResponseStatus.NotFound, "This installation does not have an agent.");

    private AgentCapabilities CapabilitiesOf(IAgentProvider agent) => agent.Capabilities;

    #endregion

    #region Who calls

    public ValueTask<VersionAuthor?> AuthorOfAsync(string? credential, CancellationToken cancellation = default)
        => credentials.ResolveAsync(credential, cancellation);

    #endregion

    #region The operator's

    public AgentSettings GetSettings() => settings.Get();

    /// <remarks>
    /// Applies to the next run asked for and the next page drawn; a run under
    /// way goes on as it was asked for.
    /// </remarks>
    public ValueTask<AgentSettings> ChangeSettingsAsync(AgentSettingsChange change, CancellationToken cancellation = default)
        => settings.SaveAsync(AgentGate.Change(settings.Get(), provider?.Capabilities, change), cancellation);

    public AgentLimits LimitsOf(bool guarded)
        => provider?.Capabilities is { } capabilities ? gate.LimitsOf(capabilities, guarded) : new AgentLimits(null, null);

    public async ValueTask<AgentSystem?> SystemAsync(CancellationToken cancellation = default)
    {
        if (provider == null)
        {
            return null;
        }

        var load = await provider.LoadAsync(cancellation);

        return new AgentSystem(load.Reachable, load.Running, load.Queued, load.Capacity, [.. load.Facts.Select(f => new AgentFact(f.Label, f.Value))]);
    }

    public async ValueTask<AgentUsageSummary> UsageAsync(int days, CancellationToken cancellation = default)
    {
        var since = DateTime.UtcNow.Date.AddDays(1 - Math.Max(1, days));

        var runs = await records.SinceAsync(since, cancellation);

        var counted = new Dictionary<string, Dictionary<string, (int Runs, long Seconds)>>(StringComparer.Ordinal);

        long total = 0;

        foreach (var (stored, _, _, started, ended) in runs)
        {
            var seconds = started is { } from && ended is { } to ? (long)Math.Max(0, (to - from).TotalSeconds) : 0;

            total += seconds;

            foreach (var (option, value) in StoredOptions.Read(stored))
            {
                if (!counted.TryGetValue(option, out var values))
                {
                    counted[option] = values = new Dictionary<string, (int, long)>(StringComparer.Ordinal);
                }

                var (n, s) = values.GetValueOrDefault(value);

                values[value] = (n + 1, s + seconds);
            }
        }

        return new AgentUsageSummary(days, runs.Count, runs.Count(r => r.Kind == AgentRunKind.Build), total,
                                     [.. counted.OrderBy(c => c.Key, StringComparer.Ordinal)
                                                .Select(c => new OptionUsage(c.Key, [.. c.Value.OrderByDescending(v => v.Value.Runs)
                                                                                                 .Select(v => new ValueUsage(v.Key, v.Value.Runs, v.Value.Seconds))]))]);
    }

    public async ValueTask<IReadOnlyList<RunListing>> NewestAsync(int take, CancellationToken cancellation = default)
    {
        var runs = await records.NewestAsync(take, cancellation);

        var files = await records.FilesOfAsync([.. runs.Select(r => r.Id)], cancellation);

        var listed = new List<RunListing>();

        foreach (var run in runs)
        {
            var lambda = run.LambdaId is { } id ? (await records.KeysOfAsync(id))?.PublicKey : null;

            var own = files.Where(f => f.RunId == run.Id).ToList();

            listed.Add(new RunListing(run.Id[..8], AgentRunKindNames.Name(run.Kind), lambda, StateNameOf(run.State), EndingNameOf(run.Ending),
                                      StoredOptions.Read(run.Options), run.Created,
                                      run.Started is { } started ? (int)Math.Max(0, ((run.Ended ?? DateTime.UtcNow) - started).TotalSeconds) : 0,
                                      run.Prompt,
                                      [.. own.Where(f => f.Direction == AgentFileDirection.Attached).Select(AgentAttachments.Show)],
                                      [.. own.Where(f => f.Direction == AgentFileDirection.Returned).Select(AgentAttachments.Show)],
                                      run.InputTokens, run.OutputTokens, run.Cost, run.Currency, run.Detail));
        }

        return listed;
    }

    private static string StateNameOf(AgentRunState state) => state switch
    {
        AgentRunState.Starting or AgentRunState.Queued => "queued",
        AgentRunState.Working => "running",
        AgentRunState.Waiting => "waiting",
        _ => "ended"
    };

    public static string? EndingNameOf(AgentEnding? ending) => ending switch
    {
        AgentEnding.Completed => "completed",
        AgentEnding.Asked => "asked",
        AgentEnding.Declined => "declined",
        AgentEnding.Cancelled => "cancelled",
        AgentEnding.TimedOut => "timed-out",
        AgentEnding.OutOfSteps => "out-of-steps",
        AgentEnding.SignInFailed => "sign-in-failed",
        AgentEnding.Failed => "failed",
        AgentEnding.Lost => "lost",
        _ => null
    };

    #endregion

    #region Files

    public async ValueTask<RunFile> AttachAsync(string? name, string? type, ReadOnlyMemory<byte> content, IPAddress? caller,
                                                CancellationToken cancellation = default)
    {
        var agent = Require();

        if (!BuildsOffered() && !ChangesOffered())
        {
            throw new ProviderException(ResponseStatus.NotFound, "Nothing is asked of the agent from here on this installation.");
        }

        if (!uploads.Spend(caller, content.Length))
        {
            throw new ProviderException(ResponseStatus.TooManyRequests, "This address sent as many files as it may in an hour. Try again later.");
        }

        var file = await attachments.AttachAsync(CapabilitiesOf(agent).Attachments, name, type, content, cancellation);

        // as the prompts are: what came, how large, and what it holds by its hash
        logger.LogInformation("Received attachment {File} name '{Name}' type {Type} size {Size} sha256 {Sha256}", file.Id, file.Name, file.MediaType,
                              file.Size, file.Sha256);

        return AgentAttachments.Show(file);
    }

    public async ValueTask<(RunFile File, byte[] Content)?> ReadFileAsync(string? id, CancellationToken cancellation = default)
        => await attachments.ReadAsync(id, cancellation) is { } read ? (AgentAttachments.Show(read.File), read.Content) : null;

    public async ValueTask<(RunFile File, byte[] Content)?> ReadFileAsync(long lambda, string? id, CancellationToken cancellation = default)
        => await attachments.ReadAsync(id, cancellation) is { } read && read.File.LambdaId == lambda ? (AgentAttachments.Show(read.File), read.Content) : null;

    public async ValueTask<IReadOnlyList<RunFile>> AttachedToAsync(long lambda, CancellationToken cancellation = default)
        => [.. (await records.AttachedToAsync(lambda, AgentAttachments.Earlier, cancellation)).Select(AgentAttachments.Show)];

    #endregion

    #region Builds

    public async ValueTask<BuildStarted> StartAsync(AgentAsk ask, IPAddress? caller)
    {
        var agent = Require();

        if (!BuildsOffered())
        {
            throw new ProviderException(ResponseStatus.NotFound, "Nothing is built from here on this installation.");
        }

        var previous = await ContinuedAsync(ask.Continues, null);

        var wanted = PromptOf(ask, previous, "Say what you would like built.");

        var choice = gate.Choose(CapabilitiesOf(agent), ask.Options, ask.Password);

        if (!allowance.Spend(caller))
        {
            throw new ProviderException(ResponseStatus.TooManyRequests,
                $"That is {PerDay} builds today, which is all this offers for now. "
              + "The editor is still there, and so is the MCP if you have an agent of your own.");
        }

        var run = NewRun(AgentRunKind.Build, wanted, choice, ask, previous, lambda: null, before: null, view: ViewOf(ask.View, EditorView.Simple));

        var position = await HandOverAsync(agent, run, ask, previous, null, caller);

        // the door logs it, with what was asked
        choice.Options.TryGetValue(AgentModelNames.ModelOption, out var model);
        choice.Options.TryGetValue(AgentModelNames.EffortOption, out var effort);

        return new BuildStarted(run.Entity.Id, position, allowance.Left(caller), model, effort, choice.Options);
    }

    public async ValueTask<BuildProgress> ProgressAsync(string id)
    {
        Require();

        var run = await BuildOfAsync(id);

        return await outcome.BuildAsync(run);
    }

    public async IAsyncEnumerable<BuildProgress?> FollowBuildAsync(string id, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        Require();

        var run = await BuildOfAsync(id);

        string? last = null;

        while (!cancellation.IsCancellationRequested)
        {
            var next = signals.NextOfRun(run.Id);

            var current = await records.GetAsync(run.Id, cancellation);

            if (current == null)
            {
                yield break;
            }

            var progress = await outcome.BuildAsync(current, cancellation);

            var said = JsonSerializer.Serialize(progress, AgentRunRecords.Format);

            if (said != last)
            {
                last = said;
                yield return progress;
            }
            else
            {
                yield return null;
            }

            if (current.State == AgentRunState.Ended)
            {
                yield break;
            }

            await WaitAsync(next, cancellation);
        }
    }

    /// <summary>
    /// A build, by its id - never a change, which belongs to whoever holds
    /// the editor key and is asked for with it, under its lambda.
    /// </summary>
    private async ValueTask<AgentRunEntity> BuildOfAsync(string id)
    {
        if (!Guid.TryParse(id, out var parsed))
        {
            throw new ProviderException(ResponseStatus.BadRequest, "That is not a build.");
        }

        var run = await records.GetAsync(parsed.ToString("D"));

        return run is { Kind: AgentRunKind.Build }
            ? run
            : throw new ProviderException(ResponseStatus.NotFound, "There is no build by that name any more.");
    }

    #endregion

    #region Messages about a lambda

    public async ValueTask<AgentState> StateAsync(long lambda, IPAddress? caller)
    {
        if (!ChangesOffered())
        {
            return new AgentState(false, PerDay, 0, Offer(), null);
        }

        var newest = await records.NewestShownAsync(lambda);

        return new AgentState(true, PerDay, allowance.Left(caller), Offer(), newest != null ? await outcome.ChangeAsync(newest) : null);
    }

    public async IAsyncEnumerable<AgentState?> FollowAsync(long lambda, IPAddress? caller, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        string? last = null;

        while (!cancellation.IsCancellationRequested)
        {
            var next = signals.NextOfLambda(lambda);

            var state = await StateAsync(lambda, caller);

            var said = JsonSerializer.Serialize(state, AgentRunRecords.Format);

            if (said != last)
            {
                last = said;
                yield return state;
            }
            else
            {
                yield return null;
            }

            await WaitAsync(next, cancellation);
        }
    }

    /// <remarks>
    /// A question costs what a change costs: the agent runs the same way for
    /// either, and the allowance defends what it runs on.
    /// </remarks>
    public async ValueTask<AgentState> ChangeAsync(long lambda, string privateKey, int? before, AgentAsk ask, IPAddress? caller)
    {
        var agent = Require();

        if (!ChangesOffered())
        {
            throw new ProviderException(ResponseStatus.NotFound, "Changes are not asked for from here on this installation.");
        }

        var previous = await ContinuedAsync(ask.Continues, lambda);

        var wanted = PromptOf(ask, previous, "Say what should be different.");

        var choice = gate.Choose(CapabilitiesOf(agent), ask.Options, ask.Password);

        // asked before anything is spent: a second run of the same lambda is
        // refused, and refusing it should not cost the owner one
        if (await records.NewestOfAsync(lambda) is { State: not AgentRunState.Ended })
        {
            throw new ProviderException(ResponseStatus.Conflict, "A change of this lambda is already under way. Wait for it, or stop it first.");
        }

        if (!allowance.Spend(caller))
        {
            throw new ProviderException(ResponseStatus.TooManyRequests,
                $"That is all {PerDay} builds and changes for today. It starts again at midnight UTC.");
        }

        var info = await meta.GetAsync(privateKey) ?? throw new ProviderException(ResponseStatus.NotFound, "There is no lambda with this key.");

        var view = ViewOf(ask.View, EditorViewNames.TryRead(info.View, out var opens) ? opens : EditorView.Full);

        var run = NewRun(AgentRunKind.Change, wanted, choice, ask, previous, lambda, before, view);

        var target = new AgentLambda(info.PublicKey, privateKey, before, run.Entity.Feature, run.Entity.Deploy);

        await HandOverAsync(agent, run, ask, previous, target, caller);

        // the key is never logged: it is the only thing standing between
        // somebody reading this log and the ability to change the lambda
        // the door logs it, with what was asked

        return await StateAsync(lambda, caller);
    }

    public async ValueTask<AgentState> StopAsync(long lambda, IPAddress? caller)
    {
        var agent = Require();

        if (await records.NewestOfAsync(lambda) is { State: not AgentRunState.Ended } current)
        {
            await StopRunAsync(agent, current);
        }

        return await StateAsync(lambda, caller);
    }

    public async ValueTask<BuildProgress> StopBuildAsync(string id)
    {
        var agent = Require();

        var run = await BuildOfAsync(id);

        if (run.State != AgentRunState.Ended)
        {
            await StopRunAsync(agent, run);
        }

        return await outcome.BuildAsync((await records.GetAsync(run.Id))!);
    }

    /// <summary>
    /// Stops a run: one that only waited is over at once, one the agent system
    /// does not know (any more) as well, one at work once the agent system says
    /// it is - and one the agent cannot stop is refused rather than said to be.
    /// </summary>
    private async ValueTask StopRunAsync(IAgentProvider agent, AgentRunEntity current)
    {
        if (!CapabilitiesOf(agent).Cancels)
        {
            throw new ProviderException(ResponseStatus.Conflict, "The agent of this installation cannot be stopped. The run ends by itself.");
        }

        var known = await agent.CancelAsync(new AgentRunHandle(current.Id, current.Reference));

        // one that only waited is off the agent system's queue at once, and
        // one it has no run of any more is over as far as anybody here is
        // concerned; one at work ends once the agent system says it did
        if (!known || current.State is AgentRunState.Starting or AgentRunState.Queued)
        {
            await follower.EndAsync(current.Id, AgentEnding.Cancelled, known ? "Stopped while it waited." : "Stopped before the agent knew of it.");
        }

        logger.LogInformation("Stopped run {Run}", current.Id);
    }

    public async ValueTask<UndoOutcome> UndoAsync(long lambda, string privateKey, string? run, CancellationToken cancellation = default)
    {
        Require();

        var chosen = run is { Length: > 0 }
            ? await records.GetAsync(run, cancellation)
            : await records.NewestOfAsync(lambda, cancellation);

        if (chosen == null || chosen.LambdaId != lambda || chosen.Kind != AgentRunKind.Change)
        {
            throw new ProviderException(ResponseStatus.NotFound, "There is no such change of this lambda to undo.");
        }

        if (chosen.State != AgentRunState.Ended)
        {
            throw new ProviderException(ResponseStatus.Conflict, "The change is still under way. Stop it first.");
        }

        var read = await outcome.ReadAsync(chosen, cancellation);

        return await undo.UndoAsync(read, privateKey, cancellation);
    }

    #endregion

    #region Starting a run

    private sealed record NewRunOf(AgentRunEntity Entity, string Credential);

    private NewRunOf NewRun(AgentRunKind kind, string prompt, AgentChoice choice, AgentAsk ask, AgentRunEntity? previous, long? lambda, int? before,
                            EditorView view)
    {
        var (credential, hash) = AgentCredentials.Issue();

        var entity = new AgentRunEntity
        {
            Id = Guid.NewGuid().ToString("D"),
            Kind = kind,
            LambdaId = lambda,
            State = AgentRunState.Starting,
            Prompt = prompt,
            Options = StoredOptions.Write(choice.Options),
            Guarded = choice.Guarded,
            Language = AgentInput.Language(ask.Language),
            View = view,
            Deploy = kind == AgentRunKind.Change && ask.Deploy,
            Feature = kind == AgentRunKind.Change ? ask.Feature : null,
            Before = before,
            Continues = previous?.Id,
            Continuation = previous == null ? null : ContinuationOf(previous, ask),
            Position = 1,
            TimeLimit = choice.Limits.Time is { } time ? (int)time.TotalSeconds : null,
            StepLimit = choice.Limits.Steps,
            Ticket = hash,
            Created = DateTime.UtcNow
        };

        return new NewRunOf(entity, credential);
    }

    /// <summary>
    /// Records the run, gives it its files and hands it to the agent system -
    /// or, where the agent system did not take it, takes all of that back,
    /// the allowance it cost included.
    /// </summary>
    /// <returns>Its place in the agent system's queue</returns>
    private async ValueTask<int> HandOverAsync(IAgentProvider agent, NewRunOf run, AgentAsk ask, AgentRunEntity? previous, AgentLambda? target, IPAddress? caller)
    {
        try
        {
            await records.CreateAsync(run.Entity);
        }
        catch (DbUpdateException)
        {
            // another message about the lambda got there first - on another
            // server, or the same one sent twice
            allowance.Refund(caller);

            throw new ProviderException(ResponseStatus.Conflict, "A change of this lambda is already under way. Wait for it, or stop it first.");
        }

        AgentAcceptance accepted;

        try
        {
            await attachments.ClaimAsync(CapabilitiesOf(agent).Attachments, ask.Attachments, run.Entity.Id, run.Entity.LambdaId);

            var handed = await context.MakeAsync(run.Entity, run.Credential, target, ask.History, previous);

            accepted = await agent.StartAsync(handed);
        }
        catch (Exception e)
        {
            await records.ReleaseFilesAsync(run.Entity.Id);
            await records.DeleteAsync(run.Entity.Id);

            // what the agent did not take costs nothing
            allowance.Refund(caller);

            if (e is AgentRefusedException refused)
            {
                logger.LogWarning("The agent did not take run {Run}: {Reason}", run.Entity.Id, refused.Reason);

                throw new ProviderException(refused.Reason == AgentRefusal.Invalid ? ResponseStatus.BadRequest : ResponseStatus.ServiceUnavailable,
                                            refused.Message);
            }

            throw;
        }

        var taken = await records.UpdateAsync(run.Entity.Id, r =>
        {
            r.Reference = accepted.Reference;

            if (r.State == AgentRunState.Starting)
            {
                r.State = accepted.Position > 0 ? AgentRunState.Queued : AgentRunState.Working;
                r.Position = accepted.Position;
                r.Started ??= accepted.Position > 0 ? null : DateTime.UtcNow;
            }
        });

        // stopped while it was handed over: the agent system took it all the
        // same, and is told now
        if (taken?.State == AgentRunState.Ended)
        {
            await agent.CancelAsync(new AgentRunHandle(run.Entity.Id, accepted.Reference));

            return accepted.Position;
        }

        follower.Follow(taken ?? run.Entity);

        return accepted.Position;
    }

    /// <summary>
    /// The earlier run a message carries on from, where it names one: a run
    /// of the same lambda, or a build, that ended.
    /// </summary>
    private async ValueTask<AgentRunEntity?> ContinuedAsync(string? id, long? lambda)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        var previous = Guid.TryParse(id, out var parsed) ? await records.GetAsync(parsed.ToString("D")) : null;

        if (previous == null || (lambda != null && previous.LambdaId != lambda) || (lambda == null && previous.Kind != AgentRunKind.Build))
        {
            throw new ProviderException(ResponseStatus.BadRequest, "There is no such run to carry on from any more.");
        }

        if (previous.State != AgentRunState.Ended)
        {
            throw new ProviderException(ResponseStatus.Conflict, "The run to carry on from is still under way.");
        }

        return previous;
    }

    /// <summary>
    /// What a run is asked: what was written - or, carrying on from an
    /// earlier run with nothing written, what that one was asked, so nobody
    /// has to type it again.
    /// </summary>
    private static string PromptOf(AgentAsk ask, AgentRunEntity? previous, string missing)
        => previous != null && string.IsNullOrWhiteSpace(ask.Prompt) ? previous.Prompt : AgentInput.Prompt(ask.Prompt, missing);

    /// <summary>
    /// Why a run carries on: an answer to a question, a wait that is over,
    /// or one more try at what stopped before it was done.
    /// </summary>
    private static AgentContinuationKind ContinuationOf(AgentRunEntity previous, AgentAsk ask)
        => previous.Ending == AgentEnding.Asked && !string.IsNullOrWhiteSpace(ask.Prompt)
            ? AgentContinuationKind.Answer
            : previous.Ending is AgentEnding.Completed or AgentEnding.Asked
                ? AgentContinuationKind.Unblocked
                : AgentContinuationKind.Retry;

    private static EditorView ViewOf(string? asked, EditorView fallback)
        => EditorViewNames.TryRead(asked, out var view) ? view : fallback;

    private static async ValueTask WaitAsync(Task next, CancellationToken cancellation)
    {
        try
        {
            await next.WaitAsync(AgentSignals.Patience, cancellation);
        }
        catch (TimeoutException)
        {
            // read again anyway: another server may have changed the records
        }
    }

    #endregion

}
