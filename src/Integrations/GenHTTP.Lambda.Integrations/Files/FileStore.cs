namespace GenHTTP.Lambda.Integrations.Files;

/// <summary>
/// The stores the platform keeps its own files in.
/// </summary>
/// <remarks>
/// What a store is called where it is kept - a directory, a prefix - is the
/// provider's to decide, and stays what it was once files are kept under it.
/// </remarks>
public enum FileStore
{

    /// <summary>
    /// What the installation presents itself with: its logo.
    /// </summary>
    Branding,

    /// <summary>
    /// The pictures of the showcase, one per lambda that is listed there.
    /// </summary>
    Showcase

}
