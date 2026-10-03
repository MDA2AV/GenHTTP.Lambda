using System.Globalization;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Settings;

/// <summary>
/// What a lambda may have and do in its tier, as the operator set it in the
/// administration panel.
/// </summary>
/// <remarks>
/// These are the product's promises rather than the server's configuration,
/// so they are changed while it runs rather than by restarting it with other
/// environment variables. They are kept in the same table as the switches of
/// <see cref="SettingsService"/>, a row each, and a missing row is the default
/// - which is what <see cref="LambdaOptions"/> says, so the environment
/// variables that used to set them still do until the operator saves the form.
///
/// Read on every request a lambda answers (the workspace it is compiled with,
/// the rate limit), so the values are held in memory once they were read and
/// only written through to the database.
///
/// A change applies to what is checked next: a save, an upload, a deploy, a
/// connection to a database. Nothing a lambda already has is removed for being
/// over a lowered limit. A lambda's workspace quota is compiled into it, so a
/// changed one compiles each lambda again on its next request.
/// </remarks>
public sealed class LimitsService(IDbContextFactory<LambdaDbContext> databases, LambdaOptions options)
{
    private const string Prefix = "limits.";

    private readonly Lock _lock = new();

    private ProductLimits? _current;

    #region Reading

    /// <summary>
    /// The limits as they stand.
    /// </summary>
    public ProductLimits Get()
    {
        if (_current is { } current)
        {
            return current;
        }

        lock (_lock)
        {
            return _current ??= Read();
        }
    }

    /// <summary>
    /// What a lambda in the given tier may have. A demo is held to what a free
    /// lambda may have.
    /// </summary>
    public TierLimits Of(LambdaTier tier)
    {
        var current = Get();

        return tier == LambdaTier.Premium ? current.Premium : current.Free;
    }

    /// <summary>
    /// How many characters of C# a lambda in the given tier may have.
    /// </summary>
    /// <remarks>
    /// Never less for a premium lambda than for any other, however the two
    /// were configured: the panel refuses that, the environment did not.
    /// </remarks>
    public int MaxCodeLengthOf(LambdaTier tier)
    {
        var current = Get();

        return tier == LambdaTier.Premium ? Math.Max(current.Premium.CodeCharacters, current.Free.CodeCharacters) : current.Free.CodeCharacters;
    }

    /// <summary>
    /// How many bytes of assets a lambda in the given tier may ship.
    /// </summary>
    public int MaxAssetBytesOf(LambdaTier tier)
    {
        var current = Get();

        return tier == LambdaTier.Premium ? Math.Max(current.Premium.AssetBytes, current.Free.AssetBytes) : current.Free.AssetBytes;
    }

    /// <summary>
    /// What a lambda in the given tier may keep in its workspace.
    /// </summary>
    /// <param name="enabled">Whether its owner left the workspace switched on</param>
    public WorkspaceLimits WorkspaceOf(LambdaTier tier, bool enabled = true) => new(Of(tier).WorkspaceBytes, enabled);

    /// <summary>
    /// How large the database of a lambda in the given tier may grow.
    /// </summary>
    public long DatabaseOf(LambdaTier tier)
    {
        var current = Get();

        return tier == LambdaTier.Premium ? Math.Max(current.Premium.DatabaseBytes, current.Free.DatabaseBytes) : current.Free.DatabaseBytes;
    }

    /// <summary>
    /// What the limits are where the operator has not set them: what the
    /// options say, which is the environment or the built in defaults.
    /// </summary>
    public static ProductLimits DefaultsOf(LambdaOptions options) => new(
        new TierLimits(options.MaxCodeLength, options.MaxAssetBytes, options.WorkspaceBytes, options.DatabaseBytes, options.MaxVersions, options.MaxFeatures),
        new TierLimits(options.PremiumMaxCodeLength, options.PremiumMaxAssetBytes, options.PremiumWorkspaceBytes, options.PremiumDatabaseBytes, options.MaxVersions, options.MaxFeatures),
        options.DeploymentLifetime,
        options.Retention,
        options.MaxShowcaseImageBytes,
        options.RateLimit,
        options.AgentBuildsPerDay
    );

    #endregion

    #region Writing

    /// <summary>
    /// Replaces the limits, every one of them.
    /// </summary>
    /// <remarks>
    /// Every value is written, not only those that differ from the default:
    /// what the operator saw in the form is what stays, whatever the
    /// environment or a later release says the default is.
    /// </remarks>
    /// <returns>The limits that were saved, and those they replaced</returns>
    public (ProductLimits Saved, ProductLimits Previous) Save(ProductLimits limits)
    {
        Validate(limits);

        lock (_lock)
        {
            var previous = _current ?? Read();

            using var database = databases.CreateDbContext();

            foreach (var (key, value) in Flatten(limits))
            {
                var existing = database.Settings.Find(key);

                if (existing == null)
                {
                    database.Settings.Add(new SettingEntity { Key = key, Value = value });
                }
                else
                {
                    existing.Value = value;
                }
            }

            database.SaveChanges();

            return (_current = limits, previous);
        }
    }

    /// <summary>
    /// Every limit by the key it is stored under, as it is stored.
    /// </summary>
    public static IEnumerable<(string Key, string Value)> Flatten(ProductLimits limits)
    {
        foreach (var (tier, values) in new[] { ("free", limits.Free), ("premium", limits.Premium) })
        {
            yield return ($"{Prefix}{tier}.code-characters", Text(values.CodeCharacters));
            yield return ($"{Prefix}{tier}.asset-bytes", Text(values.AssetBytes));
            yield return ($"{Prefix}{tier}.workspace-bytes", Text(values.WorkspaceBytes));
            yield return ($"{Prefix}{tier}.database-bytes", Text(values.DatabaseBytes));
            yield return ($"{Prefix}{tier}.versions", Text(values.Versions));
            yield return ($"{Prefix}{tier}.features", Text(values.Features));
        }

        yield return ($"{Prefix}offline-after-hours", Text(limits.OfflineAfter.TotalHours));
        yield return ($"{Prefix}removed-after-hours", Text(limits.RemovedAfter.TotalHours));
        yield return ($"{Prefix}showcase-image-bytes", Text(limits.ShowcaseImageBytes));
        yield return ($"{Prefix}requests-per-second", Text(limits.RequestsPerSecond));
        yield return ($"{Prefix}builds-per-day", Text(limits.BuildsPerDay));
    }

    /// <summary>
    /// Refuses limits a lambda could not live with, or a premium tier that
    /// promises less than the free one.
    /// </summary>
    private static void Validate(ProductLimits limits)
    {
        foreach (var (key, value) in Flatten(limits))
        {
            if (!double.TryParse(value, CultureInfo.InvariantCulture, out var number) || number <= 0)
            {
                throw LambdaException.Invalid($"The limit '{key[Prefix.Length..]}' must be greater than zero.");
            }
        }

        if (limits.Free.WorkspaceBytes < WorkspaceLimits.Block || limits.Premium.WorkspaceBytes < WorkspaceLimits.Block)
        {
            throw LambdaException.Invalid($"A workspace needs room for at least one block of {WorkspaceLimits.Block} bytes.");
        }

        var free = limits.Free;
        var premium = limits.Premium;

        var smaller = new (string Name, bool Smaller)[]
        {
            ("code characters", premium.CodeCharacters < free.CodeCharacters),
            ("asset bytes", premium.AssetBytes < free.AssetBytes),
            ("workspace bytes", premium.WorkspaceBytes < free.WorkspaceBytes),
            ("database bytes", premium.DatabaseBytes < free.DatabaseBytes),
            ("versions", premium.Versions < free.Versions),
            ("features", premium.Features < free.Features)
        }.Where(c => c.Smaller).Select(c => c.Name).ToList();

        if (smaller.Count > 0)
        {
            throw LambdaException.Invalid($"The premium tier must not allow less than the free one: {string.Join(", ", smaller)}.");
        }
    }

    #endregion

    #region Helpers

    private ProductLimits Read()
    {
        using var database = databases.CreateDbContext();

        var stored = database.Settings.AsNoTracking()
                             .Where(s => s.Key.StartsWith(Prefix))
                             .ToDictionary(s => s.Key, s => s.Value);

        var defaults = DefaultsOf(options);

        return new ProductLimits(
            ReadTier(stored, "free", defaults.Free),
            ReadTier(stored, "premium", defaults.Premium),
            TimeSpan.FromHours(Number(stored, "offline-after-hours", defaults.OfflineAfter.TotalHours)),
            TimeSpan.FromHours(Number(stored, "removed-after-hours", defaults.RemovedAfter.TotalHours)),
            (int)Number(stored, "showcase-image-bytes", defaults.ShowcaseImageBytes),
            (int)Number(stored, "requests-per-second", defaults.RequestsPerSecond),
            (int)Number(stored, "builds-per-day", defaults.BuildsPerDay)
        );
    }

    private static TierLimits ReadTier(Dictionary<string, string> stored, string tier, TierLimits defaults) => new(
        (int)Number(stored, $"{tier}.code-characters", defaults.CodeCharacters),
        (int)Number(stored, $"{tier}.asset-bytes", defaults.AssetBytes),
        (long)Number(stored, $"{tier}.workspace-bytes", defaults.WorkspaceBytes),
        (long)Number(stored, $"{tier}.database-bytes", defaults.DatabaseBytes),
        (int)Number(stored, $"{tier}.versions", defaults.Versions),
        (int)Number(stored, $"{tier}.features", defaults.Features)
    );

    private static double Number(Dictionary<string, string> stored, string key, double fallback)
        => stored.TryGetValue(Prefix + key, out var value) && double.TryParse(value, CultureInfo.InvariantCulture, out var parsed) ? parsed : fallback;

    private static string Text(double value) => value.ToString(CultureInfo.InvariantCulture);

    #endregion

}

/// <summary>
/// What a lambda of one tier may have.
/// </summary>
/// <param name="CodeCharacters">Characters of C# across all its files</param>
/// <param name="AssetBytes">Bytes of assets, documentation and tests in a version</param>
/// <param name="WorkspaceBytes">Room its workspace may take</param>
/// <param name="DatabaseBytes">How large its database may grow</param>
/// <param name="Versions">How many of its versions are kept</param>
/// <param name="Features">How many features it may have open at once</param>
public sealed record TierLimits(int CodeCharacters, int AssetBytes, long WorkspaceBytes, long DatabaseBytes, int Versions, int Features);

/// <summary>
/// Every limit of the product.
/// </summary>
/// <param name="Free">What a free lambda, or a demo, may have</param>
/// <param name="Premium">What a premium lambda may have</param>
/// <param name="OfflineAfter">How long a free lambda may go unused before it is taken offline</param>
/// <param name="RemovedAfter">How long an unused free lambda is kept before it is removed</param>
/// <param name="ShowcaseImageBytes">How large the picture of a showcase entry may be</param>
/// <param name="RequestsPerSecond">How many requests one client may send to the lambdas in a second</param>
/// <param name="BuildsPerDay">How many builds and changes one address may ask the build agent for in a day</param>
/// <remarks>
/// A premium lambda is never taken offline or removed for not being used, so
/// the two durations are the free tier's alone. The last three are counted per
/// caller rather than per lambda, so they have no tier.
/// </remarks>
public sealed record ProductLimits(
    TierLimits Free,
    TierLimits Premium,
    TimeSpan OfflineAfter,
    TimeSpan RemovedAfter,
    int ShowcaseImageBytes,
    int RequestsPerSecond,
    int BuildsPerDay
);
