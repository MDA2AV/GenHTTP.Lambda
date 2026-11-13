using GenHTTP.Lambda;
using GenHTTP.Lambda.Administration;
using GenHTTP.Lambda.Api;
using GenHTTP.Lambda.Cloud;
using GenHTTP.Lambda.Editing;
using GenHTTP.Lambda.Execution;
using GenHTTP.Lambda.Mcp;
using GenHTTP.Lambda.Server;
using GenHTTP.Lambda.Services.Hosting;
using GenHTTP.Lambda.Sources;

using GenHTTP.Modules.Layouting;

// the LAMBDA_* variables, with the lambdas at subdomains of LAMBDA_HOSTING_URL
var settings = LambdaSettings.FromEnvironment();

await using var platform = LambdaPlatform.Create()
                                         .Options(CloudInstallation.Of(settings.Platform))
                                         .Build();

var site = Layout.Create()
                 .Add(PlatformRoutes.Api, RestApi.Create(platform))
                 .Add(PlatformRoutes.Mcp, McpServer.Create(platform))
                 .Add(PlatformRoutes.Editor, Editor.Create(platform))
                 .Add(PlatformRoutes.Admin, AdminPanel.Create())
                 .Add(SourcePages.Create(platform))
                 .Add(Website.Create(platform));

return await LambdaServer.Create(platform)
                         .Settings(settings)
                         .Handler(Runner.Create(platform).Site(site))
                         .RunAsync();
