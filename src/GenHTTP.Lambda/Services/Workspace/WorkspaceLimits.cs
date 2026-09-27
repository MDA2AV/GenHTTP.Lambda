namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// What a lambda may keep in its workspace.
/// </summary>
/// <remarks>
/// Baked into the generated workspace class as constants, because that class
/// enforces them from inside the lambda, where nothing of this application is
/// reachable - and a constant is also something the code of the lambda cannot
/// change. Which numbers are baked in depends on the tier of the lambda, so
/// they are chosen when it is compiled, and a lambda whose tier moves is
/// compiled again with the new ones (see <see cref="GenHTTP.Lambda.Configuration.LambdaOptions.WorkspaceOf"/>).
/// </remarks>
/// <param name="MaxFileSize">The largest single file a lambda may write</param>
/// <param name="MaxFiles">How many files a workspace may hold</param>
/// <param name="Quota">What all of them may come to together</param>
public sealed record WorkspaceLimits(int MaxFileSize, int MaxFiles, long Quota)
{

    /// <summary>
    /// The workspace of every lambda that is not in the premium tier, unless
    /// configured otherwise.
    /// </summary>
    /// <remarks>
    /// A file may be as large as everything the tier may ship as assets, and
    /// eight of them reach the quota. The number of files stays bounded
    /// because they are created by code that runs here: without a bound, a
    /// loop writing empty files would run the host out of them long before
    /// any byte limit noticed.
    /// </remarks>
    public static WorkspaceLimits Standard { get; } = new(32 * 1024 * 1024, 64, 256L * 1024 * 1024);

}
