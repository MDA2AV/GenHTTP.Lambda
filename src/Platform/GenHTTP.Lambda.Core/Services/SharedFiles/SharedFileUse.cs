namespace GenHTTP.Lambda.Services.SharedFiles;

/// <summary>
/// What of the installation uses a shared file, and would be without it.
/// </summary>
/// <remarks>
/// What an app links to is not known here, and not looked for: an address is
/// public, and may be on a page anywhere.
/// </remarks>
public enum SharedFileUse
{

    /// <summary>
    /// The font its pages are set in.
    /// </summary>
    TextFont,

    /// <summary>
    /// The font of their headings.
    /// </summary>
    HeadingFont,

    /// <summary>
    /// The operator's section of the guide, which links to it.
    /// </summary>
    Guide

}
