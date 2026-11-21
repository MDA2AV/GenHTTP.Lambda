using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Services.Census;
using GenHTTP.Lambda.Services.Integrations;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Optimization;
using GenHTTP.Lambda.Services.QuietHours;
using GenHTTP.Lambda.Services.Settings;
using GenHTTP.Lambda.Services.Telemetry;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What the operator has switched on or off for the whole installation, and chosen.")]
public sealed record SettingsResponse(
    [Summary("Whether the platform optimizes the databases itself - its own records and analytics, and every lambda's - each at most once a day, in its quiet hours (see `GET /admin/optimization`)")]
    bool OptimizeDatabases,
    [Summary("Who the installation is for, which its pages and its guide for agents speak to: `cloud` (a public platform), `home` (somebody, their household and friends) or `enterprise` (an organization and its people). It changes words, examples and what agents are told, never what the platform allows")]
    string Profile,
    [Summary("The profiles the operator may choose among; none where the host decided which the installation is")]
    IReadOnlyList<string> Profiles
)
{

    public static SettingsResponse Of(ISettingsService settings)
    {
        var current = settings.Get();

        return new(current.OptimizeDatabases, InstallationProfiles.NameOf(current.Profile), [.. settings.Profiles.Select(InstallationProfiles.NameOf)]);
    }

}
