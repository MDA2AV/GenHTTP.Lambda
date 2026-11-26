using GenHTTP.Lambda.Integrations;

namespace GenHTTP.Lambda.Services.Integrations;

/// <summary>
/// The names the kinds of integration are given wherever they are written
/// out: in the panel, the API and the operator's MCP endpoint.
/// </summary>
/// <remarks>
/// Spelled out rather than taken from the enum, so a rename in the code does
/// not rename what callers read.
/// </remarks>
public static class IntegrationKindNames
{

    public static string Name(IntegrationKind kind) => kind switch
    {
        IntegrationKind.Database => "database",
        IntegrationKind.Files => "files",
        IntegrationKind.Secrets => "secrets",
        IntegrationKind.Agent => "agent",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "There is no such kind of integration.")
    };

}
