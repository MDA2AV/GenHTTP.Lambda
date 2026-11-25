namespace GenHTTP.Lambda.Services.SharedFiles;

/// <summary>
/// What uses a shared file as the API and the tools name it, and as a refusal
/// says it.
/// </summary>
public static class SharedFileUseNames
{

    public static string Name(SharedFileUse use) => use switch
    {
        SharedFileUse.TextFont => "textFont",
        SharedFileUse.HeadingFont => "headingFont",
        SharedFileUse.Guide => "guide",
        _ => throw new ArgumentOutOfRangeException(nameof(use), use, "There is no such use of a shared file.")
    };

    /// <summary>
    /// What it is in a sentence: <c>the font of the text</c>.
    /// </summary>
    public static string Words(SharedFileUse use) => use switch
    {
        SharedFileUse.TextFont => "the branding's text font",
        SharedFileUse.HeadingFont => "the branding's heading font",
        SharedFileUse.Guide => "the operator's section of the guide",
        _ => throw new ArgumentOutOfRangeException(nameof(use), use, "There is no such use of a shared file.")
    };

}
