namespace GenHTTP.Lambda.Api.Infrastructure;

/// <summary>
/// What a page asks the agent for, as the requests of the API say it.
/// </summary>
internal static class AgentRequests
{

    /// <summary>
    /// The values asked for, by option - with the model and the effort a page
    /// from before named on their own, where it did and the options do not.
    /// </summary>
    public static IReadOnlyDictionary<string, string?> OptionsOf(IReadOnlyDictionary<string, string>? options, string? model, string? effort)
    {
        var asked = new Dictionary<string, string?>(StringComparer.Ordinal);

        foreach (var (option, value) in options ?? new Dictionary<string, string>())
        {
            asked[option.Trim().ToLowerInvariant()] = value;
        }

        if (!string.IsNullOrWhiteSpace(model))
        {
            asked.TryAdd("model", model);
        }

        if (!string.IsNullOrWhiteSpace(effort))
        {
            asked.TryAdd("effort", effort);
        }

        return asked;
    }

}
