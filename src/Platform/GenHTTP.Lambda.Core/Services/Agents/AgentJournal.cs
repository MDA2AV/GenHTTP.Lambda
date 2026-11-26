using GenHTTP.Lambda.Data.Entities;
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
public sealed class AgentJournal(AgentRunRecords records, IFileProvider files, ILogger<AgentJournal> logger) : IAgentJournal
{

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

    public async ValueTask ForgetAsync(long lambdaId, CancellationToken cancellation = default)
    {
        foreach (var id in await records.FilesOfLambdaAsync(lambdaId, cancellation))
        {
            await files.DeleteAsync(FileStore.AgentFiles, id, cancellation);
        }
    }

}
