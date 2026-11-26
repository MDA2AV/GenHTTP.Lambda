using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Integrations.Agents;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Secrets;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// What a page shows of a run: where it has got to, what it did, and how it
/// ended - from the platform's records of what its calls did, never from
/// the agent's account of itself.
/// </summary>
/// <remarks>
/// <para>
/// The agent is a client like any other, so what a run saved, put online,
/// started or switched on is what the services recorded as they did it for
/// the run (<see cref="IAgentJournal"/>). The agent's own words are what it
/// wrote for the person - its closing note, its answer, its question - and
/// are shown as that. So a run that ended in an error with a draft half done
/// shows the draft, a run that says it put something online and did not
/// shows that it did not, and a run that failed before it wrote a line
/// claims nothing.
/// </para>
/// <para>
/// What waits after a run - the operator's decision on connecting, keys the
/// code reads that nobody entered - is read from the lambda as it is now.
/// The English sentences here are for the log and for a client with no words
/// of its own; the pages say the facts in their visitor's language.
/// </para>
/// </remarks>
public sealed class AgentOutcome(AgentRunRecords records, AgentFollower follower, IMetaService meta, IFeatureService features, ISecretService secrets,
                                 IConnectivityService connectivity, LambdaOptions options)
{

    #region Reading a run

    /// <summary>
    /// Everything the records hold of a run, read once for whatever is made of it.
    /// </summary>
    public async ValueTask<RunRecord> ReadAsync(AgentRunEntity run, CancellationToken cancellation = default)
    {
        var events = await records.EventsAsync(run.Id, cancellation);
        var files = await records.FilesOfAsync([run.Id], cancellation);

        return new RunRecord(run, events, files);
    }

    /// <summary>
    /// A run with what its events and files are.
    /// </summary>
    public sealed record RunRecord(AgentRunEntity Run, IReadOnlyList<AgentRunEventEntity> Events, IReadOnlyList<AgentFileEntity> Files)
    {

        public IReadOnlyList<AgentEffect> Effects { get; } = [.. Events.Where(e => e.Kind == AgentRunEventKind.Effect).Select(AgentRunRecords.Read<AgentEffect>).OfType<AgentEffect>()];

        public IReadOnlyDictionary<string, string> Options { get; } = StoredOptions.Read(Run.Options);

    }

    #endregion

    #region Progress

    /// <summary>
    /// How a build is getting on, as /build follows it.
    /// </summary>
    public async ValueTask<BuildProgress> BuildAsync(AgentRunEntity run, CancellationToken cancellation = default)
    {
        var read = await ReadAsync(run, cancellation);

        var result = run.State == AgentRunState.Ended ? await BuildResultAsync(read, cancellation) : null;

        return new BuildProgress(StateOf(read, result?.Ok == true || result?.Asked == true), StepsOf(read), result, WaitingOf(run), SecondsOf(run), run.TimeLimit,
                                 run.Id, WaitOf(run), read.Options, AttachedOf(read), ReturnedOf(read));
    }

    /// <summary>
    /// How a message about a lambda is getting on, as the editor's Change
    /// section and the workspace follow it.
    /// </summary>
    public async ValueTask<ChangeProgress> ChangeAsync(AgentRunEntity run, CancellationToken cancellation = default)
    {
        var read = await ReadAsync(run, cancellation);

        var result = run.State == AgentRunState.Ended ? await ChangeResultAsync(read, cancellation) : null;

        var done = result is { } r && (r.Ok || r.Asked == true || (r.Unchanged == true && r.Summary != null && r.Reason is null or "declined"));

        read.Options.TryGetValue(AgentModelNames.ModelOption, out var model);
        read.Options.TryGetValue(AgentModelNames.EffortOption, out var effort);

        return new ChangeProgress(run.Id, StateOf(read, done), run.Prompt, run.Deploy, model, run.Before, StepsOf(read), result, WaitingOf(run),
                                  SecondsOf(run), run.TimeLimit, run.Feature, effort, read.Options, WaitOf(run), AttachedOf(read), ReturnedOf(read),
                                  run.Continues, run.Created);
    }

    private static string StateOf(RunRecord read, bool done) => read.Run.State switch
    {
        AgentRunState.Starting or AgentRunState.Queued => "queued",
        AgentRunState.Working or AgentRunState.Waiting => "running",
        _ => read.Run.Ending == AgentEnding.Cancelled ? "cancelled" : done ? "done" : "failed"
    };

    private static int WaitingOf(AgentRunEntity run)
        => run.State is AgentRunState.Starting or AgentRunState.Queued ? Math.Max(1, run.Position) : 0;

    private static int SecondsOf(AgentRunEntity run)
        => run.Started is { } started ? (int)Math.Max(0, ((run.Ended ?? DateTime.UtcNow) - started).TotalSeconds) : 0;

    private RunWait? WaitOf(AgentRunEntity run)
    {
        if (run.State == AgentRunState.Ended)
        {
            return null;
        }

        if (follower.UnreachableSince(run.Id) != null)
        {
            return new RunWait("unreachable");
        }

        return run.State switch
        {
            AgentRunState.Starting or AgentRunState.Queued => new RunWait("queue", Math.Max(1, run.Position)),
            AgentRunState.Waiting => new RunWait(WaitNameOf(run.WaitReason), Until: run.WaitUntil),
            _ => null
        };
    }

    private static string WaitNameOf(AgentWaitReason? reason) => reason switch
    {
        AgentWaitReason.Overloaded => "overloaded",
        AgentWaitReason.RateLimited => "rate-limited",
        AgentWaitReason.Preparing => "preparing",
        _ => "other"
    };

    /// <summary>
    /// The steps of a run as the pages draw them: what the platform saw its
    /// calls do, what the agent said and did beyond it, and the files it
    /// handed back, in the order they happened.
    /// </summary>
    private static IReadOnlyList<AgentStep> StepsOf(RunRecord read)
    {
        var from = read.Run.Started ?? read.Run.Created;

        var steps = new List<AgentStep>();

        // by when they happened rather than when they were recorded: a line the
        // agent said before a call reaches the platform through its system,
        // the call itself straight away - so it may be recorded after it
        foreach (var happened in read.Events.OrderBy(e => e.At))
        {
            if (happened.Kind == AgentRunEventKind.Effect)
            {
                continue;
            }

            if (AgentRunRecords.Read<AgentStep>(happened) is { } step)
            {
                steps.Add(step with { At = (int)Math.Max(0, (happened.At - from).TotalSeconds) });
            }
        }

        // the closing note is the result's; shown again as the last step it
        // would be said twice
        if (read.Run.State == AgentRunState.Ended && steps.Count > 0 && steps[^1] is { Kind: "say", Text: { } last } && read.Run.Message is { } message
            && message.Contains(last.TrimEnd('…'), StringComparison.Ordinal))
        {
            steps.RemoveAt(steps.Count - 1);
        }

        // a long run keeps its most recent steps, which are the ones anybody
        // is still reading
        return steps.Count > 200 ? steps[^200..] : steps;
    }

    private static IReadOnlyList<RunFile> AttachedOf(RunRecord read)
        => [.. read.Files.Where(f => f.Direction == AgentFileDirection.Attached).Select(AgentAttachments.Show)];

    private static IReadOnlyList<RunFile> ReturnedOf(RunRecord read)
        => [.. read.Files.Where(f => f.Direction == AgentFileDirection.Returned).Select(AgentAttachments.Show)];

    #endregion

    #region What a run did

    /// <summary>
    /// What a run did to the lambda it was about, folded from what was
    /// recorded as it did it.
    /// </summary>
    public sealed record Deeds(int? Saved, int? Online, bool Wrote, bool Started, string? Feature, string? FeatureName, IReadOnlyList<string> Settings,
                               IReadOnlyList<long> Created);

    public static Deeds DeedsOf(RunRecord read)
    {
        var lambda = read.Run.LambdaId;

        int? saved = null, online = null;

        var wrote = false;
        var started = false;

        string? feature = read.Run.Feature, name = null;

        var settings = new List<string>();
        var created = new List<long>();

        foreach (var effect in read.Effects)
        {
            var own = effect.LambdaId == lambda;

            switch (effect.Kind)
            {
                case AgentEffectKind.Created:
                    created.Add(effect.LambdaId);
                    break;

                case AgentEffectKind.Saved when own:
                    wrote = true;
                    saved = Math.Max(saved ?? 0, effect.Version ?? 0);
                    online = effect.Online == true ? effect.Version : online;
                    break;

                case AgentEffectKind.Merged when own:
                    wrote = true;
                    saved = Math.Max(saved ?? 0, effect.Version ?? 0);
                    online = effect.Online == true ? effect.Version : online;

                    if (effect.Feature == feature)
                    {
                        feature = null;
                        name = null;
                    }

                    break;

                case AgentEffectKind.Deployed when own:
                    online = effect.Version;
                    break;

                case AgentEffectKind.Started when own:
                    started = true;
                    feature = effect.Feature;
                    name = effect.Name;
                    break;

                case AgentEffectKind.Drafted when own:
                    wrote = true;
                    feature = effect.Feature;
                    name = effect.Name ?? name;
                    break;

                case AgentEffectKind.Discarded when own:
                    settings.Add("discarded");

                    if (effect.Feature == feature)
                    {
                        feature = null;
                        name = null;
                    }

                    break;

                case AgentEffectKind.Enabled when own && effect.Name != null:
                    settings.Add(effect.Name);
                    break;

                case AgentEffectKind.Counted when own:
                    settings.Add("analytics");
                    break;

                case AgentEffectKind.Published when own:
                    settings.Add("source");
                    break;

                case AgentEffectKind.Requested when own:
                    settings.Add("connectivity");
                    break;

                case AgentEffectKind.Showcased when own:
                    settings.Add("showcase");
                    break;

                case AgentEffectKind.Viewed when own:
                    settings.Add("view");
                    break;
            }
        }

        return new Deeds(saved == 0 ? null : saved, online, wrote, started, feature, name, [.. settings.Distinct()], created);
    }

    /// <summary>
    /// Whether the last code a run had compiled compiles, where it is known:
    /// from what the platform answered its saves and checks.
    /// </summary>
    private static bool? CompilesOf(RunRecord read)
    {
        bool? compiles = null;

        foreach (var happened in read.Events.Where(e => e.Kind == AgentRunEventKind.Step))
        {
            if (AgentRunRecords.Read<AgentStep>(happened) is not { Done: true } step || step.Kind is not ("write" or "check" or "deploy" or "merge"))
            {
                continue;
            }

            if (step.Errors is { } errors)
            {
                compiles = errors == 0;
            }
            else if (step.Online == true)
            {
                compiles = true;
            }
        }

        return compiles;
    }

    #endregion

    #region Results

    private async ValueTask<ChangeResult> ChangeResultAsync(RunRecord read, CancellationToken cancellation)
    {
        var run = read.Run;
        var deeds = DeedsOf(read);

        var acted = deeds.Wrote || deeds.Online != null || deeds.Settings.Count > 0 || deeds.Created.Count > (run.Kind == AgentRunKind.Build ? 1 : 0);

        var ending = run.Ending ?? AgentEnding.Failed;

        var declined = ending == AgentEnding.Declined;
        var asked = ending == AgentEnding.Asked;

        var reason = ReasonOf(ending) ?? (!acted && run.Message == null ? "nothing" : null);

        var privateKey = run.LambdaId is { } id ? await PrivateKeyOfAsync(id, cancellation) : null;

        // the draft it left, as it is now: still there, and whether its
        // preview is online to be tried
        string? feature = null, featureName = null;
        bool? preview = null;

        if (deeds.Wrote && deeds.Feature != null && privateKey != null)
        {
            var found = (await features.ListAsync(privateKey, cancellation)).FirstOrDefault(f => f.Key == deeds.Feature);

            if (found != null)
            {
                feature = found.Key;
                featureName = found.Name;
                preview = found.Online;
            }
        }

        var created = new List<string>();
        var apps = new List<CreatedApp>();

        foreach (var made in deeds.Created.Where(c => c != run.LambdaId))
        {
            if (await records.KeysOfAsync(made, cancellation) is { } keys)
            {
                created.Add(keys.PublicKey);

                var info = await meta.GetAsync(keys.PrivateKey, cancellation);

                apps.Add(new CreatedApp(keys.PublicKey, keys.PrivateKey, info?.PublicUrl, EditorOf(keys.PrivateKey)));
            }
        }

        return new ChangeResult(
            Ok: acted && !declined,
            Version: deeds.Saved,
            Online: deeds.Online,
            Deployed: deeds.Online != null,
            Compiles: CompilesOf(read),
            Before: run.Before,
            Unchanged: acted ? null : true,
            Cancelled: ending == AgentEnding.Cancelled ? true : null,
            Reason: declined ? "declined" : reason,
            Summary: run.Message,
            Error: acted ? null : ErrorOf(ending, run.Message, change: true),
            Detail: acted && ending is AgentEnding.Completed ? null : run.Detail,
            Feature: feature,
            FeatureName: featureName,
            Preview: preview,
            Declined: declined ? true : null,
            // a question it answered - perhaps switching a setting on for it -
            // rather than a change it made: no draft, nothing saved or put online
            Answered: ending == AgentEnding.Completed && !deeds.Wrote && !deeds.Started && deeds.Online == null && run.Message != null ? true : null,
            Settings: deeds.Settings.Count > 0 ? deeds.Settings : null,
            Asked: asked ? true : null,
            Question: asked ? run.Message : null,
            // what the app waits for is said where the run had to do with it:
            // code it wrote may read a key, a request it made waits for the
            // operator - an answered question waits for nothing
            WaitsFor: privateKey != null && (deeds.Wrote || deeds.Settings.Contains("connectivity") || deeds.Settings.Contains("secrets"))
                ? await WaitsOfAsync(privateKey, cancellation)
                : null,
            Created: created.Count > 0 ? created : null,
            Apps: apps.Count > 0 ? apps : null);
    }

    private async ValueTask<BuildResult> BuildResultAsync(RunRecord read, CancellationToken cancellation)
    {
        var run = read.Run;
        var deeds = DeedsOf(read);

        var ending = run.Ending ?? AgentEnding.Failed;

        var privateKey = run.LambdaId is { } id ? (await records.KeysOfAsync(id, cancellation))?.PrivateKey : null;

        var info = privateKey != null ? await meta.GetAsync(privateKey, cancellation) : null;

        if (ending == AgentEnding.Declined)
        {
            // a lambda it made before it saw what the request was stays theirs
            return new BuildResult(false, null, info?.PublicKey, info?.PrivateKey, null, null, null, null, run.Message, run.Detail, Declined: true, Reason: "declined");
        }

        if (ending == AgentEnding.Asked)
        {
            return new BuildResult(false, null, info?.PublicKey, info?.PrivateKey, info?.PublicUrl, EditorOf(info?.PrivateKey), run.Message, null, null, null,
                                   Asked: true, Question: run.Message);
        }

        /*
         * A build that never wrote code is a failure however cheerfully it
         * ended: creating a lambda seeds the starter, so the address answers
         * and what is online is "It works" - which was once reported as a
         * success to somebody who had asked for a game.
         */
        if (info == null || !deeds.Wrote)
        {
            return new BuildResult(false, null, info?.PublicKey, info?.PrivateKey, null, null, null, null, ErrorOf(ending, run.Message, change: false),
                                   run.Detail ?? run.Message, Reason: ReasonOf(ending));
        }

        var waits = await WaitsOfAsync(info.PrivateKey, cancellation);

        return new BuildResult(true, deeds.Online != null, info.PublicKey, info.PrivateKey, info.PublicUrl, EditorOf(info.PrivateKey),
                               run.Message, "The editor link is the only way back in. There is no way to recover it.", null, null,
                               WaitsFor: waits, Reason: ReasonOf(ending));
    }

    private string? EditorOf(string? privateKey)
        => privateKey == null ? null : $"{options.PublicUrl?.TrimEnd('/')}{PlatformRoutes.EditorOf(privateKey)}";

    private async ValueTask<string?> PrivateKeyOfAsync(long lambda, CancellationToken cancellation)
        => (await records.KeysOfAsync(lambda, cancellation))?.PrivateKey;

    /// <summary>
    /// What the app waits for, and whom - as it is now.
    /// </summary>
    private async ValueTask<IReadOnlyList<RunBlocker>?> WaitsOfAsync(string privateKey, CancellationToken cancellation)
    {
        var waits = new List<RunBlocker>();

        try
        {
            if ((await connectivity.GetAsync(privateKey, cancellation)).Status == ConnectivityStatus.Requested)
            {
                waits.Add(new RunBlocker("operator", "connectivity"));
            }

            var keys = await secrets.ListAsync(privateKey, cancellation: cancellation);

            if (keys.Missing.Count > 0)
            {
                waits.Add(new RunBlocker("owner", "keys", keys.Missing));
            }
        }
        catch (LambdaException)
        {
            // gone or blocked meanwhile: nothing waits that anybody could do
        }

        return waits.Count > 0 ? waits : null;
    }

    public static string? ReasonOf(AgentEnding ending) => ending switch
    {
        AgentEnding.TimedOut => "timeout",
        AgentEnding.OutOfSteps => "turns",
        AgentEnding.SignInFailed => "unauthorised",
        AgentEnding.Lost => "lost",
        AgentEnding.Cancelled => "cancelled",
        AgentEnding.Failed => "failed",
        _ => null
    };

    /// <summary>
    /// Why a run did nothing, in English. Why matters: a run that cannot sign
    /// in dies in a second having done nothing at all, and telling the person
    /// to ask for something smaller blames their request for this server's
    /// credentials.
    /// </summary>
    private static string ErrorOf(AgentEnding ending, string? message, bool change) => ending switch
    {
        AgentEnding.SignInFailed => "The agent could not sign in, so nothing ran. That is this server's credentials rather than anything about what you asked for.",
        AgentEnding.TimedOut => change ? "It ran out of time before it changed anything." : "It ran out of time before it wrote anything. Try asking for something smaller, or ask again - it gets further some runs than others.",
        AgentEnding.OutOfSteps => change ? "It used up its steps before it changed anything." : "It used up its steps before it wrote anything. Try asking for something smaller.",
        AgentEnding.Lost => "The agent lost track of this request, so it stopped. What it did before is kept.",
        AgentEnding.Failed => change ? "It stopped with an error before it changed anything." : "The builder stopped with an error before it wrote anything.",
        AgentEnding.Cancelled => "It was stopped.",
        _ => change ? "Nothing was changed." : message == null ? "The build did not produce anything that could be put online." : "It did not write anything."
    };

    #endregion

}
