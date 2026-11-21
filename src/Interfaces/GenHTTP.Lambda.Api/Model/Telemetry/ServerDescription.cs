using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Telemetry;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What is running, and on what.")]
public sealed record ServerDescription(
    [Summary("The engine serving the requests: `ioxide`, `kestrel` or `internal`")]
    string Engine,
    [Summary("The version of GenHTTP")]
    string Version,
    [Summary("The .NET runtime")]
    string Runtime,
    [Summary("The platform the runtime runs on: `linux-x64`, say")]
    string Platform,
    [Summary("Whether the server garbage collector is used")]
    bool ServerGarbageCollection,
    [Summary("How many processors the process may use")]
    int Processors,
    [Summary("When the server came up")]
    DateTime Started,
    [Summary("How long it has been up, in seconds")]
    long UptimeSeconds
);
