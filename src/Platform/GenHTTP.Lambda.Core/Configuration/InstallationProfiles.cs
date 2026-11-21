using System.Diagnostics.CodeAnalysis;

namespace GenHTTP.Lambda.Configuration;

/// <summary>
/// The names of the profiles where one becomes text - the setting it is kept
/// as, the APIs, the pages and the usage report - never their names in the
/// code, which may change.
/// </summary>
public static class InstallationProfiles
{

    /// <summary>
    /// What a profile is called outside the code.
    /// </summary>
    public static string NameOf(InstallationProfile profile) => profile switch
    {
        InstallationProfile.Cloud => "cloud",
        InstallationProfile.Home => "home",
        InstallationProfile.Enterprise => "enterprise",
        _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, "There is no such profile.")
    };

    /// <summary>
    /// The profile a name stands for, whatever its case.
    /// </summary>
    public static bool TryParse(string? name, [NotNullWhen(true)] out InstallationProfile? profile)
    {
        profile = name?.Trim().ToLowerInvariant() switch
        {
            "cloud" => InstallationProfile.Cloud,
            "home" => InstallationProfile.Home,
            "enterprise" => InstallationProfile.Enterprise,
            _ => null
        };

        return profile != null;
    }

}
