using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Building;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Storage;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// What is written down about a lambda as time goes on: its versions, the
/// stretches it was online, and what happened to it.
/// </summary>
/// <remarks>
/// Written into the context it is handed rather than one of its own, so an
/// entry lands in the same transaction as the change it records; whoever
/// hands it the context holds the lambda's turn where the change needs one.
///
/// Each kind is kept to a bound, oldest first, so a lambda an agent saves and
/// deploys all day long does not grow without end.
///
/// Takes no cancellation token: what it writes is part of a change already
/// decided on, and pruning deletes the manifests of versions before the rows
/// that name them are saved - stopped in between, rows would be left naming
/// nothing. What a pruned version alone held is collected from the store
/// later (see <see cref="IStorageService.CollectAsync"/>).
/// </remarks>
public sealed class LambdaHistory(IStorageService storage, ILimitsService limits, EditorAddresses editors)
{

    /// <summary>
    /// How many stretches of being online are remembered per lambda.
    /// </summary>
    /// <remarks>
    /// An agent iterating on something deploys a great deal, and the history
    /// is for reading back what happened lately, not for keeping every
    /// deployment a lambda ever had.
    /// </remarks>
    private const int MaxActivations = 200;

    #region Versions

    /// <summary>
    /// Adds the files as the next version of the lambda, and saves.
    /// </summary>
    /// <remarks>
    /// The files go to storage and the row to the database; the versions
    /// beyond what the lambda's tier keeps (<see cref="TierLimits.Versions"/>)
    /// go from both. Where their content was kept before the lambda's turn,
    /// which a save with much to keep does, writing them is writing the
    /// manifest of the version.
    /// </remarks>
    public async ValueTask<LambdaVersionInfo> AppendAsync(LambdaDbContext database, LambdaEntity lambda, IReadOnlyList<VersionFile> files, DateTime now, VersionNote note)
    {
        var version = await database.Deployments.Where(d => d.LambdaId == lambda.Id)
                                    .MaxAsync(d => (int?)d.Version) + 1 ?? 1;

        var specification = VersionInput.Tidy(note.Specification, VersionNote.MaxSpecification);

        var change = VersionInput.Tidy(note.Change, VersionNote.MaxChange);

        await storage.WriteAsync(lambda.Id, version, files);

        database.Deployments.Add(new DeploymentEntity
        {
            LambdaId = lambda.Id,
            Version = version,
            Created = now,
            Specification = specification,
            Change = change,
            Origin = note.Origin,
            Writer = note.Author?.Writer,
            AgentModel = note.Author?.Model,
            AgentEffort = note.Author?.Effort
        });

        lambda.Modified = now;

        await RecordAsync(database, lambda, LambdaEvent.Saved);

        await database.SaveChangesAsync();

        await PruneAsync(database, lambda);

        return new LambdaVersionInfo(version, now, specification, change, VersionOriginNames.Name(note.Origin), VersionWriterNames.Name(note.Author?.Writer),
                                     AgentCatalog.NameOf(note.Author?.Model), AgentCatalog.NameOf(note.Author?.Effort));
    }

    /// <summary>
    /// Keeps the version history bounded, never touching the version that is live.
    /// </summary>
    private async ValueTask PruneAsync(LambdaDbContext database, LambdaEntity lambda)
    {
        // the base of a feature is kept like the version online: it is what
        // the feature is compared with, and what it is shown to change
        var bases = await database.Features.Where(f => f.LambdaId == lambda.Id).Select(f => f.BaseVersion).ToListAsync();

        var obsolete = await database.Deployments.Where(d => d.LambdaId == lambda.Id && d.Version != lambda.ActiveVersion && !bases.Contains(d.Version))
                                     .OrderByDescending(d => d.Version)
                                     .Skip(limits.Get().Versions)
                                     .ToListAsync();

        if (obsolete.Count == 0)
        {
            return;
        }

        foreach (var deployment in obsolete)
        {
            storage.DeleteVersion(lambda.Id, deployment.Version);
        }

        database.Deployments.RemoveRange(obsolete);

        await database.SaveChangesAsync();
    }

    #endregion

    #region Activations

    /// <summary>
    /// Starts a stretch of being online with a version, ending the one before.
    /// </summary>
    /// <param name="origin">Who put it online</param>
    public async ValueTask ActivateAsync(LambdaDbContext database, long lambdaId, int version, DateTime now, VersionOrigin origin)
    {
        await CloseAsync(database, lambdaId, now, ActivationEnding.Replaced);

        database.Activations.Add(new ActivationEntity
        {
            LambdaId = lambdaId,
            Version = version,
            Started = now,
            Origin = origin
        });
    }

    /// <summary>
    /// Ends whatever stretch of being online is still open.
    /// </summary>
    /// <param name="endedBy">Why it ended, one of <see cref="ActivationEndings"/></param>
    public async ValueTask CloseAsync(LambdaDbContext database, long lambdaId, DateTime now, ActivationEnding endedBy)
    {
        var open = await database.Activations.Where(a => a.LambdaId == lambdaId && a.Ended == null)
                                 .ToListAsync();

        foreach (var activation in open)
        {
            activation.Ended = now;
            activation.EndedBy = endedBy;
        }
    }

    /// <summary>
    /// Forgets the oldest stretches that ended, beyond <see cref="MaxActivations"/>.
    /// </summary>
    /// <remarks>
    /// Reads what was saved, so it runs after the stretch it would count was.
    /// </remarks>
    public async ValueTask PruneActivationsAsync(LambdaDbContext database, long lambdaId)
    {
        var obsolete = await database.Activations.Where(a => a.LambdaId == lambdaId && a.Ended != null)
                                     .OrderByDescending(a => a.Started)
                                     .ThenByDescending(a => a.Id)
                                     .Skip(MaxActivations)
                                     .ToListAsync();

        if (obsolete.Count > 0)
        {
            database.Activations.RemoveRange(obsolete);

            await database.SaveChangesAsync();
        }
    }

    #endregion

    #region Events

    /// <summary>
    /// Notes that something happened, to be read long after it did.
    /// </summary>
    /// <remarks>
    /// Added to the same context as the change it describes, so it is written
    /// in the same transaction: an event recorded for a save that then failed
    /// would be a lie, and one written separately could be lost on its own.
    ///
    /// Demos are skipped. The installation seeds its own on every boot, and
    /// counting those would bury the activity the figures are meant to show.
    /// </remarks>
    /// <param name="kind">What happened, one of <see cref="LambdaEvents"/></param>
    public async ValueTask RecordAsync(LambdaDbContext database, LambdaEntity lambda, LambdaEvent kind)
    {
        if (limits.PolicyOf(lambda.Tier).ReadOnly)
        {
            return;
        }

        database.Events.Add(new EventEntity
        {
            Kind = kind,
            LambdaId = lambda.Id,
            PublicKey = lambda.PublicKey,
            Occurred = DateTime.UtcNow
        });

        // where it was changed from, for the operator who blocks it; a
        // lambda that is going has nothing left to block
        if (kind != LambdaEvent.Deleted)
        {
            await editors.NoteAsync(lambda.Id);
        }
    }

    #endregion

}
