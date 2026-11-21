namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// What an origin is called wherever it leaves the platform - the API, MCP,
/// the pages - spelled out here so the names never follow a rename in code.
/// </summary>
public static class VersionOriginNames
{

    public static string? Name(VersionOrigin? origin) => origin switch
    {
        VersionOrigin.Template => "template",
        VersionOrigin.Api => "api",
        VersionOrigin.Agent => "agent",
        VersionOrigin.Git => "git",
        VersionOrigin.Admin => "admin",
        VersionOrigin.System => "system",
        _ => null
    };

}
