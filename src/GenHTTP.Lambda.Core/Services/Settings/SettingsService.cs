using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Settings;

/// <summary>
/// What the operator can switch on or off, or choose, without restarting the
/// host.
/// </summary>
/// <remarks>
/// Read by the platform's jobs as they run and by every page served, so the
/// values are read once as the platform starts (<see cref="LoadAsync"/>), held
/// in memory and only written through to the database - nothing waits for the
/// records.
///
/// The profile is the host's to offer (<see cref="LambdaOptions.Profiles"/>):
/// where it offers none, the installation is its <see cref="LambdaOptions.Profile"/>
/// whatever the database holds - a copy of the data of a community
/// installation brings nothing of it to the cloud - and choosing another is
/// refused.
///
/// The switches of the build agent were here once; they are the agent's
/// settings now (<c>Services/Building</c>), kept under the same rows.
/// </remarks>
public sealed class SettingsService(IDbContextFactory<LambdaDbContext> databases, LambdaOptions options) : ISettingsService
{
    private const string OptimizationKey = "database-optimization";

    private const string ProfileKey = "profile";

    /// <summary>
    /// Who is changing the settings: one at a time, across the write.
    /// </summary>
    private readonly SemaphoreSlim _turn = new(1, 1);

    private SiteSettings? _current;

    /// <summary>
    /// The profiles the operator may choose among; none where the host decided.
    /// </summary>
    public IReadOnlyList<InstallationProfile> Profiles => options.Profiles;

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
        if (options.Profiles.Count > 0 && !options.Profiles.Contains(options.Profile))
        {
            throw new InvalidOperationException($"The installation's profile ({InstallationProfiles.NameOf(options.Profile)}) is not one of those its host offers.");
        }

        await using var database = await databases.CreateDbContextAsync(cancellation);

        var stored = await database.Settings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value, cancellation);

        _current = new SiteSettings(
            OptimizeDatabases: Flag(stored, OptimizationKey, SiteSettings.Default.OptimizeDatabases),
            Profile: Profile(stored)
        );
    }

    /// <summary>
    /// Replaces the settings.
    /// </summary>
    public async ValueTask<SiteSettings> SaveAsync(SiteSettings settings, CancellationToken cancellation = default)
    {
        if (!Offered(settings.Profile))
        {
            throw LambdaException.Invalid(options.Profiles.Count == 0
                                              ? $"This installation is {InstallationProfiles.NameOf(options.Profile)}, which its host decides."
                                              : $"The profile may be {string.Join(" or ", options.Profiles.Select(InstallationProfiles.NameOf))} here.");
        }

        await _turn.WaitAsync(cancellation);

        try
        {
            await using var database = await databases.CreateDbContextAsync(cancellation);

            await WriteAsync(database, OptimizationKey, settings.OptimizeDatabases.ToString().ToLowerInvariant(), cancellation);

            // the host's own profile is not written down: what is not chosen
            // stays the host's, should it ever offer another default
            if (options.Profiles.Count > 0)
            {
                await WriteAsync(database, ProfileKey, InstallationProfiles.NameOf(settings.Profile), cancellation);
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
    /// The profile the operator chose, where the host lets them and it is one
    /// it offers - the host's otherwise.
    /// </summary>
    private InstallationProfile Profile(Dictionary<string, string> stored)
        => options.Profiles.Count > 0 && stored.TryGetValue(ProfileKey, out var name) && InstallationProfiles.TryParse(name, out var chosen) && options.Profiles.Contains(chosen.Value)
               ? chosen.Value
               : options.Profile;

    private bool Offered(InstallationProfile profile)
        => options.Profiles.Count > 0 ? options.Profiles.Contains(profile) : profile == options.Profile;

    private static bool Flag(Dictionary<string, string> stored, string key, bool fallback)
        => stored.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : fallback;

    private static async ValueTask WriteAsync(LambdaDbContext database, string key, string value, CancellationToken cancellation)
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

}

/// <summary>
/// The switches and choices of the installation.
/// </summary>
/// <param name="OptimizeDatabases">
/// Whether the platform keeps the databases in shape itself - its own and the
/// lambdas', each in its quiet hours. Off for an operator whose database is
/// looked after by its own tools, such as a PostgreSQL server with its own
/// maintenance.
/// </param>
/// <param name="Profile">
/// Who the installation is for, which the pages and the guide for agents
/// speak to - and nothing else (see <see cref="InstallationProfile"/>).
/// </param>
public sealed record SiteSettings(bool OptimizeDatabases = true, InstallationProfile Profile = InstallationProfile.Home)
{

    public static readonly SiteSettings Default = new(OptimizeDatabases: true);

}
