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
// unless LAMBDA_HOSTING_URL names a domain for them, and the daily usage
// report on unless LAMBDA_USAGE_REPORTS=false
var settings = LambdaSettings.FromEnvironment(LambdaAddressing.Paths, usageReports: true);

await using var platform = LambdaPlatform.Create()
                                         .Options(settings.Platform)
                                         .Build();

var site = Layout.Create()
                 .Add(PlatformRoutes.Api, RestApi.Create(platform))
                 .Add(PlatformRoutes.Mcp, McpServer.Create(platform))
                 .Add(PlatformRoutes.Editor, Editor.Create(platform))
                 .Add(PlatformRoutes.Admin, AdminPanel.Create())
                 .Add(SourcePages.Create(platform))
                 .Add(LandingPage.Create(platform));

return await LambdaServer.Create(platform)
                         .Settings(settings)
                         .Handler(Runner.Create(platform).Site(site))
                         .RunAsync();
