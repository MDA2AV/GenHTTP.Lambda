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
public sealed class SettingsService(IDbContextFactory<LambdaDbContext> databases) : ISettingsService
{
    private const string EnterprisePageKey = "enterprise-page";

    private const string BuildBoxKey = "build-box";

    private const string ChangeBoxKey = "change-box";

    private readonly Lock _lock = new();

    private SiteSettings? _current;

    /// <summary>
    /// The settings as they stand.
    /// </summary>
    public SiteSettings Get()
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
    /// Replaces the settings.
    /// </summary>
    public SiteSettings Save(SiteSettings settings)
    {
        lock (_lock)
        {
            using var database = databases.CreateDbContext();

            Write(database, EnterprisePageKey, settings.EnterprisePage);
            Write(database, BuildBoxKey, settings.BuildBox);
            Write(database, ChangeBoxKey, settings.ChangeBox);

            database.SaveChanges();

            return _current = settings;
        }
    }

    private SiteSettings Read()
    {
        using var database = databases.CreateDbContext();

        var stored = database.Settings.AsNoTracking().ToDictionary(s => s.Key, s => s.Value);

        return new SiteSettings(
            EnterprisePage: Flag(stored, EnterprisePageKey, SiteSettings.Default.EnterprisePage),
            BuildBox: Flag(stored, BuildBoxKey, SiteSettings.Default.BuildBox),
            ChangeBox: Flag(stored, ChangeBoxKey, SiteSettings.Default.ChangeBox)
        );
    }

    private static bool Flag(Dictionary<string, string> stored, string key, bool fallback)
        => stored.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : fallback;

    private static void Write(LambdaDbContext database, string key, bool value)
    {
        var text = value.ToString().ToLowerInvariant();

        var existing = database.Settings.Find(key);

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
/// <param name="BuildBox">
/// Whether /build offers its text box. Off, the page only explains how to
/// connect an agent of one's own. Either way there is no box without an agent.
/// </param>
/// <param name="ChangeBox">
/// Whether the Change section of the editor offers its text box. Off, it only
/// explains how to connect an agent of one's own.
/// </param>
public sealed record SiteSettings(bool EnterprisePage, bool BuildBox, bool ChangeBox)
{

    public static readonly SiteSettings Default = new(EnterprisePage: true, BuildBox: true, ChangeBox: true);

}
