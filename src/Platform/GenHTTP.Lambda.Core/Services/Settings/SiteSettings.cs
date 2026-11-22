using GenHTTP.Lambda.Configuration;

namespace GenHTTP.Lambda.Services.Settings;

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
