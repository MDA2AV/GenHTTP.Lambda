namespace GenHTTP.Lambda.Services.SharedFiles;

/// <summary>
/// What a shared file is for, which its type says.
/// </summary>
public enum SharedFileKind
{

    /// <summary>
    /// A picture: a logo, an illustration.
    /// </summary>
    Picture,

    /// <summary>
    /// A font, or one weight of one.
    /// </summary>
    Font,

    /// <summary>
    /// A style sheet for the pages of the apps.
    /// </summary>
    Style,

    /// <summary>
    /// Something to read: Markdown, plain text, data.
    /// </summary>
    Text

}
