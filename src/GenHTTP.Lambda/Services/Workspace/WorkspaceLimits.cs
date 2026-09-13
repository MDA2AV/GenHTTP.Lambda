namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// What a lambda may keep in its workspace.
/// </summary>
/// <remarks>
/// Compile time constants because the generated workspace class enforces them
/// from inside the lambda, where nothing of this application is reachable - so
/// the numbers are baked into that source rather than read from a service. This
/// is the one place they are written down, and both the generated code and the
/// editor read them from here.
/// </remarks>
public static class WorkspaceLimits
{

    /// <summary>
    /// The largest single file a lambda may write.
    /// </summary>
    public const int MaxFileSize = 1024 * 1024;

    /// <summary>
    /// How many files a workspace may hold.
    /// </summary>
    public const int MaxFiles = 64;

    /// <summary>
    /// Everything a workspace could hold at once, which is what the editor
    /// draws its quota bar against.
    /// </summary>
    public const long Quota = (long)MaxFileSize * MaxFiles;

}
