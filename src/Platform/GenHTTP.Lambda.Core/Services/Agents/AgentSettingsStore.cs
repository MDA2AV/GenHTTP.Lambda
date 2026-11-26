using System.Globalization;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// Keeps what the operator decided about the agent, in the settings of the
/// installation.
/// </summary>
/// <remarks>
/// <para>
/// Read by every page that shows a box for the agent and by every run asked
/// for, so it is read once as the platform starts (<see cref="LoadAsync"/>),
/// held in memory and only written through to the database.
/// </para>
/// <para>
/// A row each, in the table the panel's other switches and the limits are
/// kept in: <c>agent.option.{option}.{value}</c> says who may choose a value,
/// <c>agent.default.{option}</c> what a run takes where nobody chose, and
/// <c>agent.limit.*</c> what a run may take. The two switches keep the rows
/// they had since they moved from the site's settings (<c>build-box</c>,
/// <c>change-box</c>).
/// </para>
/// <para>
/// An installation from before the options were the integration's chose its
/// models and efforts in rows of their own (<c>agent.model.{name}</c>,
/// <c>agent.default-model</c>, <c>agent.efforts</c>, <c>agent.default-effort</c>).
/// They are read as the options <c>model</c> and <c>effort</c> they became,
/// where no row of the new kind says otherwise, and left where they are: what
/// the operator decided then holds after the upgrade until they decide again.
/// </para>
/// </remarks>
public sealed class AgentSettingsStore(IDbContextFactory<LambdaDbContext> databases)
{
    private const string BuildsKey = "build-box";

    private const string ChangesKey = "change-box";

    private const string OptionPrefix = "agent.option.";

    private const string DefaultPrefix = "agent.default.";

    private const string TimeKey = "agent.limit.time";

    private const string StepsKey = "agent.limit.steps";

    private const string GuardedTimeKey = "agent.limit.guarded-time";

    private const string GuardedStepsKey = "agent.limit.guarded-steps";

    private const string LegacyModelPrefix = "agent.model.";

    private const string LegacyDefaultModelKey = "agent.default-model";

    private const string LegacyEffortsKey = "agent.efforts";

    private const string LegacyDefaultEffortKey = "agent.default-effort";

    /// <summary>The efforts the rows from before named, so a list of them is read as which were offered and which not.</summary>
    private static readonly string[] LegacyEfforts = ["low", "medium", "high", "xhigh", "max"];

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
        => _current ?? throw new InvalidOperationException("The settings of the agent are read as the platform starts, which it has not.");

    /// <summary>
    /// Reads the settings into memory, before anything is served.
    /// </summary>
    public async ValueTask LoadAsync(CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var stored = await database.Settings.AsNoTracking()
                                   .Where(s => s.Key.StartsWith("agent.") || s.Key == BuildsKey || s.Key == ChangesKey)
                                   .ToDictionaryAsync(s => s.Key, s => s.Value, cancellation);

        _current = Read(stored);
    }

    private static AgentSettings Read(Dictionary<string, string> stored)
    {
        var fallback = AgentSettings.Default;

        var access = new Dictionary<string, Dictionary<string, OptionAccess>>(StringComparer.Ordinal);
        var defaults = new Dictionary<string, string>(StringComparer.Ordinal);

        void Decide(string option, string value, OptionAccess decided, bool overwrite)
        {
            if (!access.TryGetValue(option, out var values))
            {
                access[option] = values = new Dictionary<string, OptionAccess>(StringComparer.Ordinal);
            }

            if (overwrite || !values.ContainsKey(value))
            {
                values[value] = decided;
            }
        }

        // what was decided before the options were the integration's, first,
        // so a row of the new kind takes its place
        foreach (var (key, value) in stored)
        {
            if (key.StartsWith(LegacyModelPrefix, StringComparison.Ordinal) && OptionAccessNames.TryParse(value, out var legacy))
            {
                Decide(AgentModelNames.ModelOption, key[LegacyModelPrefix.Length..], legacy, overwrite: false);
            }
        }

        if (stored.TryGetValue(LegacyEffortsKey, out var efforts))
        {
            var offered = efforts.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            foreach (var effort in LegacyEfforts)
            {
                Decide(AgentModelNames.EffortOption, effort, offered.Contains(effort) ? OptionAccess.Open : OptionAccess.Off, overwrite: false);
            }
        }

        if (stored.TryGetValue(LegacyDefaultModelKey, out var model) && Named(model))
        {
            defaults[AgentModelNames.ModelOption] = model;
        }

        if (stored.TryGetValue(LegacyDefaultEffortKey, out var level) && Named(level))
        {
            defaults[AgentModelNames.EffortOption] = level;
        }

        foreach (var (key, value) in stored)
        {
            if (key.StartsWith(OptionPrefix, StringComparison.Ordinal) && Split(key[OptionPrefix.Length..]) is { } named && OptionAccessNames.TryParse(value, out var decided))
            {
                Decide(named.Option, named.Value, decided, overwrite: true);
            }

            if (key.StartsWith(DefaultPrefix, StringComparison.Ordinal) && Named(key[DefaultPrefix.Length..]) && Named(value))
            {
                defaults[key[DefaultPrefix.Length..]] = value;
            }
        }

        return new AgentSettings(Flag(stored, BuildsKey, fallback.Builds), Flag(stored, ChangesKey, fallback.Changes),
                                 access.ToDictionary(a => a.Key, a => (IReadOnlyDictionary<string, OptionAccess>)a.Value, StringComparer.Ordinal),
                                 defaults,
                                 new LimitSetting(Number(stored, TimeKey), Number(stored, StepsKey)),
                                 new LimitSetting(Number(stored, GuardedTimeKey), Number(stored, GuardedStepsKey)));
    }

    private static bool Flag(Dictionary<string, string> stored, string key, bool fallback)
        => stored.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : fallback;

    private static int? Number(Dictionary<string, string> stored, string key)
        => stored.TryGetValue(key, out var value) && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    /// <summary>The option and the value of a key below the option prefix: an option's name has no dot, a value's may.</summary>
    private static (string Option, string Value)? Split(string rest)
    {
        var dot = rest.IndexOf('.', StringComparison.Ordinal);

        if (dot <= 0 || dot == rest.Length - 1)
        {
            return null;
        }

        var option = rest[..dot];
        var value = rest[(dot + 1)..];

        return Named(option) && Named(value) ? (option, value) : null;
    }

    #endregion

    #region Writing

    /// <summary>
    /// Replaces the settings, which the gate has checked against what the
    /// integration offers.
    /// </summary>
    /// <returns>The settings as they were saved</returns>
    public async ValueTask<AgentSettings> SaveAsync(AgentSettings settings, CancellationToken cancellation = default)
    {
        await _turn.WaitAsync(cancellation);

        try
        {
            await using var database = await databases.CreateDbContextAsync(cancellation);

            var rows = Flatten(settings).ToList();

            foreach (var (key, value) in rows)
            {
                var existing = await database.Settings.FindAsync([key], cancellation);

                if (value == null)
                {
                    if (existing != null)
                    {
                        database.Settings.Remove(existing);
                    }
                }
                else if (existing == null)
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
    /// Every setting by the key it is stored under, as it is stored - nothing
    /// for a limit left to the agent, whose row is removed.
    /// </summary>
    private static IEnumerable<(string Key, string? Value)> Flatten(AgentSettings settings)
    {
        yield return (BuildsKey, settings.Builds ? "true" : "false");
        yield return (ChangesKey, settings.Changes ? "true" : "false");

        foreach (var (option, values) in settings.Access)
        {
            foreach (var (value, access) in values)
            {
                yield return ($"{OptionPrefix}{option}.{value}", OptionAccessNames.Name(access));
            }
        }

        foreach (var (option, value) in settings.Defaults)
        {
            yield return (DefaultPrefix + option, value);
        }

        yield return (TimeKey, Text(settings.Limits.Seconds));
        yield return (StepsKey, Text(settings.Limits.Steps));
        yield return (GuardedTimeKey, Text(settings.GuardedLimits.Seconds));
        yield return (GuardedStepsKey, Text(settings.GuardedLimits.Steps));
    }

    private static string? Text(int? number) => number?.ToString(CultureInfo.InvariantCulture);

    #endregion

    #region Names

    /// <summary>
    /// Whether a name may be an option's or a value's: lower case letters,
    /// digits and dashes, as the integration contract names them.
    /// </summary>
    public static bool Named(string? name)
        => name is { Length: > 0 and <= 64 } && name.All(c => c is >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_');

    #endregion

}
