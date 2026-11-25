using System.Text.RegularExpressions;

namespace GenHTTP.Lambda.Services.Guide;

/// <summary>
/// The shared files the operator's section of the guide links to.
/// </summary>
/// <remarks>
/// Written as <c>[our logo](shared/logo.svg)</c> - relative, since the
/// operator knows the file and not under which address the installation will
/// be reached - and handed to agents with the address a page they build needs. Nothing else in the section is touched: a relative
/// link to anything else has nowhere to lead in a guide, and is the
/// operator's to say.
/// </remarks>
internal static partial class GuideLinks
{

    /// <summary>
    /// The target of a link or a picture - inline, or named at the end.
    /// </summary>
    [GeneratedRegex(@"(?<=\]\(\s*<?|^[ ]{0,3}\[[^\]\n]+\]:[ \t]*<?)(?:\./)?shared/(?<name>[A-Za-z0-9][A-Za-z0-9._-]*)", RegexOptions.Multiline)]
    private static partial Regex Link();

    #region Functionality

    /// <summary>
    /// The names of the files a text links to.
    /// </summary>
    public static IReadOnlySet<string> Names(string? text)
        => text == null ? new HashSet<string>() : Link().Matches(text).Select(m => m.Groups["name"].Value).ToHashSet(StringComparer.Ordinal);

    /// <summary>
    /// The text with the address of every file it links to.
    /// </summary>
    /// <param name="address">What the files are found below</param>
    public static string Resolved(string text, string address)
        => Link().Replace(text, m => address + m.Groups["name"].Value);

    #endregion

}
