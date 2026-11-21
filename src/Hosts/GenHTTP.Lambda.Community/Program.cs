using GenHTTP.Lambda;
using GenHTTP.Lambda.Administration;
using GenHTTP.Lambda.Api;
using GenHTTP.Lambda.Community;
using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Editing;
using GenHTTP.Lambda.Execution;
using GenHTTP.Lambda.Mcp;
using GenHTTP.Lambda.Server;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Sources;

using GenHTTP.Modules.Layouting;

// the LAMBDA_* variables, with the lambdas below /lambda/ on this address
// unless LAMBDA_HOSTING_URL names a domain for them, the daily usage report
// on unless LAMBDA_USAGE_REPORTS=false, and the data in SQLite unless
// LAMBDA_DATABASE=postgresql
var settings = LambdaSettings.FromEnvironment(LambdaAddressing.Paths, usageReports: true, databases: CommunityDatabase.Offered);

// the image's health check: asks the server running beside it, and starts nothing
if (HealthProbe.IsRequested(args))
{
    return await HealthProbe.CheckAsync(settings.Server);
}

// whoever runs it may give it a name and a logo of their own, in the panel,
// and say whom it is for: themselves (home) or an organization (enterprise)
await using var platform = await LambdaPlatform.Create()
                                               .Options(settings.Platform with
                                               {
                                                   Branding = true,
                                                   Profile = InstallationProfile.Home,
                                                   Profiles = [InstallationProfile.Home, InstallationProfile.Enterprise]
                                               })
                                               .Database(CommunityDatabase.Of(settings.Database))
                                               .BuildAsync();

var site = Layout.Create()
                 .Add(PlatformRoutes.Api, RestApi.Create(platform))
                 .Add(PlatformRoutes.Mcp, McpServer.Create(platform))
                 .Add(PlatformRoutes.Editor, Editor.Create(platform))
                 .Add(PlatformRoutes.Admin, AdminPanel.Create(platform))
                 .Add(SourcePages.Create(platform))
                 .Add(LandingPage.Create(platform));

return await LambdaServer.Create(platform)
                         .Settings(settings)
                         .Handler(Runner.Create(platform).Site(site))
                         .RunAsync();
