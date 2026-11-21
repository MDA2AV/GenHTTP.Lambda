namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// How often the content of a file the stores keep was read - a resource's
/// apart from the rest of the code.
/// </summary>
/// <remarks>
/// What a version holds is read where somebody needs it and nowhere else:
/// compiling reads the C#, describing a version its C# and its
/// documentation, deploying copies its resources out to be served. These are
/// the counts that say so, which a test reads to hold the paths that need
/// no resource - a save, a check, a merge - to never opening one.
/// </remarks>
public sealed class ContentReads
{
    private long _resources;

    private long _code;

    /// <summary>
    /// How often a resource was read, or copied out to be served.
    /// </summary>
    public long Resources => Interlocked.Read(ref _resources);

    /// <summary>
    /// How often a file of the code was read: C#, documentation, anything else kept with it.
    /// </summary>
    public long Code => Interlocked.Read(ref _code);

    internal void Note(bool resource)
    {
        if (resource)
        {
            Interlocked.Increment(ref _resources);
        }
        else
        {
            Interlocked.Increment(ref _code);
        }
    }

}
