namespace GenHTTP.Lambda.Services.Settings;

/// <summary>
/// The switches the operator sets for the whole installation in the panel.
/// </summary>
public interface ISettingsService
{

    /// <summary>
    /// The settings as they stand.
    /// </summary>
    SiteSettings Get();

    /// <summary>
    /// Replaces the settings.
    /// </summary>
    SiteSettings Save(SiteSettings settings);

}
