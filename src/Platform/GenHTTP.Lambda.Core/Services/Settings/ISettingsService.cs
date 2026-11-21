using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Services.Settings;

/// <summary>
/// The switches the operator sets for the whole installation in the panel,
/// and who it is for.
/// </summary>
public interface ISettingsService
{

    /// <summary>
    /// The profiles the operator may choose among; none where the host
    /// decided which the installation is.
    /// </summary>
    IReadOnlyList<InstallationProfile> Profiles { get; }

    /// <summary>
    /// The settings as they stand.
    /// </summary>
    SiteSettings Get();

    /// <summary>
    /// Replaces the settings. A profile the host does not offer is refused.
    /// </summary>
    ValueTask<SiteSettings> SaveAsync(SiteSettings settings, CancellationToken cancellation = default);

}
