using System.Text.Json;
using System.Text.Json.Serialization;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Infrastructure;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// The runs of the agent as the platform keeps them: the run, its events in
/// the order they happened, and the files that travelled with it.
/// </summary>
/// <remarks>
/// <para>
/// Every write wakes whatever listens for the run (<see cref="AgentSignals"/>),
/// so a page is told as soon as something is recorded rather than asking on
/// a timer.
/// </para>
/// <para>
/// An event takes the next number of its run. Several calls of one run may
/// record at once, and so may the follower, so the number is taken under
/// the run's turn on this server - and taken again where another server of
/// the installation took it first.
/// </para>
/// </remarks>
public sealed class AgentRunRecords(IDbContextFactory<LambdaDbContext> databases, AgentSignals signals)
{

    /// <summary>How events are written: camel case, without what is null.</summary>
    public static readonly JsonSerializerOptions Format = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly Turns _turns = new();

    #region Runs

    public async ValueTask CreateAsync(AgentRunEntity run, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        database.AgentRuns.Add(run);

        await database.SaveChangesAsync(cancellation);

        signals.Touch(run.Id, run.LambdaId);
    }

    public async ValueTask<AgentRunEntity?> GetAsync(string id, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.AgentRuns.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellation);
    }

    /// <summary>
    /// Changes a run, and says what it is afterwards - or nothing where there
    /// is no such run (any more).
    /// </summary>
    public async ValueTask<AgentRunEntity?> UpdateAsync(string id, Action<AgentRunEntity> change, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var run = await database.AgentRuns.FirstOrDefaultAsync(r => r.Id == id, cancellation);

        if (run == null)
        {
            return null;
        }

        change(run);

        await database.SaveChangesAsync(cancellation);

        signals.Touch(run.Id, run.LambdaId);

        return run;
    }

    public async ValueTask DeleteAsync(string id, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        await database.AgentRuns.Where(r => r.Id == id).ExecuteDeleteAsync(cancellation);
    }

    /// <summary>
    /// The newest run of a lambda, or nothing.
    /// </summary>
    public async ValueTask<AgentRunEntity?> NewestOfAsync(long lambda, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.AgentRuns.AsNoTracking()
                             .Where(r => r.LambdaId == lambda)
                             .OrderByDescending(r => r.Created)
                             .FirstOrDefaultAsync(cancellation);
    }

    /// <summary>
    /// The newest runs of a lambda, the newest first.
    /// </summary>
    public async ValueTask<IReadOnlyList<AgentRunEntity>> OfLambdaAsync(long lambda, int take, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.AgentRuns.AsNoTracking()
                             .Where(r => r.LambdaId == lambda)
                             .OrderByDescending(r => r.Created)
                             .Take(take)
                             .ToListAsync(cancellation);
    }

    /// <summary>
    /// Every run that has not ended, the oldest first: what the follower takes
    /// up again as the platform starts.
    /// </summary>
    public async ValueTask<IReadOnlyList<AgentRunEntity>> ActiveAsync(CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.AgentRuns.AsNoTracking()
                             .Where(r => r.State != AgentRunState.Ended)
                             .OrderBy(r => r.Created)
                             .ToListAsync(cancellation);
    }

    /// <summary>
    /// The newest runs of the installation, the newest first, for the operator.
    /// </summary>
    public async ValueTask<IReadOnlyList<AgentRunEntity>> NewestAsync(int take, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.AgentRuns.AsNoTracking()
                             .OrderByDescending(r => r.Created)
                             .Take(take)
                             .ToListAsync(cancellation);
    }

    /// <summary>
    /// The options, the lengths and the endings of the runs since a moment,
    /// to count them by - no prompt, nothing anybody wrote.
    /// </summary>
    public async ValueTask<IReadOnlyList<(string Options, AgentRunKind Kind, AgentRunState State, DateTime? Started, DateTime? Ended)>> SinceAsync(DateTime since,
        CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var rows = await database.AgentRuns.AsNoTracking()
                                 .Where(r => r.Created >= since)
                                 .Select(r => new { r.Options, r.Kind, r.State, r.Started, r.Ended })
                                 .ToListAsync(cancellation);

        return [.. rows.Select(r => (r.Options, r.Kind, r.State, r.Started, r.Ended))];
    }

    /// <summary>
    /// Removes the runs that ended before a moment, with their events, and
    /// says which files went with them.
    /// </summary>
    public async ValueTask<IReadOnlyList<string>> ExpireAsync(DateTime before, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var expired = database.AgentRuns.Where(r => r.State == AgentRunState.Ended && r.Ended < before);

        var files = await database.AgentFiles.Where(f => f.RunId != null && expired.Select(r => r.Id).Contains(f.RunId))
                                  .Select(f => f.Id)
                                  .ToListAsync(cancellation);

        await database.AgentRunEvents.Where(e => expired.Select(r => r.Id).Contains(e.RunId)).ExecuteDeleteAsync(cancellation);
        await database.AgentFiles.Where(f => files.Contains(f.Id)).ExecuteDeleteAsync(cancellation);
        await expired.ExecuteDeleteAsync(cancellation);

        return files;
    }

    #endregion

    #region Events

    /// <summary>
    /// Records an event of a run, and says the number it took.
    /// </summary>
    public async ValueTask<int> AppendAsync(string run, long? lambda, AgentRunEventKind kind, object data, string? key = null, DateTime? at = null,
                                            CancellationToken cancellation = default)
    {
        var text = JsonSerializer.Serialize(data, Format);

        using (await _turns.TakeAsync(StripeOf(run)))
        {
            for (var attempt = 0; ; attempt++)
            {
                await using var database = await databases.CreateDbContextAsync(cancellation);

                var last = await database.AgentRunEvents.Where(e => e.RunId == run).MaxAsync(e => (int?)e.Number, cancellation) ?? 0;

                database.AgentRunEvents.Add(new AgentRunEventEntity
                {
                    RunId = run,
                    Number = last + 1,
                    At = at ?? DateTime.UtcNow,
                    Kind = kind,
                    Key = key,
                    Data = text
                });

                try
                {
                    await database.SaveChangesAsync(cancellation);

                    signals.Touch(run, lambda);

                    return last + 1;
                }
                catch (DbUpdateException) when (attempt < 3)
                {
                    // another server of the installation took the number
                }
            }
        }
    }

    /// <summary>
    /// Replaces what an event of a run holds - a step once it is done - by its
    /// number, or by its key where it has one.
    /// </summary>
    public async ValueTask CompleteAsync(string run, long? lambda, int? number, string? key, object data, CancellationToken cancellation = default)
    {
        var text = JsonSerializer.Serialize(data, Format);

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var events = database.AgentRunEvents.Where(e => e.RunId == run);

        events = number is { } at ? events.Where(e => e.Number == at) : events.Where(e => e.Key == key);

        var changed = await events.ExecuteUpdateAsync(e => e.SetProperty(x => x.Data, text), cancellation);

        if (changed > 0)
        {
            signals.Touch(run, lambda);
        }
    }

    /// <summary>
    /// The events of a run, in the order they happened.
    /// </summary>
    public async ValueTask<IReadOnlyList<AgentRunEventEntity>> EventsAsync(string run, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.AgentRunEvents.AsNoTracking()
                             .Where(e => e.RunId == run)
                             .OrderBy(e => e.Number)
                             .ToListAsync(cancellation);
    }

    /// <summary>
    /// The events of one kind of several runs, in the order they happened.
    /// </summary>
    public async ValueTask<IReadOnlyList<AgentRunEventEntity>> EventsAsync(IReadOnlyList<string> runs, AgentRunEventKind kind, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.AgentRunEvents.AsNoTracking()
                             .Where(e => runs.Contains(e.RunId) && e.Kind == kind)
                             .OrderBy(e => e.Number)
                             .ToListAsync(cancellation);
    }

    public static T? Read<T>(AgentRunEventEntity happened) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(happened.Data, Format);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    #endregion

    #region Files

    public async ValueTask AddFileAsync(AgentFileEntity file, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        database.AgentFiles.Add(file);

        await database.SaveChangesAsync(cancellation);
    }

    public async ValueTask<AgentFileEntity?> FileAsync(string id, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.AgentFiles.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, cancellation);
    }

    /// <summary>
    /// The files of the given runs, in the order they came.
    /// </summary>
    public async ValueTask<IReadOnlyList<AgentFileEntity>> FilesOfAsync(IReadOnlyList<string> runs, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.AgentFiles.AsNoTracking()
                             .Where(f => f.RunId != null && runs.Contains(f.RunId))
                             .OrderBy(f => f.Created)
                             .ToListAsync(cancellation);
    }

    /// <summary>
    /// The files attached to messages about a lambda, the newest first.
    /// </summary>
    public async ValueTask<IReadOnlyList<AgentFileEntity>> AttachedToAsync(long lambda, int take, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.AgentFiles.AsNoTracking()
                             .Where(f => f.LambdaId == lambda && f.Direction == AgentFileDirection.Attached)
                             .OrderByDescending(f => f.Created)
                             .Take(take)
                             .ToListAsync(cancellation);
    }

    /// <summary>
    /// Gives attachments that belong to nothing yet to a run - those of the
    /// given ids that are still free - and says which it took.
    /// </summary>
    public async ValueTask<IReadOnlyList<AgentFileEntity>> ClaimAsync(IReadOnlyList<string> ids, string run, long? lambda, CancellationToken cancellation = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var free = await database.AgentFiles.Where(f => ids.Contains(f.Id) && f.RunId == null && f.Direction == AgentFileDirection.Attached)
                                 .ToListAsync(cancellation);

        foreach (var file in free)
        {
            file.RunId = run;
            file.LambdaId = lambda;
        }

        await database.SaveChangesAsync(cancellation);

        return [.. ids.Select(id => free.FirstOrDefault(f => f.Id == id)).OfType<AgentFileEntity>()];
    }

    /// <summary>
    /// Hands the files of a run to its lambda, once a build created one.
    /// </summary>
    public async ValueTask SettleFilesAsync(string run, long lambda, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        await database.AgentFiles.Where(f => f.RunId == run && f.LambdaId == null)
                      .ExecuteUpdateAsync(f => f.SetProperty(x => x.LambdaId, lambda), cancellation);
    }

    /// <summary>
    /// Gives the attachments of a run back, as belonging to nothing: the run
    /// was never taken on.
    /// </summary>
    public async ValueTask ReleaseFilesAsync(string run, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        await database.AgentFiles.Where(f => f.RunId == run && f.Direction == AgentFileDirection.Attached)
                      .ExecuteUpdateAsync(f => f.SetProperty(x => x.RunId, (string?)null).SetProperty(x => x.LambdaId, (long?)null), cancellation);
    }

    /// <summary>
    /// Removes the attachments nobody sent within a while of uploading them,
    /// and says which they were.
    /// </summary>
    public async ValueTask<IReadOnlyList<string>> ExpireFreeAsync(DateTime before, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var free = database.AgentFiles.Where(f => f.RunId == null && f.Created < before);

        var ids = await free.Select(f => f.Id).ToListAsync(cancellation);

        await free.ExecuteDeleteAsync(cancellation);

        return ids;
    }

    /// <summary>
    /// The files of a lambda's runs, before the lambda is removed.
    /// </summary>
    public async ValueTask<IReadOnlyList<string>> FilesOfLambdaAsync(long lambda, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var runs = database.AgentRuns.Where(r => r.LambdaId == lambda).Select(r => r.Id);

        return await database.AgentFiles.Where(f => f.LambdaId == lambda || (f.RunId != null && runs.Contains(f.RunId)))
                             .Select(f => f.Id)
                             .ToListAsync(cancellation);
    }

    /// <summary>
    /// How many bytes the attachments hold that belong to no message yet,
    /// which bounds what may be uploaded before anything is sent.
    /// </summary>
    public async ValueTask<long> FreeBytesAsync(CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return await database.AgentFiles.Where(f => f.RunId == null).SumAsync(f => (long?)f.Size, cancellation) ?? 0;
    }

    #endregion

    #region Lambdas

    /// <summary>
    /// The keys of a lambda by its id, whether it is online or not - or
    /// nothing where there is no such lambda (any more).
    /// </summary>
    public async ValueTask<(string PublicKey, string PrivateKey)?> KeysOfAsync(long lambda, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var keys = await database.Lambdas.AsNoTracking()
                                 .Where(l => l.Id == lambda)
                                 .Select(l => new { l.PublicKey, l.PrivateKey })
                                 .FirstOrDefaultAsync(cancellation);

        return keys == null ? null : (keys.PublicKey, keys.PrivateKey);
    }

    #endregion

    #region Helpers

    private static long StripeOf(string run) => (long)(uint)StringComparer.Ordinal.GetHashCode(run);

    #endregion

}
