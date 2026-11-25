namespace GenHTTP.Lambda.Services.SharedFiles;

/// <summary>
/// The kinds of shared files as the API and the tools name them.
/// </summary>
public static class SharedFileKindNames
{

    public static string Name(SharedFileKind kind) => kind switch
    {
        SharedFileKind.Picture => "picture",
        SharedFileKind.Font => "font",
        SharedFileKind.Style => "style",
        SharedFileKind.Text => "text",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "There is no such kind of shared file.")
    };

}
