namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// What a caller changes of the agent's settings: what is left out stays as
/// it is.
/// </summary>
/// <param name="Access">Who may choose a value, as <c>open</c>, <c>password</c> or <c>off</c>, by option and value name</param>
/// <param name="Defaults">What each option takes where nobody chose, by option name</param>
/// <param name="Limits">What a run may take, replacing what was set: nothing of either for the agent's default, zero for no limit</param>
/// <param name="GuardedLimits">The same, for a run behind the password</param>
public sealed record AgentSettingsChange(bool? Builds = null, bool? Changes = null,
                                         IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>? Access = null,
                                         IReadOnlyDictionary<string, string>? Defaults = null,
                                         LimitSetting? Limits = null, LimitSetting? GuardedLimits = null)
{

    /// <summary>Whether it changes anything of the given option.</summary>
    public bool Touches(string option) => (Access?.ContainsKey(option) ?? false) || (Defaults?.ContainsKey(option) ?? false);

}
