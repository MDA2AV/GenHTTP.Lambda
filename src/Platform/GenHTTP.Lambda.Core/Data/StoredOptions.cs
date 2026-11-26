using System.Text.Json;

using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Data;

/// <summary>
/// The options of the agent a run ran on, as a column keeps them: a JSON
/// object of option and value names, ordered by option.
/// </summary>
/// <remarks>
/// An open set - another integration brings other options - so text rather
/// than a column each, and ordered so two runs on the same options are kept
/// as the same text and counted together. What the build agent wrote before
/// the options were the integration's is read from the model and effort it
/// stored instead (<see cref="AgentModelNames"/>).
/// </remarks>
public static class StoredOptions
{

    public static string Write(IReadOnlyDictionary<string, string> options)
        => JsonSerializer.Serialize(new SortedDictionary<string, string>(options.ToDictionary(o => o.Key, o => o.Value), StringComparer.Ordinal));

    public static IReadOnlyDictionary<string, string> Read(string? stored)
    {
        if (string.IsNullOrWhiteSpace(stored))
        {
            return Empty;
        }

        try
        {
            return JsonSerializer.Deserialize<SortedDictionary<string, string>>(stored) is { } read
                ? new SortedDictionary<string, string>(read, StringComparer.Ordinal)
                : Empty;
        }
        catch (JsonException)
        {
            return Empty;
        }
    }

    /// <summary>
    /// The options of what was written: those stored with it, or - for what
    /// was written before - those its model and effort stand for.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Of(string? stored, AgentModel? model, AgentEffort? effort)
        => stored != null ? Read(stored) : AgentModelNames.Options(model, effort);

    /// <summary>
    /// How many of what was written ran on each value of each option.
    /// </summary>
    /// <param name="counted">The options of what was written, each with how many times</param>
    /// <param name="known">The values to count at zero where none ran on them, by option</param>
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> Tally(IEnumerable<(IReadOnlyDictionary<string, string> Options, int Count)> counted,
                                                                                    IReadOnlyDictionary<string, IReadOnlyList<string>>? known = null)
    {
        var tally = new SortedDictionary<string, SortedDictionary<string, int>>(StringComparer.Ordinal);

        foreach (var (option, values) in known ?? new Dictionary<string, IReadOnlyList<string>>())
        {
            tally[option] = new SortedDictionary<string, int>(values.Distinct().ToDictionary(v => v, _ => 0), StringComparer.Ordinal);
        }

        foreach (var (options, count) in counted)
        {
            foreach (var (option, value) in options)
            {
                if (!tally.TryGetValue(option, out var values))
                {
                    tally[option] = values = new SortedDictionary<string, int>(StringComparer.Ordinal);
                }

                values[value] = values.GetValueOrDefault(value) + count;
            }
        }

        return tally.ToDictionary(t => t.Key, t => (IReadOnlyDictionary<string, int>)t.Value, StringComparer.Ordinal);
    }

    private static readonly IReadOnlyDictionary<string, string> Empty = new SortedDictionary<string, string>(StringComparer.Ordinal);

}
