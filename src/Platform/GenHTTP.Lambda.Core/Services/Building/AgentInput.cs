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

    /// <summary>How many turns of a conversation the agent is handed, the newest ones.</summary>
    private const int MostTurns = 8;

    /// <summary>The longest earlier message the agent is handed, and the longest answer.</summary>
    private const int LongestTurnPrompt = 600, LongestReply = 1000;

    /// <summary>What the turns may come to together, beside the message itself.</summary>
    private const int LongestConversation = 8000;

    /// <summary>
    /// The conversation a message was sent with, cut to what the agent takes:
    /// the newest turns that say something, each clipped, as many as fit.
    /// </summary>
    /// <remarks>
    /// Sent by the page, which keeps it, so it is no more trustworthy than the
    /// message is - the brief says so - and it is held to a size the runner
    /// takes, beside a message at its longest.
    /// </remarks>
    public static IReadOnlyList<ConversationTurn>? History(IReadOnlyList<ConversationTurn>? history)
    {
        if (history is not { Count: > 0 })
        {
            return null;
        }

        var kept = new List<ConversationTurn>();
        var room = LongestConversation;

        foreach (var turn in history.Reverse())
        {
            var asked = Clip(turn.Prompt, LongestTurnPrompt);

            if (asked == null)
            {
                continue;
            }

            var answered = Clip(turn.Reply, LongestReply);

            room -= asked.Length + (answered?.Length ?? 0);

            if (room < 0 || kept.Count == MostTurns)
            {
                break;
            }

            kept.Add(new ConversationTurn(asked, answered));
        }

        kept.Reverse();

        return kept.Count > 0 ? kept : null;
    }

    private static string? Clip(string? text, int longest)
    {
        var trimmed = (text ?? "").Trim();

        return trimmed.Length == 0 ? null : trimmed.Length > longest ? trimmed[..longest] : trimmed;
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
