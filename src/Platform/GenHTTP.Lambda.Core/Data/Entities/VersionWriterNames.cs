namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// What a writer is called wherever it leaves the platform.
/// </summary>
public static class VersionWriterNames
{

    public static string? Name(VersionWriter? writer) => writer switch
    {
        VersionWriter.BuildAgent => "build-agent",
        _ => null
    };

}
