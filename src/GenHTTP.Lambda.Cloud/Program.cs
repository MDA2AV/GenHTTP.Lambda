using GenHTTP.Lambda;
using GenHTTP.Lambda.Api;
using GenHTTP.Lambda.Cloud;
using GenHTTP.Lambda.Execution;
using GenHTTP.Lambda.Mcp;
using GenHTTP.Lambda.Server;

using GenHTTP.Modules.Layouting;

var settings = LambdaSettings.FromEnvironment();

await using var platform = LambdaPlatform.Create()
                                         .Options(settings.Platform with { Terms = CloudTerms.Text })
                                         .Build();

var site = Layout.Create()
                 .Add("api", RestApi.Create(platform))
                 .Add("mcp", McpServer.Create(platform))
                 .Add(Website.Create(platform));

return await LambdaServer.Create(platform)
                         .Settings(settings)
                         .Handler(Runner.Create(platform).Site(site))
                         .RunAsync();
