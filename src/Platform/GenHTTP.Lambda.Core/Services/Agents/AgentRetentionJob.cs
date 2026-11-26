using GenHTTP.Lambda.Integrations.Files;
using GenHTTP.Lambda.Services.Background;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// Lets go of the runs of the agent once they are old, with their events and
/// files - and of what was only ever begun.
/// </summary>
/// <remarks>
/// <para>
/// A run is kept for <see cref="Kept"/> after it ended: long enough to come
/// back to a conversation and refer to what was sent in it, and for the
/// operator to look into one that went wrong, not longer - what people ask
/// and attach is theirs. The privacy pages say so. The prompts are in the
/// log as well, which keeps what it keeps.
/// </para>
/// <para>
/// A file nobody sent with a message within an hour of uploading it is
/// removed, and so are the tickets of the build agent's jobs from before
/// runs were kept, once their day is over.
/// </para>
/// </remarks>
public sealed class AgentRetentionJob(AgentRunRecords records, AgentCredentials credentials, IFileProvider files, ILogger<AgentRetentionJob> logger)
    : IBackgroundJob
{

    /// <summary>How long a run is kept after it ended.</summary>
    public static readonly TimeSpan Kept = TimeSpan.FromDays(30);

    public string Name => "agent runs";

    public TimeSpan Interval => TimeSpan.FromHours(1);

    public async ValueTask ExecuteAsync(CancellationToken cancellation)
    {
        var now = DateTime.UtcNow;

        var expired = await records.ExpireAsync(now - Kept, cancellation);

        var unsent = await records.ExpireFreeAsync(now - AgentAttachments.Unclaimed, cancellation);

        foreach (var id in expired.Concat(unsent))
        {
            await files.DeleteAsync(FileStore.AgentFiles, id, cancellation);
        }

        await credentials.ExpireAsync(now, cancellation);

        if (expired.Count + unsent.Count > 0)
        {
            logger.LogInformation("Removed {Expired} files of expired runs and {Unsent} unsent attachments", expired.Count, unsent.Count);
        }
    }

}
