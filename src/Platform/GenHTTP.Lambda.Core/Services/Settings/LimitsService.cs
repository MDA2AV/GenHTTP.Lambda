using System.Globalization;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Analytics;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Settings;

/// <summary>
/// What a lambda may have and do in its tier, and what holds for every
/// lambda alike, as the operator set it in the administration panel.
/// </summary>
/// <remarks>
/// These are the product's promises rather than the server's configuration,
/// so they are changed while it runs rather than by restarting it with other
/// environment variables. They are kept in the same table as the switches of
/// <see cref="SettingsService"/>, a row each, and a missing row is the default
/// - which is what <see cref="LambdaOptions"/> says, so what a host sets for
/// its installation holds until the operator saves the form.
///
/// Read on every request a lambda answers (the room of its data, the rate
/// limit), so the values and the policies made of them are read once as the
/// platform starts (<see cref="LoadAsync"/>), held in memory and only written
/// through to the database - a request never waits for the records.
///
/// A change applies to what is checked next: a save, an upload, a write to the
/// workspace, a connection to a database, the next sweep. Nothing a lambda
/// already has is removed for being over a lowered limit, and nothing is
/// compiled again for a changed one - but for whether a tier connects by
/// default, which is part of what a lambda is compiled with.
/// </remarks>
public sealed class LimitsService(IDbContextFactory<LambdaDbContext> databases, LambdaOptions options) : ILimitsService
{
    private const string Prefix = "limits.";

    /// <summary>
    /// Who is changing the limits: one at a time, across the write.
    /// </summary>
    private readonly SemaphoreSlim _turn = new(1, 1);

    private volatile State? _current;

    /// <summary>
    /// The limits and the policy of every tier made of them, replaced together.
    /// </summary>
    private sealed record State(ProductLimits Limits, TierPolicy[] Policies);

    #region Reading

    /// <summary>
    /// The limits as they stand.
    /// </summary>
    public ProductLimits Get() => Current().Limits;

    /// <summary>
    /// What a lambda in the given tier may have and how it is treated.
    /// </summary>
    public TierPolicy PolicyOf(LambdaTier tier) => Current().Policies[(int)tier];

    /// <summary>
    /// The tiers whose policy says so.
    /// </summary>
    public IReadOnlyList<LambdaTier> TiersWhere(Func<TierPolicy, bool> predicate)
        => [.. Current().Policies.Where(predicate).Select(p => p.Tier)];

    /// <summary>
    /// The tier an app in the given one would be moved to for more, or
    /// nothing where there is none.
    /// </summary>
    public TierPolicy? Above(LambdaTier tier)
    {
        var policies = Current().Policies;

        return policies[(int)tier].Installation ? null : policies.Skip((int)tier + 1).FirstOrDefault(p => !p.Installation);
    }

    /// <summary>
    /// What a lambda in the given tier may keep in its workspace: the room of
    /// its data, which its database takes some of.
    /// </summary>
    /// <param name="enabled">Whether its owner left the workspace switched on</param>
    public WorkspaceLimits WorkspaceOf(LambdaTier tier, bool enabled = true) => new(PolicyOf(tier).MaxDataBytes, enabled);

    /// <summary>
    /// What the limits are where the operator has not set them: what the
    /// options say, which is the host's or the built in defaults.
    /// </summary>
    public static ProductLimits DefaultsOf(LambdaOptions options) => new(
        options.DefaultTier,
        options.Sandbox,
        options.Production,
        options.System,
        options.MaxVersions,
        options.MaxFeatures,
        options.DeploymentLifetime,
        options.Retention,
        options.MaxShowcaseImageBytes,
        options.RateLimit,
        options.AgentBuildsPerDay
    );

    private State Current()
        => _current ?? throw new InvalidOperationException("The limits of the installation are read as the platform starts, which it has not.");

    /// <summary>
    /// Reads the limits into memory, before anything is served.
    /// </summary>
    public async ValueTask LoadAsync(CancellationToken cancellation = default)
        => _current = Resolve(await ReadAsync(cancellation));

    private static State Resolve(ProductLimits limits)
        => new(limits, [.. TierPolicies.All.Select(t => TierPolicies.Of(t, limits))]);

    #endregion

    #region Writing

    /// <summary>
    /// Replaces the limits, every one of them.
    /// </summary>
    /// <remarks>
    /// Every value is written, not only those that differ from the default:
    /// what the operator saw in the form is what stays, whatever the
    /// options or a later release say the default is.
    /// </remarks>
    /// <returns>The limits that were saved, and those they replaced</returns>
    public async ValueTask<(ProductLimits Saved, ProductLimits Previous)> SaveAsync(ProductLimits limits, CancellationToken cancellation = default)
    {
        limits = Tidy(limits);

        Validate(limits);

        await _turn.WaitAsync(cancellation);

        try
        {
            var previous = Current();

            // in memory before it is written: the write makes what serving
            // holds about each lambda stale (DatabaseChanges), and what it
            // reads again then - whether a tier connects by default - has to
            // be what was saved, not what it replaces
            _current = Resolve(limits);

            try
            {
                await WriteAsync(limits, cancellation);
            }
            catch
            {
                _current = previous;
                throw;
            }

            return (limits, previous.Limits);
        }
        finally
        {
            _turn.Release();
        }
    }

    private async ValueTask WriteAsync(ProductLimits limits, CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        foreach (var (key, value) in Flatten(limits))
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

        // what the four allowances that became two were saved as, which
        // the two read until they were saved themselves
        foreach (var key in Retired.SelectMany(r => new[] { $"{Prefix}sandbox.{r}", $"{Prefix}production.{r}" }))
        {
            if (await database.Settings.FindAsync([key], cancellation) is { } retired)
            {
                database.Settings.Remove(retired);
            }
        }

        await database.SaveChangesAsync(cancellation);
    }

    /// <summary>
    /// Every limit by the key it is stored under, as it is stored.
    /// </summary>
    public static IEnumerable<(string Key, string Value)> Flatten(ProductLimits limits)
    {
        yield return ($"{Prefix}default-tier", LambdaTierNames.Name(limits.DefaultTier));

        foreach (var (tier, values) in Tiers(limits))
        {
            yield return ($"{Prefix}{tier}.build-bytes", Text(values.BuildBytes));
            yield return ($"{Prefix}{tier}.data-bytes", Text(values.DataBytes));
            yield return ($"{Prefix}{tier}.analytics-days", Text(values.AnalyticsDays));
            yield return ($"{Prefix}{tier}.connectivity", values.Connectivity ? "true" : "false");

            // written empty rather than left out, so a label the operator
            // cleared stays cleared instead of falling back to the host's
            yield return ($"{Prefix}{tier}.label", values.Label ?? string.Empty);
        }

        yield return ($"{Prefix}versions", Text(limits.Versions));
        yield return ($"{Prefix}features", Text(limits.Features));
        yield return ($"{Prefix}offline-after-hours", Text(limits.OfflineAfter.TotalHours));
        yield return ($"{Prefix}removed-after-hours", Text(limits.RemovedAfter.TotalHours));
        yield return ($"{Prefix}showcase-image-bytes", Text(limits.ShowcaseImageBytes));
        yield return ($"{Prefix}requests-per-second", Text(limits.RequestsPerSecond));
        yield return ($"{Prefix}builds-per-day", Text(limits.BuildsPerDay));
    }

    /// <summary>
    /// The tiers whose settings the operator sets, by the name they are stored under.
    /// </summary>
    private static IEnumerable<(string Tier, TierSettings Values)> Tiers(ProductLimits limits)
    {
        yield return ("sandbox", limits.Sandbox);
        yield return ("production", limits.Production);
        yield return ("system", limits.System);
    }

    /// <summary>
    /// Labels as they are kept: trimmed, and nothing where there was only space.
    /// </summary>
    private static ProductLimits Tidy(ProductLimits limits)
    {
        static TierSettings Labelled(TierSettings values)
            => values with { Label = string.IsNullOrWhiteSpace(values.Label) ? null : values.Label.Trim() };

        return limits with { Sandbox = Labelled(limits.Sandbox), Production = Labelled(limits.Production), System = Labelled(limits.System) };
    }

    /// <summary>
    /// Refuses limits a lambda could not live with, a tier in production that
    /// promises less than a sandbox, or a new lambda starting where only the
    /// installation puts one.
    /// </summary>
    private static void Validate(ProductLimits limits)
    {
        foreach (var (key, value) in Flatten(limits).Where(f => !f.Key.EndsWith(".label") && !f.Key.EndsWith(".connectivity") && f.Key != $"{Prefix}default-tier"))
        {
            if (!double.TryParse(value, CultureInfo.InvariantCulture, out var number) || number <= 0)
            {
                throw LambdaException.Invalid($"The limit '{key[Prefix.Length..]}' must be greater than zero.");
            }
        }

        var tiers = Tiers(limits).ToList();

        if (tiers.Any(t => t.Values.DataBytes < WorkspaceLimits.Block))
        {
            throw LambdaException.Invalid($"The data of a lambda needs room for at least one block of {WorkspaceLimits.Block} bytes.");
        }

        if (tiers.Any(t => t.Values.AnalyticsDays > AnalyticsStore.Kept))
        {
            throw LambdaException.Invalid($"Analytics are kept for {AnalyticsStore.Kept} days, so no tier can show more of them.");
        }

        if (tiers.FirstOrDefault(t => t.Values.Label?.Length > TierSettings.MaxLabel) is { Tier: { } labelled })
        {
            throw LambdaException.Invalid($"The label of the tier '{labelled}' must not be longer than {TierSettings.MaxLabel} characters.");
        }

        if (!Enum.IsDefined(limits.DefaultTier) || TierPolicies.Of(limits.DefaultTier, limits).Installation)
        {
            throw LambdaException.Invalid("A new lambda starts in a tier for the apps of the people who use the installation - the sandbox or production - not in one of the installation's own.");
        }

        var sandbox = limits.Sandbox;
        var production = limits.Production;

        var smaller = new (string Name, bool Smaller)[]
        {
            ("build bytes", production.BuildBytes < sandbox.BuildBytes),
            ("data bytes", production.DataBytes < sandbox.DataBytes),
            ("analytics days", production.AnalyticsDays < sandbox.AnalyticsDays)
        }.Where(c => c.Smaller).Select(c => c.Name).ToList();

        if (smaller.Count > 0)
        {
            throw LambdaException.Invalid($"Production must not allow less than the sandbox: {string.Join(", ", smaller)}.");
        }
    }

    #endregion

    #region Helpers

    private async ValueTask<ProductLimits> ReadAsync(CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var stored = await database.Settings.AsNoTracking()
                                   .Where(s => s.Key.StartsWith(Prefix))
                                   .ToDictionaryAsync(s => s.Key, s => s.Value, cancellation);

        var defaults = DefaultsOf(options);

        return new ProductLimits(
            stored.TryGetValue($"{Prefix}default-tier", out var tier) && LambdaTierNames.TryRead(tier, out var parsed) ? parsed : defaults.DefaultTier,
            ReadTier(stored, "sandbox", defaults.Sandbox),
            ReadTier(stored, "production", defaults.Production),
            ReadTier(stored, "system", defaults.System),
            (int)Number(stored, "versions", defaults.Versions),
            (int)Number(stored, "features", defaults.Features),
            TimeSpan.FromHours(Number(stored, "offline-after-hours", defaults.OfflineAfter.TotalHours)),
            TimeSpan.FromHours(Number(stored, "removed-after-hours", defaults.RemovedAfter.TotalHours)),
            (int)Number(stored, "showcase-image-bytes", defaults.ShowcaseImageBytes),
            (int)Number(stored, "requests-per-second", defaults.RequestsPerSecond),
            (int)Number(stored, "builds-per-day", defaults.BuildsPerDay)
        );
    }

    private static TierSettings ReadTier(Dictionary<string, string> stored, string tier, TierSettings defaults) => new(
        (long)Number(stored, $"{tier}.build-bytes", Before(stored, tier, "code-characters", "asset-bytes", defaults.BuildBytes)),
        (long)Number(stored, $"{tier}.data-bytes", Before(stored, tier, "workspace-bytes", "database-bytes", defaults.DataBytes)),
        (int)Number(stored, $"{tier}.analytics-days", defaults.AnalyticsDays),
        stored.TryGetValue(Prefix + $"{tier}.connectivity", out var connectivity) && bool.TryParse(connectivity, out var connects) ? connects : defaults.Connectivity,
        stored.TryGetValue(Prefix + $"{tier}.label", out var label) ? (label.Length > 0 ? label : null) : defaults.Label
    );

    /// <summary>
    /// The limits a panel saved before there were two allowances where there
    /// had been four, which the two are made of until they are saved.
    /// </summary>
    private static readonly string[] Retired = ["code-characters", "asset-bytes", "workspace-bytes", "database-bytes"];

    /// <summary>
    /// What an operator saved of the two allowances one of today's is made
    /// of, as their sum - or the default, where neither was saved.
    /// </summary>
    /// <remarks>
    /// The characters of code were held apart from the bytes of assets, and
    /// the room of the database apart from the workspace's: a version may now
    /// come to what both did together, and the data to what both kept. One
    /// that was saved without the other is added to the other's default.
    /// The system tier came later than they went, so it never has any.
    /// </remarks>
    private static double Before(Dictionary<string, string> stored, string tier, string first, string second, double fallback)
    {
        var one = Stored(stored, $"{tier}.{first}");
        var other = Stored(stored, $"{tier}.{second}");

        if (one == null && other == null)
        {
            return fallback;
        }

        double Default(string key) => tier == "production" ? LambdaOptions.RetiredDefaults[key].Production : LambdaOptions.RetiredDefaults[key].Sandbox;

        return (one ?? Default(first)) + (other ?? Default(second));
    }

    private static double? Stored(Dictionary<string, string> stored, string key)
        => stored.TryGetValue(Prefix + key, out var value) && double.TryParse(value, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    private static double Number(Dictionary<string, string> stored, string key, double fallback)
        => stored.TryGetValue(Prefix + key, out var value) && double.TryParse(value, CultureInfo.InvariantCulture, out var parsed) ? parsed : fallback;

    private static string Text(double value) => value.ToString(CultureInfo.InvariantCulture);

    #endregion

}
