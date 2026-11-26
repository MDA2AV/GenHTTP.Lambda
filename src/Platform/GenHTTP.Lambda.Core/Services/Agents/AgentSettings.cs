using GenHTTP.Lambda.Integrations.Agents;

namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// What the operator decided about the installation's agent: whether it
/// builds and changes apps here, which values of its options are offered and
/// to whom, what a run takes where nobody chose, and what a run may take.
/// </summary>
/// <remarks>
/// <para>
/// Kept by the names the integration gives its options and their values,
/// which the platform never reads: what is decided about an option the
/// integration does not have (any more) is kept and applies again once it
/// does, and an option nothing was decided about offers every value - but
/// those the integration marks as guarded, which only the password opens -
/// with the integration's default.
/// </para>
/// <para>
/// The password is not among them: it is the installation's
/// (<c>LAMBDA_AGENT_PASSWORD</c>), kept where its other secrets are, and the
/// panel says only whether there is one.
/// </para>
/// </remarks>
/// <param name="Builds">Whether the workspace on /build builds new apps. Off, it only explains how to connect an agent of one's own.</param>
/// <param name="Changes">Whether owners may ask the agent about their apps - in the workspace and the editor's Change section.</param>
/// <param name="Access">Who may choose each value, by option and value name; one not named here is as the integration offers it</param>
/// <param name="Defaults">The value each option takes where nobody chose, by option name; one not named here takes the integration's</param>
/// <param name="Limits">What a run may take, where the agent holds runs to limits; nothing of either for what the integration holds a run to by default</param>
/// <param name="GuardedLimits">What a run on a value behind the password may take; nothing of either for no limit</param>
public sealed record AgentSettings(bool Builds, bool Changes, IReadOnlyDictionary<string, IReadOnlyDictionary<string, OptionAccess>> Access,
                                   IReadOnlyDictionary<string, string> Defaults, LimitSetting Limits, LimitSetting GuardedLimits)
{

    /// <summary>
    /// What an installation decided before its operator decided anything:
    /// building and changing on, every option as its integration offers it,
    /// runs held to what the agent holds them to by default - and those
    /// behind the password to nothing.
    /// </summary>
    public static readonly AgentSettings Default = new(true, true, new Dictionary<string, IReadOnlyDictionary<string, OptionAccess>>(),
                                                       new Dictionary<string, string>(), LimitSetting.Agents, LimitSetting.Agents);

    /// <summary>
    /// Who may choose a value, as decided here or - where nothing was - as the
    /// integration offers it.
    /// </summary>
    public OptionAccess AccessOf(AgentOption option, AgentOptionValue value)
        => Access.TryGetValue(option.Name, out var values) && values.TryGetValue(value.Name, out var access)
            ? access
            : value.Guarded ? OptionAccess.Password : OptionAccess.Open;

}
