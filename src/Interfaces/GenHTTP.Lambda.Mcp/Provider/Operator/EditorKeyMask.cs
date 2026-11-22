using System.Text.RegularExpressions;

namespace GenHTTP.Lambda.Mcp;

/// <summary>
/// Puts the editor keys in the paths of a log line out of sight, for the
/// operator's endpoint.
/// </summary>
/// <remarks>
/// A request line names the path as it was requested, editor keys included:
/// the log is behind the admin token, so the panel shows it as it is. An
/// answer of the operator's endpoint goes into an agent's transcript as well,
/// and no other answer there holds an editor key. The operator who needs one
/// reads the panel's log.
/// </remarks>
internal static partial class EditorKeyMask
{

    /// <summary>
    /// The text with the editor keys in its paths - of the editor, its git
    /// repositories and the API - put out of sight.
    /// </summary>
    public static string? Apply(string? text) => text == null ? null : EditorKeys().Replace(text, "$1(editor key)");

    /// <summary>
    /// An editor key where a path takes one: 32 characters of the alphabet
    /// keys are made of, after <c>/editor/</c> or <c>/lambdas/</c>.
    /// </summary>
    [GeneratedRegex("(/(?:editor|lambdas)/)[bcdfghjkmnpqrstvwxyz23456789]{32}\\b")]
    private static partial Regex EditorKeys();

}
