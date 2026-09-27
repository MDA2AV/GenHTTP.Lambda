using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Settings;

/// <summary>
/// What the operator can switch on or off without restarting the host.
/// </summary>
/// <remarks>
/// Read on every page view by the header, so the values are held in memory
/// once they were read and only written through to the database.
/// </remarks>
public sealed class SettingsService(IDbContextFactory<LambdaDbContext> databases)
{
    private const string EnterprisePageKey = "enterprise-page";

    private readonly SemaphoreSlim _lock = new(1, 1);

    private SiteSettings? _current;

    /// <summary>
    /// The settings as they stand.
    /// </summary>
    public async ValueTask<SiteSettings> GetAsync(CancellationToken cancellation = default)
    {
        if (_current is { } current)
        {
            return current;
        }

        await _lock.WaitAsync(cancellation);

        try
        {
            return _current ??= await ReadAsync(cancellation);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Replaces the settings.
    /// </summary>
    public async ValueTask<SiteSettings> SaveAsync(SiteSettings settings, CancellationToken cancellation = default)
    {
        await _lock.WaitAsync(cancellation);

        try
        {
            await using var database = await databases.CreateDbContextAsync(cancellation);

            await WriteAsync(database, EnterprisePageKey, settings.EnterprisePage, cancellation);

            await database.SaveChangesAsync(cancellation);

            return _current = settings;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async ValueTask<SiteSettings> ReadAsync(CancellationToken cancellation)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var stored = await database.Settings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value, cancellation);

        return new SiteSettings(
            EnterprisePage: Flag(stored, EnterprisePageKey, SiteSettings.Default.EnterprisePage)
        );
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
/// <param name="EnterprisePage">Whether the enterprise page is linked from the header. It is reachable either way.</param>
public sealed record SiteSettings(bool EnterprisePage)
{

    public static readonly SiteSettings Default = new(EnterprisePage: true);

}
