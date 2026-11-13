using System.Text.RegularExpressions;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// What a page sends along with a job, checked and cut to what the agent takes.
/// </summary>
public static partial class AgentInput
{

    /// <summary>The longest request the agent is handed; anything beyond it is cut off.</summary>
    private const int LongestPrompt = 2000;

    /// <summary>
    /// What was asked for, cut to what the agent takes.
    /// </summary>
    /// <param name="missing">What to say where nothing much was asked for</param>
    public static string Prompt(string? prompt, string missing)
    {
        var wanted = (prompt ?? "").Trim();

        if (wanted.Length < 3)
        {
            throw new ProviderException(ResponseStatus.BadRequest, missing);
        }

        return wanted.Length > LongestPrompt ? wanted[..LongestPrompt] : wanted;
    }

    /// <summary>A language code as the pages send one, or nothing where it is not.</summary>
    public static string? Language(string? language)
    {
        var code = (language ?? "").Trim().ToLowerInvariant();

        return LanguageCode().IsMatch(code) ? code : null;
    }

    [GeneratedRegex("^[a-z]{2}(-[a-z]{2})?$")]
    private static partial Regex LanguageCode();

}
