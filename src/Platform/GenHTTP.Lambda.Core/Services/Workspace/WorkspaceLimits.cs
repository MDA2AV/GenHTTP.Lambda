namespace GenHTTP.Lambda.Services.Workspace;

/// <summary>
/// What a lambda may keep in its workspace: an amount of room, and nothing
/// else - neither the number of files nor the size of one is limited.
/// </summary>
/// <remarks>
/// Enforced by the generated workspace class from inside the lambda, where
/// nothing of this application is reachable: the quota is read through a
/// function the platform hands the class once the lambda is loaded
/// (<c>IDatabaseVault.DataRoomFor</c>), as the lambda's tier allows it now -
/// so a lambda moved to another tier, or whose tier the operator gives other
/// room, writes into the new room from its next write, without being
/// compiled or started again.
///
/// Room is counted the way the disk counts it, in blocks: every file takes a
/// whole number of them and at least one, and so does every folder. Counted in
/// bytes, an empty file would be free, and a loop writing them would run the
/// host out of files long before any quota noticed.
///
/// Whether the lambda has a workspace at all is compiled in: its owner can
/// switch it off, and a lambda compiled while it was on is compiled again
/// once it is not.
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
    /// The workspace of a sandbox lambda, unless configured otherwise.
    /// </summary>
    public static WorkspaceLimits Standard { get; } = new(256L * 1024 * 1024);

    /// <summary>
    /// The room a file of the given length takes.
    /// </summary>
    public static long Footprint(long length) => Math.Max(1, (length + Block - 1) / Block) * Block;

}
