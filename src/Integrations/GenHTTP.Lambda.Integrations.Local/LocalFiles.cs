namespace GenHTTP.Lambda.Integrations.Local;

/// <summary>
/// Keeps the files as directories in the data directory - the workspaces and
/// the platform's pictures - what every installation does unless it chooses
/// another place for them.
/// </summary>
/// <example>
/// <code>
/// LambdaPlatform.Create().Files(LocalFiles.Create())
/// </code>
/// </example>
public static class LocalFiles
{

    public static LocalFilesBuilder Create() => new();

}
