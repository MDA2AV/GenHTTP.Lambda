using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Features;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Showcase;
using GenHTTP.Lambda.Services.Source;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// Undoes what one run did to an app, as one step, where that can be undone -
/// and says what it leaves and why.
/// </summary>
/// <remarks>
/// <para>
/// Read from what the services recorded as they did it for the run
/// (<see cref="AgentEffect"/>), so undoing touches only what the run changed,
/// and only where it is still as the run left it: a version somebody else
/// put online since stays online, a request the operator decided stays
/// decided.
/// </para>
/// <para>
/// A version is never taken back - versions only ever add up - so undoing
/// puts the one that was online before back online, and the run's stay in
/// the history. Nothing that would delete data is undone: a kind of data or
/// analytics switched off deletes what it holds, a file in the workspace may
/// be what visitors saved since. Those are said to stay, with the section of
/// the editor where the owner decides them. A new app is not undone either:
/// it is deleted in its editor.
/// </para>
/// </remarks>
public sealed class AgentUndo(AgentRunRecords records, IMetaService meta, IFeatureService features, IConnectivityService connectivity, ISourceService sources,
                              IShowcaseService showcase, ILogger<AgentUndo> logger)
{

    #region Offering

    /// <summary>
    /// What undoing a run would do, and what it would leave.
    /// </summary>
    public static UndoOffer Offer(AgentOutcome.RunRecord read)
    {
        if (read.Undone != null)
        {
            return new UndoOffer(false, [], ["undone"]);
        }

        var plan = PlanOf(read);

        return new UndoOffer(plan.Undoes.Count > 0, plan.Undoes, plan.Keeps);
    }

    private sealed record Plan(IReadOnlyList<string> Undoes, IReadOnlyList<string> Keeps, int? Online, IReadOnlyList<string> Drafts, EditorView? View);

    private static Plan PlanOf(AgentOutcome.RunRecord read)
    {
        var run = read.Run;
        var lambda = run.LambdaId;

        var undoes = new List<string>();
        var keeps = new List<string>();

        int? online = null;

        var started = new List<string>();
        var ended = new HashSet<string>(StringComparer.Ordinal);

        bool requested = false, published = false, listed = false;

        EditorView? view = null;

        foreach (var effect in read.Effects)
        {
            if (effect.Kind == AgentEffectKind.Created)
            {
                keeps.Add("lambda");
                continue;
            }

            if (effect.LambdaId != lambda)
            {
                continue;
            }

            switch (effect.Kind)
            {
                case AgentEffectKind.Saved:
                case AgentEffectKind.Merged:
                    keeps.Add("versions");
                    online = effect.Online == true ? effect.Version : online;

                    if (effect is { Kind: AgentEffectKind.Merged, Feature: { } merged })
                    {
                        ended.Add(merged);
                    }

                    break;

                case AgentEffectKind.Deployed:
                    online = effect.Version;
                    break;

                case AgentEffectKind.Started when effect.Feature != null:
                    started.Add(effect.Feature);
                    break;

                case AgentEffectKind.Drafted when effect.Feature != null && !started.Contains(effect.Feature):
                    keeps.Add("draft");
                    break;

                case AgentEffectKind.Discarded when effect.Feature != null:
                    ended.Add(effect.Feature);
                    break;

                case AgentEffectKind.Enabled:
                    keeps.Add("data");
                    break;

                case AgentEffectKind.Counted:
                    keeps.Add("analytics");
                    break;

                case AgentEffectKind.Wrote:
                case AgentEffectKind.Removed:
                    keeps.Add("workspace");
                    break;

                case AgentEffectKind.Requested when effect.Was != true:
                    requested = true;
                    break;

                case AgentEffectKind.Published when effect.Was != true:
                    published = true;
                    break;

                case AgentEffectKind.Showcased when effect.Was != true:
                    listed = true;
                    break;

                case AgentEffectKind.Viewed when view == null && EditorViewNames.TryRead(effect.Name, out var before):
                    view = before;
                    break;
            }
        }

        // what was online before goes back - where there was something, and
        // the run put something else there
        var back = online != null && run.Before is { } was && was != online ? run.Before : null;

        if (back != null)
        {
            undoes.Add("online");
        }

        var drafts = started.Where(f => !ended.Contains(f)).Distinct().ToList();

        if (drafts.Count > 0)
        {
            undoes.Add("draft");
        }

        if (requested)
        {
            undoes.Add("connectivity");
        }

        if (published)
        {
            undoes.Add("source");
        }

        if (listed)
        {
            undoes.Add("showcase");
        }

        if (view != null)
        {
            undoes.Add("view");
        }

        return new Plan(undoes, [.. keeps.Distinct()], back, drafts, view);
    }

    #endregion

    #region Undoing

    /// <summary>
    /// Undoes what can be undone of a run, as its lambda is now, and keeps
    /// what it did with the run.
    /// </summary>
    /// <param name="privateKey">The editor key of the run's lambda, which whoever undoes it holds</param>
    public async ValueTask<UndoOutcome> UndoAsync(AgentOutcome.RunRecord read, string privateKey, CancellationToken cancellation = default)
    {
        if (read.Undone is { } already)
        {
            return already;
        }

        var plan = PlanOf(read);

        var lambda = await meta.GetAsync(privateKey, cancellation) ?? throw LambdaException.NotFound("There is no lambda with this key.");

        var done = new List<string>();
        var kept = new List<string>(plan.Keeps);

        if (plan.Online is { } back)
        {
            var current = DeedsOnlineOf(read);

            // somebody put another version online since: that one stays
            if (lambda.ActiveVersion != current)
            {
                kept.Add("changed:online");
            }
            else
            {
                // the version before may be gone from the history, or not
                // build any more - what is online then stays, and says so
                try
                {
                    var deployed = await meta.DeployAsync(privateKey, back, VersionOrigin.Api, cancellation: cancellation);

                    if (deployed.Success)
                    {
                        done.Add("online");
                    }
                    else
                    {
                        kept.Add("failed:online");
                    }
                }
                catch (LambdaException)
                {
                    kept.Add("failed:online");
                }
            }
        }

        if (plan.Drafts.Count > 0)
        {
            var open = (await features.ListAsync(privateKey, cancellation)).ToDictionary(f => f.Key, StringComparer.Ordinal);

            var discarded = false;

            foreach (var draft in plan.Drafts.Where(open.ContainsKey))
            {
                // saved into since by somebody else - the owner, another run:
                // their work is not the run's to take back. One nobody saved
                // into since it was started is still the run's own
                var untouched = open[draft].Run == null && open[draft].Revision <= 1;

                if (open[draft].Run != read.Run.Id && !untouched)
                {
                    kept.Add("changed:draft");
                    continue;
                }

                await features.DeleteAsync(privateKey, draft, cancellation: cancellation);

                discarded = true;
            }

            if (discarded)
            {
                done.Add("draft");
            }
        }

        if (plan.Undoes.Contains("connectivity"))
        {
            if ((await connectivity.GetAsync(privateKey, cancellation)).Status == ConnectivityStatus.Requested)
            {
                await connectivity.WithdrawAsync(privateKey, cancellation: cancellation);
                done.Add("connectivity");
            }
            else
            {
                // the operator decided it meanwhile
                kept.Add("changed:connectivity");
            }
        }

        if (plan.Undoes.Contains("source") && await sources.GetAsync(privateKey, cancellation) is { Published: true })
        {
            await sources.WithdrawAsync(privateKey, cancellation: cancellation);
            done.Add("source");
        }

        if (plan.Undoes.Contains("showcase") && await showcase.GetAsync(privateKey, cancellation) != null)
        {
            await showcase.RemoveAsync(privateKey, cancellation: cancellation);
            done.Add("showcase");
        }

        if (plan.View is { } view)
        {
            await meta.ChangeViewAsync(privateKey, view, cancellation: cancellation);
            done.Add("view");
        }

        var online = (await meta.GetAsync(privateKey, cancellation))?.ActiveVersion;

        var outcome = new UndoOutcome(done, [.. kept.Distinct()], online);

        await records.AppendAsync(read.Run.Id, read.Run.LambdaId, AgentRunEventKind.Undone, outcome, cancellation: cancellation);

        logger.LogInformation("Undid run {Run} of lambda {Lambda} undone {Undone} kept {Kept}", read.Run.Id, lambda.PublicKey,
                              string.Join(',', done), string.Join(',', outcome.Kept));

        return outcome;
    }

    private static int? DeedsOnlineOf(AgentOutcome.RunRecord read) => AgentOutcome.DeedsOf(read).Online;

    #endregion

}
