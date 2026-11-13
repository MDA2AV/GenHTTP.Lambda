namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// What a lambda may keep in its workspace: an amount of room, and nothing
/// else - neither the number of files nor the size of one is limited.
/// </summary>
/// <remarks>
/// Baked into the generated workspace class as a constant, because that class
/// enforces it from inside the lambda, where nothing of this application is
/// reachable - and a constant is also something the code of the lambda cannot
/// change. Which number is baked in depends on the tier of the lambda, so it
/// is chosen when it is compiled, and a lambda whose tier moves is compiled
/// again with the new one, and so is every lambda once the operator changes
/// the quota of its tier (see <see cref="GenHTTP.Lambda.Services.Settings.LimitsService.WorkspaceOf"/>).
///
/// Room is counted the way the disk counts it, in blocks: every file takes a
/// whole number of them and at least one, and so does every folder. Counted in
/// bytes, an empty file would be free, and a loop writing them would run the
/// host out of files long before any quota noticed.
///
/// Whether the lambda has a workspace at all is baked in the same way, for the
/// same reasons: its owner can switch it off, and a lambda compiled while it
/// was on is compiled again once it is not.
/// </remarks>
/// <param name="Quota">How much room the workspace may take, in bytes</param>
/// <param name="Enabled">Whether the owner left the workspace switched on</param>
public sealed record WorkspaceLimits(long Quota, bool Enabled = true)
{

    /// <summary>
    /// The unit room is counted in.
    /// </summary>
    public const int Block = 4096;

    /// <summary>
    /// The workspace of every lambda that is not in the premium tier, unless
    /// configured otherwise.
    /// </summary>
    public static WorkspaceLimits Standard { get; } = new(256L * 1024 * 1024);

    /// <summary>
    /// The room a file of the given length takes.
    /// </summary>
    public static long Footprint(long length) => Math.Max(1, (length + Block - 1) / Block) * Block;

}
