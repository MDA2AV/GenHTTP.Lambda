using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Settings;

/// <summary>
/// What the operator can switch on or off without restarting the host.
/// </summary>
/// <remarks>
/// Read on every page view by the header, so the values are read once as the
/// platform starts (<see cref="LoadAsync"/>), held in memory and only written
/// through to the database - a page view never waits for the records.
/// </remarks>
public sealed class SettingsService(IDbContextFactory<LambdaDbContext> databases) : ISettingsService
{
    private const string BuildBoxKey = "build-box";

    private const string ChangeBoxKey = "change-box";

    private const string OptimizationKey = "database-optimization";

    /// <summary>
    /// Who is changing the settings: one at a time, across the write.
    /// </summary>
    private readonly SemaphoreSlim _turn = new(1, 1);

    private SiteSettings? _current;

    /// <summary>
    /// The settings as they stand.
    /// </summary>
    public SiteSettings Get()
        => _current ?? throw new InvalidOperationException("The settings of the installation are read as the platform starts, which it has not.");

    /// <summary>
    /// Reads the settings into memory, before anything is served.
    /// </summary>
    public async ValueTask LoadAsync(CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var stored = await database.Settings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value, cancellation);

        _current = new SiteSettings(
            BuildBox: Flag(stored, BuildBoxKey, SiteSettings.Default.BuildBox),
            ChangeBox: Flag(stored, ChangeBoxKey, SiteSettings.Default.ChangeBox),
            OptimizeDatabases: Flag(stored, OptimizationKey, SiteSettings.Default.OptimizeDatabases)
        );
    }

    /// <summary>
    /// Replaces the settings.
    /// </summary>
    public async ValueTask<SiteSettings> SaveAsync(SiteSettings settings, CancellationToken cancellation = default)
    {
        await _turn.WaitAsync(cancellation);

        try
        {
            await using var database = await databases.CreateDbContextAsync(cancellation);

            await WriteAsync(database, BuildBoxKey, settings.BuildBox, cancellation);
            await WriteAsync(database, ChangeBoxKey, settings.ChangeBox, cancellation);
            await WriteAsync(database, OptimizationKey, settings.OptimizeDatabases, cancellation);

            await database.SaveChangesAsync(cancellation);

            return _current = settings;
        }
        finally
        {
            _turn.Release();
        }
    }

    private static bool Flag(Dictionary<string, string> stored, string key, bool fallback)
        => stored.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : fallback;

    private static async ValueTask WriteAsync(LambdaDbContext database, string key, bool value, CancellationToken cancellation)
    {
        var text = value.ToString().ToLowerInvariant();

        var existing = await database.Settings.FindAsync([key], cancellation);

        if (existing == null)
        {
            database.Settings.Add(new SettingEntity { Key = key, Value = text });
        }
        else
        {
            existing.Value = text;
        }
    }

}

/// <summary>
/// The switches of the installation.
/// </summary>
/// <param name="BuildBox">
/// Whether the site offers the text box that has the agent build an app - on
/// /build, or the front page of an installation without one. Off, the page
/// only explains how to connect an agent of one's own. Either way there is no
/// box without an agent.
/// </param>
/// <param name="ChangeBox">
/// Whether the Change section of the editor offers its text box. Off, it only
/// explains how to connect an agent of one's own.
/// </param>
/// <param name="OptimizeDatabases">
/// Whether the platform keeps the databases in shape itself - its own and the
/// lambdas', each in its quiet hours. Off for an operator whose database is
/// looked after by its own tools, such as a PostgreSQL server with its own
/// maintenance.
/// </param>
public sealed record SiteSettings(bool BuildBox, bool ChangeBox, bool OptimizeDatabases = true)
{

    public static readonly SiteSettings Default = new(BuildBox: true, ChangeBox: true, OptimizeDatabases: true);

}
