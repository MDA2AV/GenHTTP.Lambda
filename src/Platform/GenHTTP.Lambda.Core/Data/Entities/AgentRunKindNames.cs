namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// What the kind of a run is called wherever it leaves the platform.
/// </summary>
public static class AgentRunKindNames
{

    public static string Name(AgentRunKind kind) => kind switch
    {
        AgentRunKind.Change => "change",
        _ => "build"
    };

}
