using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Integrations.Agents;
using GenHTTP.Lambda.Integrations.Files;
using GenHTTP.Lambda.Services.Meta.Model;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// Writes down what a run did, into its events (see <see cref="IAgentJournal"/>).
/// </summary>
/// <remarks>
/// What it records was done already: a write that fails here is logged and
/// let go of rather than failing the call that did it, which would tell the
/// agent that what happened did not.
/// </remarks>
public sealed class AgentJournal(AgentRunRecords records, IFileProvider files, ILogger<AgentJournal> logger, IAgentProvider? provider = null) : IAgentJournal
{

    /// <summary>Whether the call at hand is one the MCP door records as a step of its own.</summary>
    private static readonly AsyncLocal<bool> Stepping = new();

    public IDisposable Step()
    {
        // set where the door calls it, not in an async method, so it holds for
        // what the door then awaits
        Stepping.Value = true;

        return new Unstep();
    }

    private sealed class Unstep : IDisposable
    {
        public void Dispose() => Stepping.Value = false;
    }

    public async ValueTask RecordAsync(VersionAuthor? author, AgentEffect effect, CancellationToken cancellation = default)
    {
        if (author?.Run is not { } run)
        {
            return;
        }

        try
        {
            if (effect.Kind == AgentEffectKind.Created)
            {
                // a build learns its lambda from what it created, and the files
                // it was asked with belong to that lambda from now on
                await records.UpdateAsync(run, r => r.LambdaId ??= effect.LambdaId, cancellation);
                await records.SettleFilesAsync(run, effect.LambdaId, cancellation);
            }

            await records.AppendAsync(run, effect.LambdaId, AgentRunEventKind.Effect, effect, cancellation: cancellation);

            // a call through another door - the REST API, git - is a step too,
            // told by what it did: whatever door an agent takes, the pages show it
            if (!Stepping.Value && StepOf(effect) is { } step)
            {
                await records.AppendAsync(run, effect.LambdaId, AgentRunEventKind.Step, step, cancellation: cancellation);
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogWarning(e, "Failed to record what run {Run} changed of lambda #{LambdaId}", run, effect.LambdaId);
        }
    }

    public async ValueTask<int?> BeginAsync(VersionAuthor? author, AgentStep step, CancellationToken cancellation = default)
    {
        if (author?.Run is not { } run)
        {
            return null;
        }

        try
        {
            var current = await records.GetAsync(run, cancellation);

            // a run whose calls reach the platform is at work, whatever the
            // agent system said of it last
            if (current is { State: AgentRunState.Starting or AgentRunState.Queued })
            {
                await records.UpdateAsync(run, r =>
                {
                    if (r.State is AgentRunState.Starting or AgentRunState.Queued)
                    {
                        r.State = AgentRunState.Working;
                        r.Started ??= DateTime.UtcNow;
                        r.Position = 0;
                    }
                }, cancellation);
            }

            return await records.AppendAsync(run, current?.LambdaId, AgentRunEventKind.Step, step, cancellation: cancellation);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogWarning(e, "Failed to record a step of run {Run}", run);
            return null;
        }
    }

    public async ValueTask CompleteAsync(VersionAuthor? author, int? number, AgentStep step, CancellationToken cancellation = default)
    {
        if (author?.Run is not { } run || number == null)
        {
            return;
        }

        try
        {
            var lambda = (await records.GetAsync(run, cancellation))?.LambdaId;

            await records.CompleteAsync(run, lambda, number, null, step, cancellation);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogWarning(e, "Failed to complete a step of run {Run}", run);
        }
    }

    /// <summary>
    /// What an effect of a call through another door is as a step - the same
    /// kinds the MCP door records, done.
    /// </summary>
    private static AgentStep? StepOf(AgentEffect effect)
    {
        var preview = effect.Feature != null ? true : (bool?)null;

        return effect.Kind switch
        {
            AgentEffectKind.Created => new AgentStep(0, "create", Done: true),
            AgentEffectKind.Saved => new AgentStep(0, "write", Version: effect.Version, Online: effect.Online, Done: true),
            AgentEffectKind.Drafted => new AgentStep(0, "write", Preview: true, Online: effect.Online, Done: true),
            AgentEffectKind.Deployed => new AgentStep(0, "deploy", Version: effect.Version, Online: true, Preview: preview, Done: true),
            AgentEffectKind.Started => new AgentStep(0, "feature", Feature: effect.Name, Preview: true, Done: true),
            AgentEffectKind.Merged => new AgentStep(0, "merge", Version: effect.Version, Online: effect.Online, Done: true),
            AgentEffectKind.Discarded => new AgentStep(0, "discard", Done: true),
            AgentEffectKind.Enabled => new AgentStep(0, "data", Data: effect.Name, Done: true),
            AgentEffectKind.Counted => new AgentStep(0, "analytics", Done: true),
            AgentEffectKind.Published => new AgentStep(0, "source", Done: true),
            AgentEffectKind.Requested => new AgentStep(0, "connectivity", Done: true),
            AgentEffectKind.Showcased => new AgentStep(0, "showcase", Done: true),
            AgentEffectKind.Viewed => new AgentStep(0, "other", Tool: "update_lambda", Done: true),
            AgentEffectKind.Wrote => new AgentStep(0, "upload", Path: effect.Name, Preview: preview, Done: true),
            AgentEffectKind.Removed => new AgentStep(0, "delete", Path: effect.Name, Preview: preview, Done: true),
            _ => null
        };
    }

    public async ValueTask ForgetAsync(long lambdaId, CancellationToken cancellation = default)
    {
        // a run still at work on it is stopped: there is nothing left to work on
        if (provider != null && await records.NewestOfAsync(lambdaId, cancellation) is { State: not AgentRunState.Ended } current)
        {
            try
            {
                await provider.CancelAsync(new AgentRunHandle(current.Id, current.Reference), cancellation);
            }
            catch (Exception)
            {
                // the agent system that does not answer finds the run gone when it does
            }
        }

        foreach (var id in await records.FilesOfLambdaAsync(lambdaId, cancellation))
        {
            await files.DeleteAsync(FileStore.AgentFiles, id, cancellation);
        }
    }

}
