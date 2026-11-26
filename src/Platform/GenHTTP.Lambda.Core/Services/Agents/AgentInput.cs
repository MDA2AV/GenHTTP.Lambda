using System.Text.RegularExpressions;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Services.Agents;

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
    public const int MostTurns = 8;

    /// <summary>The longest earlier message the agent is handed, and the longest answer.</summary>
    private const int LongestTurnPrompt = 600, LongestReply = 1000;

    /// <summary>What the turns may come to together, beside the message itself.</summary>
    private const int LongestConversation = 8000;

    /// <summary>
    /// The conversation a message was sent with, cut to what the agent takes:
    /// the newest turns that say something, each clipped, as many as fit.
    /// </summary>
    /// <remarks>
    /// The platform keeps the runs of an app itself and hands the agent
    /// those, cut the same way; the conversation a page sends is taken where
    /// it keeps none - an app asked about before the platform kept runs,
    /// whose turns only the browser still has. Either is somebody's typing
    /// and the agent's answers, so it is context, never instructions, and is
    /// held to a size beside a message at its longest.
    /// </remarks>
    public static IReadOnlyList<ConversationTurn>? History(IReadOnlyList<ConversationTurn>? history)
    {
        var kept = Cut(history ?? [], t => t.Prompt, t => t.Reply);

        return kept.Count > 0 ? [.. kept.Select(k => new ConversationTurn(k.Prompt, k.Reply))] : null;
    }

    /// <summary>
    /// The newest turns of a conversation, oldest first, that say something
    /// and fit - each clipped - with what each was made of.
    /// </summary>
    public static IReadOnlyList<(T Turn, string Prompt, string? Reply)> Cut<T>(IReadOnlyList<T> oldestFirst, Func<T, string?> prompt, Func<T, string?> reply)
    {
        var kept = new List<(T, string, string?)>();
        var room = LongestConversation;

        foreach (var turn in oldestFirst.Reverse())
        {
            var asked = Clip(prompt(turn), LongestTurnPrompt);

            if (asked == null)
            {
                continue;
            }

            var answered = Clip(reply(turn), LongestReply);

            room -= asked.Length + (answered?.Length ?? 0);

            if (room < 0 || kept.Count == MostTurns)
            {
                break;
            }

            kept.Add((turn, asked, answered));
        }

        kept.Reverse();

        return kept;
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
