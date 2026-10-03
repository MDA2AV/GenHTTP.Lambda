using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Storage;

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
/// </remarks>
public sealed class LambdaHistory(IStorageService storage, ILimitsService limits)
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
    /// Adds the code as the next version of the lambda, and saves.
    /// </summary>
    /// <remarks>
    /// The code goes to storage and its row to the database; the versions
    /// beyond what the lambda's tier keeps (<see cref="TierLimits.Versions"/>)
    /// go from both.
    /// </remarks>
    public LambdaVersionInfo Append(LambdaDbContext database, LambdaEntity lambda, string code, DateTime now, VersionNote note)
    {
        var version = database.Deployments.Where(d => d.LambdaId == lambda.Id)
                              .Max(d => (int?)d.Version) + 1 ?? 1;

        var specification = VersionInput.Tidy(note.Specification, VersionNote.MaxSpecification);

        var change = VersionInput.Tidy(note.Change, VersionNote.MaxChange);

        storage.Write(lambda.Id, version, code);

        database.Deployments.Add(new DeploymentEntity
        {
            LambdaId = lambda.Id,
            Version = version,
            Created = now,
            Specification = specification,
            Change = change,
            Origin = note.Origin
        });

        lambda.Modified = now;

        Record(database, lambda, LambdaEvents.Saved);

        database.SaveChanges();

        Prune(database, lambda);

        return new LambdaVersionInfo(version, now, specification, change, note.Origin);
    }

    /// <summary>
    /// Keeps the version history bounded, never touching the version that is live.
    /// </summary>
    private void Prune(LambdaDbContext database, LambdaEntity lambda)
    {
        // the base of a feature is kept like the version online: it is what
        // the feature is compared with, and what it is shown to change
        var bases = database.Features.Where(f => f.LambdaId == lambda.Id).Select(f => f.BaseVersion).ToList();

        var obsolete = database.Deployments.Where(d => d.LambdaId == lambda.Id && d.Version != lambda.ActiveVersion && !bases.Contains(d.Version))
                               .OrderByDescending(d => d.Version)
                               .Skip(limits.Of(lambda.Tier).Versions)
                               .ToList();

        if (obsolete.Count == 0)
        {
            return;
        }

        foreach (var deployment in obsolete)
        {
            storage.DeleteVersion(lambda.Id, deployment.Version);
        }

        database.Deployments.RemoveRange(obsolete);

        database.SaveChanges();
    }

    #endregion

    #region Activations

    /// <summary>
    /// Starts a stretch of being online with a version, ending the one before.
    /// </summary>
    /// <param name="origin">Who put it online</param>
    public void Activate(LambdaDbContext database, long lambdaId, int version, DateTime now, string origin)
    {
        Close(database, lambdaId, now, ActivationEndings.Replaced);

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
    public void Close(LambdaDbContext database, long lambdaId, DateTime now, string endedBy)
    {
        var open = database.Activations.Where(a => a.LambdaId == lambdaId && a.Ended == null)
                           .ToList();

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
    public void PruneActivations(LambdaDbContext database, long lambdaId)
    {
        var obsolete = database.Activations.Where(a => a.LambdaId == lambdaId && a.Ended != null)
                               .OrderByDescending(a => a.Started)
                               .ThenByDescending(a => a.Id)
                               .Skip(MaxActivations)
                               .ToList();

        if (obsolete.Count > 0)
        {
            database.Activations.RemoveRange(obsolete);

            database.SaveChanges();
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
    public void Record(LambdaDbContext database, LambdaEntity lambda, string kind)
    {
        if (lambda.Tier == LambdaTier.Demo)
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
    }

    #endregion

}
