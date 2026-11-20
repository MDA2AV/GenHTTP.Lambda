using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// Keeps what the operator decided about the build agent, in the settings
/// of the installation.
/// </summary>
/// <remarks>
/// Read by every page that shows a box for the agent and by every job asked
/// for, so it is read once as the platform starts (<see cref="LoadAsync"/>),
/// held in memory and only written through to the database.
///
/// A row each, in the table the panel's other switches and the limits are
/// kept in. The two switches keep the rows they had before they moved here
/// from the site's settings (<c>build-box</c>, <c>change-box</c>), so an
/// installation that switched one off keeps it off; a missing row is the
/// default (<see cref="AgentSettings.Default"/>).
/// </remarks>
public sealed class AgentSettingsStore(IDbContextFactory<LambdaDbContext> databases)
{
    private const string BuildsKey = "build-box";

    private const string ChangesKey = "change-box";

    private const string ModelPrefix = "agent.model.";

    private const string DefaultModelKey = "agent.default-model";

    private const string EffortsKey = "agent.efforts";

    private const string DefaultEffortKey = "agent.default-effort";

    /// <summary>
    /// Who is changing the settings: one at a time, across the write.
    /// </summary>
    private readonly SemaphoreSlim _turn = new(1, 1);

    private volatile AgentSettings? _current;

    #region Reading

    /// <summary>
    /// The settings as they stand.
    /// </summary>
    public AgentSettings Get()
        => _current ?? throw new InvalidOperationException("The settings of the build agent are read as the platform starts, which it has not.");

    /// <summary>
    /// Reads the settings into memory, before anything is served.
    /// </summary>
    public async ValueTask LoadAsync(CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var stored = await database.Settings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value, cancellation);

        _current = Read(stored);
    }

    private static AgentSettings Read(Dictionary<string, string> stored)
    {
        var fallback = AgentSettings.Default;

        var builds = Flag(stored, BuildsKey, fallback.Builds);
        var changes = Flag(stored, ChangesKey, fallback.Changes);

        var models = AgentCatalog.Models
                                 .Select(m => new ModelSetting(m.Model,
                                     stored.TryGetValue(ModelPrefix + m.Name, out var text) && AgentCatalog.TryAccess(text, out var access)
                                         ? access
                                         : fallback.AccessOf(m.Model)))
                                 .ToList();

        var defaultModel = stored.TryGetValue(DefaultModelKey, out var modelName) && AgentCatalog.TryModel(modelName, out var model)
            ? model
            : fallback.DefaultModel;

        var efforts = stored.TryGetValue(EffortsKey, out var effortNames)
            ? [.. effortNames.Split(',').Select(n => AgentCatalog.TryEffort(n, out var e) ? e : (AgentEffort?)null).OfType<AgentEffort>()]
            : fallback.Efforts;

        var defaultEffort = stored.TryGetValue(DefaultEffortKey, out var effortName) && AgentCatalog.TryEffort(effortName, out var effort)
            ? effort
            : fallback.DefaultEffort;

        var read = Tidy(new AgentSettings(builds, changes, models, defaultModel, efforts, defaultEffort));

        // rows nobody saved through the panel - edited by hand, or from a
        // catalog that changed - are not left to offer nothing to everybody:
        // what the platform offers by default takes their place, the switches
        // stay as they were
        return Problem(read) == null ? read : fallback with { Builds = builds, Changes = changes };
    }

    private static bool Flag(Dictionary<string, string> stored, string key, bool fallback)
        => stored.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : fallback;

    #endregion

    #region Writing

    /// <summary>
    /// Replaces the settings.
    /// </summary>
    /// <returns>The settings as they were saved</returns>
    /// <exception cref="LambdaException">The settings would offer nobody a model or an effort to start with</exception>
    public async ValueTask<AgentSettings> SaveAsync(AgentSettings settings, CancellationToken cancellation = default)
    {
        settings = Tidy(settings);

        if (Problem(settings) is { } problem)
        {
            throw LambdaException.Invalid(problem);
        }

        await _turn.WaitAsync(cancellation);

        try
        {
            await using var database = await databases.CreateDbContextAsync(cancellation);

            foreach (var (key, value) in Flatten(settings))
            {
                var existing = await database.Settings.FindAsync([key], cancellation);

                if (existing == null)
                {
                    database.Settings.Add(new SettingEntity { Key = key, Value = value });
                }
                else
                {
                    existing.Value = value;
                }
            }

            await database.SaveChangesAsync(cancellation);

            return _current = settings;
        }
        finally
        {
            _turn.Release();
        }
    }

    /// <summary>
    /// Every setting by the key it is stored under, as it is stored.
    /// </summary>
    private static IEnumerable<(string Key, string Value)> Flatten(AgentSettings settings)
    {
        yield return (BuildsKey, settings.Builds ? "true" : "false");
        yield return (ChangesKey, settings.Changes ? "true" : "false");

        foreach (var model in settings.Models)
        {
            yield return (ModelPrefix + AgentCatalog.NameOf(model.Model), AgentCatalog.NameOf(model.Access));
        }

        yield return (DefaultModelKey, AgentCatalog.NameOf(settings.DefaultModel));
        yield return (EffortsKey, string.Join(',', settings.Efforts.Select(AgentCatalog.NameOf)));
        yield return (DefaultEffortKey, AgentCatalog.NameOf(settings.DefaultEffort));
    }

    #endregion

    #region Rules

    /// <summary>
    /// The settings as they are kept: every model of the catalog once, in its
    /// order - one left out is offered to nobody - and the efforts once each,
    /// the least first.
    /// </summary>
    private static AgentSettings Tidy(AgentSettings settings)
    {
        var models = AgentCatalog.Models
                                 .Select(m => new ModelSetting(m.Model, settings.Models.FirstOrDefault(s => s.Model == m.Model)?.Access ?? ModelAccess.Off))
                                 .ToList();

        var efforts = AgentCatalog.Efforts.Select(e => e.Effort).Where(settings.Efforts.Contains).ToList();

        return settings with { Models = models, Efforts = efforts };
    }

    /// <summary>
    /// Why the settings cannot be kept, or nothing where they can.
    /// </summary>
    /// <remarks>
    /// The default model is what a job runs on where nobody chose, so it is
    /// the one everybody gets: offered, and not behind the password. Whoever
    /// means to offer the agent to nobody switches building and changing off.
    /// </remarks>
    private static string? Problem(AgentSettings settings)
    {
        if (!Enum.IsDefined(settings.DefaultModel) || settings.Models.Any(m => !Enum.IsDefined(m.Access)))
        {
            return "There is no such model or access here.";
        }

        if (settings.AccessOf(settings.DefaultModel) != ModelAccess.Open)
        {
            return $"The default model is what everybody gets, so {AgentCatalog.LabelOf(settings.DefaultModel)} has to be offered to everybody, without the password.";
        }

        if (settings.Efforts.Count == 0)
        {
            return "At least one effort has to be offered.";
        }

        if (!settings.Efforts.Contains(settings.DefaultEffort))
        {
            return "The default effort has to be one of those offered.";
        }

        return null;
    }

    #endregion

}
