using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Agents;

namespace GenHTTP.Lambda.Api.Model;

/// <remarks>
/// What is left out stays as it is, so a script that only switches building
/// off does not have to repeat the options. <c>models</c>, <c>model</c>,
/// <c>efforts</c> and <c>effort</c> are the options <c>model</c> and
/// <c>effort</c> as the panel set them before options were the agent
/// integration's, and mean what they meant then: a model <c>models</c> does
/// not name is offered to nobody, and so is an effort <c>efforts</c> does not.
/// </remarks>
[Summary("What the operator decides about the agent. What is left out stays as it is.")]
public sealed record AgentSettingsRequest(
    [Summary("Whether the workspace on /build builds new apps")]
    bool? Builds = null,
    [Summary("Whether owners may ask the agent about their apps")]
    bool? Changes = null,
    [Summary("Who may have each model, as the option `model`; a model this does not name is offered to nobody")]
    IReadOnlyList<AgentModelRequest>? Models = null,
    [Summary("The default of the option `model`")]
    string? Model = null,
    [Summary("The efforts that may be chosen, as the option `effort`: at least one; one this does not name is offered to nobody")]
    IReadOnlyList<string>? Efforts = null,
    [Summary("The default of the option `effort`")]
    string? Effort = null,
    [Summary("Who may choose the values of each option, and its default, by option")]
    IReadOnlyList<AgentOptionRequest>? Options = null,
    [Summary("What a run may take, where the agent holds runs to limits - replacing what was set")]
    AgentLimitsModel? Limits = null,
    [Summary("What a run on a value behind the password may take - replacing what was set")]
    AgentLimitsModel? GuardedLimits = null
)
{

    /// <summary>
    /// The change this asks for, by the names of the agent's options.
    /// </summary>
    public AgentSettingsChange Change(Integrations.Agents.AgentCapabilities? capabilities)
    {
        var access = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal);
        var defaults = new Dictionary<string, string>(StringComparer.Ordinal);

        void Everything(string option, IEnumerable<(string Value, string Access)> named)
        {
            var given = named.ToDictionary(n => n.Value.Trim().ToLowerInvariant(), n => n.Access, StringComparer.Ordinal);

            // as it meant before: what is not named is offered to nobody
            foreach (var value in capabilities?.Option(option)?.Values ?? [])
            {
                given.TryAdd(value.Name, "off");
            }

            access[option] = given;
        }

        if (Models != null)
        {
            Everything("model", Models.Select(m => (m.Name, m.Access)));
        }

        if (Efforts != null)
        {
            Everything("effort", Efforts.Select(e => (e, "open")));
        }

        if (Model != null)
        {
            defaults["model"] = Model.Trim().ToLowerInvariant();
        }

        if (Effort != null)
        {
            defaults["effort"] = Effort.Trim().ToLowerInvariant();
        }

        foreach (var option in Options ?? [])
        {
            var name = option.Name.Trim().ToLowerInvariant();

            if (option.Values != null)
            {
                var merged = access.TryGetValue(name, out var already) ? new Dictionary<string, string>(already, StringComparer.Ordinal) : new Dictionary<string, string>(StringComparer.Ordinal);

                foreach (var value in option.Values)
                {
                    merged[value.Name.Trim().ToLowerInvariant()] = value.Access;
                }

                access[name] = merged;
            }

            if (option.Default != null)
            {
                defaults[name] = option.Default.Trim().ToLowerInvariant();
            }
        }

        return new AgentSettingsChange(Builds, Changes, access.Count > 0 ? access : null, defaults.Count > 0 ? defaults : null,
                                       Limits is { } limits ? new LimitSetting(limits.Seconds, limits.Steps) : null,
                                       GuardedLimits is { } guarded ? new LimitSetting(guarded.Seconds, guarded.Steps) : null);
    }

}
